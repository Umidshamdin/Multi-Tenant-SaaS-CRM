using SalesFlow.Application.Abstractions;
using SalesFlow.Application.Common.Errors;
using SalesFlow.Application.Common.Pagination;
using SalesFlow.Application.Security;
using SalesFlow.Domain.Common;
using SalesFlow.Domain.Entities;
using SalesFlow.Domain.Enums;
using SalesFlow.Domain.Exceptions;

namespace SalesFlow.Application.Features.Enterprise;

public record CreateProductRequest(string Sku, string Name, string? Description, decimal UnitPrice, string Currency);
public record CreateDealProductRequest(Guid ProductId, decimal Quantity, decimal DiscountPercent, decimal TaxPercent);
public record CreateQuoteRequest(Guid DealId, string Number, DateOnly ValidUntil);
public record AddQuoteLineRequest(Guid ProductId, decimal Quantity, decimal DiscountPercent, decimal TaxPercent);
public record CreateOrderRequest(Guid QuoteId, string Number);
public record CreateInvoiceRequest(Guid OrderId, string Number, DateOnly DueDate);
public record RecordPaymentRequest(string Reference, decimal Amount);
public record CreatePipelineRequest(string Name);
public record CreatePipelineStageRequest(string Name, DealStage Kind, int Position, int ProbabilityPercent);
public record PlaceDealRequest(Guid PipelineId, Guid StageId);
public record CreateNoteRequest(CrmRecordKind RecordKind, Guid RecordId, string Body);
public record EditNoteRequest(string Body);
public record CreateTagRequest(string Name, string? Color);
public record AssignTagRequest(Guid TagId, CrmRecordKind RecordKind, Guid RecordId);
public record CreateCustomFieldRequest(CrmRecordKind RecordKind, string Key, string Label, CustomFieldType Type);
public record SetCustomValueRequest(Guid FieldId, Guid RecordId, string Value);
public record CreateSalesGoalRequest(Guid MemberId, int Year, int Month, string Currency, decimal TargetAmount);
public record ChangeSalesGoalRequest(decimal TargetAmount);

public abstract class EnterpriseServiceBase
{
    protected readonly AccessGuard Guard;
    protected readonly IUnitOfWork UnitOfWork;
    private readonly ITenantRepository<CrmAuditEvent> _audit;
    protected EnterpriseServiceBase(AccessGuard guard, IUnitOfWork unitOfWork, ITenantRepository<CrmAuditEvent> audit)
    {
        Guard = guard;
        UnitOfWork = unitOfWork;
        _audit = audit;
    }
    protected static async Task<T> Required<T>(ITenantRepository<T> repo, Guid tenantId, Guid id, CancellationToken ct) where T : TenantEntity =>
        await repo.GetAsync(tenantId, id, ct) ?? throw new NotFoundException($"{typeof(T).Name} not found.");
    protected async Task Audit(AccessContext access, string action, string entityType, Guid entityId, string details, CancellationToken ct) =>
        await _audit.AddAsync(new CrmAuditEvent(access.TenantId, access.MemberId, action, entityType, entityId, details), ct);
}

public sealed class EnterpriseProductService : EnterpriseServiceBase
{
    private readonly ITenantRepository<Product> _products;
    public EnterpriseProductService(AccessGuard guard, IUnitOfWork uow, ITenantRepository<CrmAuditEvent> audit, ITenantRepository<Product> products)
        : base(guard, uow, audit) => _products = products;
    public async Task<PagedResult<Product>> List(PageRequest page, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        return await _products.PageAsync(a.TenantId, page, null, ct);
    }
    public async Task<Product> Create(CreateProductRequest request, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        if (request.UnitPrice != decimal.Round(request.UnitPrice, 2, MidpointRounding.AwayFromZero))
            throw new ValidationException("Product price must have at most two decimal places.");
        var p = new Product(a.TenantId, request.Sku, request.Name, request.Description, request.UnitPrice, request.Currency);
        if (await _products.AnyAsync(a.TenantId, x => x.Sku == p.Sku, ct)) throw new ConflictException("SKU already exists.");
        await _products.AddAsync(p, ct);
        await Audit(a, "create", nameof(Product), p.Id, "Product created", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return p;
    }
    public async Task<Product> Update(Guid id, CreateProductRequest request, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        var p = await Required(_products, a.TenantId, id, ct);
        var normalizedSku = request.Sku?.Trim().ToUpperInvariant() ?? string.Empty;
        if (await _products.AnyAsync(a.TenantId, x => x.Id != id && x.Sku == normalizedSku, ct)) throw new ConflictException("SKU already exists.");
        if (request.UnitPrice != decimal.Round(request.UnitPrice, 2, MidpointRounding.AwayFromZero))
            throw new ValidationException("Product price must have at most two decimal places.");
        p.Update(request.Sku, request.Name, request.Description, request.UnitPrice, request.Currency);
        await Audit(a, "update", nameof(Product), id, "Product updated", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return p;
    }
    public async Task<Product> Archive(Guid id, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        var p = await Required(_products, a.TenantId, id, ct);
        p.Archive();
        await Audit(a, "archive", nameof(Product), id, "Product archived", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return p;
    }
}

public sealed class EnterpriseDealProductService : EnterpriseServiceBase
{
    private readonly ITenantRepository<DealProduct> _lines;
    private readonly ITenantRepository<Product> _products;
    private readonly ITenantRepository<Deal> _deals;
    public EnterpriseDealProductService(AccessGuard g, IUnitOfWork u, ITenantRepository<CrmAuditEvent> a,
        ITenantRepository<DealProduct> lines, ITenantRepository<Product> products, ITenantRepository<Deal> deals) : base(g, u, a)
        => (_lines, _products, _deals) = (lines, products, deals);
    public async Task<PagedResult<DealProduct>> List(Guid dealId, PageRequest page, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        await Required(_deals, a.TenantId, dealId, ct);
        return await _lines.PageAsync(a.TenantId, page, x => x.DealId == dealId && x.IsActive, ct);
    }
    public async Task<DealProduct> Add(Guid dealId, CreateDealProductRequest req, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        var deal = await Required(_deals, a.TenantId, dealId, ct);
        var product = await Required(_products, a.TenantId, req.ProductId, ct);
        if (deal.Stage is DealStage.Won or DealStage.Lost) throw new ConflictException("Closed deals cannot be edited.");
        if (!string.Equals(deal.Currency, product.Currency, StringComparison.OrdinalIgnoreCase))
            throw new ConflictException("Deal and product currencies differ. Currency conversion is not configured.");
        var line = new DealProduct(a.TenantId, dealId, product, req.Quantity, req.DiscountPercent, req.TaxPercent);
        await _lines.AddAsync(line, ct);
        await Audit(a, "add-line", nameof(Deal), dealId, "Product attached to deal", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return line;
    }
    public async Task<DealProduct> Remove(Guid dealId, Guid lineId, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        var deal = await Required(_deals, a.TenantId, dealId, ct);
        if (deal.Stage is DealStage.Won or DealStage.Lost) throw new ConflictException("Closed deals cannot be edited.");
        var line = await Required(_lines, a.TenantId, lineId, ct);
        if (line.DealId != dealId) throw new NotFoundException("Deal product not found.");
        line.Remove();
        await Audit(a, "remove-line", nameof(Deal), dealId, "Product removed from deal", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return line;
    }
}

public sealed class EnterpriseQuoteService : EnterpriseServiceBase
{
    private readonly ITenantRepository<SalesQuote> _quotes;
    private readonly ITenantRepository<SalesQuoteLine> _lines;
    private readonly ITenantRepository<Deal> _deals;
    private readonly ITenantRepository<Customer> _customers;
    private readonly ITenantRepository<Product> _products;
    public EnterpriseQuoteService(AccessGuard g, IUnitOfWork u, ITenantRepository<CrmAuditEvent> a,
        ITenantRepository<SalesQuote> quotes, ITenantRepository<SalesQuoteLine> lines, ITenantRepository<Deal> deals,
        ITenantRepository<Customer> customers, ITenantRepository<Product> products) : base(g, u, a)
        => (_quotes, _lines, _deals, _customers, _products) = (quotes, lines, deals, customers, products);
    public async Task<PagedResult<SalesQuote>> List(PageRequest page, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        return await _quotes.PageAsync(a.TenantId, page, null, ct);
    }
    public async Task<SalesQuote> Get(Guid id, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        return await Required(_quotes, a.TenantId, id, ct);
    }
    public async Task<PagedResult<SalesQuoteLine>> Lines(Guid quoteId, PageRequest page, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        await Required(_quotes, a.TenantId, quoteId, ct);
        return await _lines.PageAsync(a.TenantId, page, x => x.QuoteId == quoteId, ct);
    }
    public async Task<SalesQuote> Create(CreateQuoteRequest req, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        var deal = await Required(_deals, a.TenantId, req.DealId, ct);
        if (deal.Stage is DealStage.Won or DealStage.Lost) throw new ConflictException("Cannot quote a closed deal.");
        var customer = await Required(_customers, a.TenantId, deal.CustomerId, ct);
        if (!customer.IsActive) throw new ConflictException("Customer is inactive.");
        var q = new SalesQuote(a.TenantId, deal.Id, customer.Id, req.Number, deal.Currency, req.ValidUntil);
        if (await _quotes.AnyAsync(a.TenantId, x => x.Number == q.Number, ct)) throw new ConflictException("Quote number already exists.");
        await _quotes.AddAsync(q, ct);
        await Audit(a, "create", nameof(SalesQuote), q.Id, "Quote created", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return q;
    }
    public async Task<SalesQuoteLine> AddLine(Guid quoteId, AddQuoteLineRequest req, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        var q = await Required(_quotes, a.TenantId, quoteId, ct);
        var p = await Required(_products, a.TenantId, req.ProductId, ct);
        if (q.Currency != p.Currency) throw new ConflictException("Quote and product currencies differ.");
        // The quote is the write aggregate: totals and line are committed in a single transaction.
        var line = new SalesQuoteLine(a.TenantId, q.Id, p, req.Quantity, req.DiscountPercent, req.TaxPercent);
        q.AddAmounts(line.NetAmount, line.TaxAmount);
        await _lines.AddAsync(line, ct);
        await Audit(a, "add-line", nameof(SalesQuote), q.Id, "Line added to draft quote", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return line;
    }
    public async Task<SalesQuote> SetStatus(Guid id, QuoteStatus status, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        var q = await Required(_quotes, a.TenantId, id, ct);
        switch (status)
        {
            case QuoteStatus.Sent: q.Send(); break;
            case QuoteStatus.Accepted: q.Accept(); break;
            case QuoteStatus.Rejected: q.Reject(); break;
            default: throw new DomainException("Unsupported quote transition.");
        }
        await Audit(a, "status", nameof(SalesQuote), id, $"Quote changed to {status}", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return q;
    }
}

public sealed class EnterpriseBillingService : EnterpriseServiceBase
{
    private readonly ITenantRepository<SalesQuote> _quotes;
    private readonly ITenantRepository<SalesOrder> _orders;
    private readonly ITenantRepository<SalesInvoice> _invoices;
    private readonly ITenantRepository<SalesPayment> _payments;
    public EnterpriseBillingService(AccessGuard g, IUnitOfWork u, ITenantRepository<CrmAuditEvent> a,
        ITenantRepository<SalesQuote> quotes, ITenantRepository<SalesOrder> orders,
        ITenantRepository<SalesInvoice> invoices, ITenantRepository<SalesPayment> payments) : base(g, u, a)
        => (_quotes, _orders, _invoices, _payments) = (quotes, orders, invoices, payments);
    public async Task<SalesOrder> CreateOrder(CreateOrderRequest req, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        var q = await Required(_quotes, a.TenantId, req.QuoteId, ct);
        if (await _orders.AnyAsync(a.TenantId, x => x.QuoteId == q.Id || x.Number == req.Number, ct))
            throw new ConflictException("An order for this quote or number already exists.");
        var order = new SalesOrder(a.TenantId, q, req.Number);
        await _orders.AddAsync(order, ct);
        await Audit(a, "create", nameof(SalesOrder), order.Id, "Order created from accepted quote", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return order;
    }
    public async Task<SalesOrder> ChangeOrder(Guid id, bool fulfill, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        var o = await Required(_orders, a.TenantId, id, ct);
        // Cancelling after an invoice exists is prohibited to avoid inconsistent billing.
        if (!fulfill && await _invoices.AnyAsync(a.TenantId, x => x.OrderId == id, ct))
            throw new ConflictException("An invoiced order cannot be cancelled.");
        if (fulfill) o.Fulfill(); else o.Cancel();
        await Audit(a, "status", nameof(SalesOrder), id, fulfill ? "Order fulfilled" : "Order cancelled", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return o;
    }
    public async Task<SalesInvoice> CreateInvoice(CreateInvoiceRequest req, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        var o = await Required(_orders, a.TenantId, req.OrderId, ct);
        if (await _invoices.AnyAsync(a.TenantId, x => x.OrderId == o.Id || x.Number == req.Number, ct))
            throw new ConflictException("An invoice for this order or number already exists.");
        var invoice = new SalesInvoice(a.TenantId, o, req.Number, req.DueDate);
        await _invoices.AddAsync(invoice, ct);
        await Audit(a, "create", nameof(SalesInvoice), invoice.Id, "Invoice created from order", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return invoice;
    }
    public async Task<SalesInvoice> SetInvoiceStatus(Guid id, bool issue, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        var invoice = await Required(_invoices, a.TenantId, id, ct);
        if (issue) invoice.Issue(); else invoice.Void();
        await Audit(a, "status", nameof(SalesInvoice), id, issue ? "Invoice issued" : "Invoice voided", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return invoice;
    }
    public async Task<SalesPayment> RecordPayment(Guid invoiceId, RecordPaymentRequest req, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        var invoice = await Required(_invoices, a.TenantId, invoiceId, ct);
        var reference = req.Reference?.Trim() ?? string.Empty;
        if (await _payments.AnyAsync(a.TenantId, x => x.Reference == reference, ct))
            throw new ConflictException("Payment reference has already been recorded.");
        var payment = new SalesPayment(a.TenantId, invoice.Id, req.Reference, req.Amount);
        invoice.AddPayment(payment.Amount);
        await _payments.AddAsync(payment, ct);
        await Audit(a, "payment", nameof(SalesInvoice), invoice.Id, $"Payment recorded: {payment.Reference}", ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return payment;
    }
    public async Task<PagedResult<SalesOrder>> Orders(PageRequest page, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        return await _orders.PageAsync(a.TenantId, page, null, ct);
    }
    public async Task<PagedResult<SalesInvoice>> Invoices(PageRequest page, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        return await _invoices.PageAsync(a.TenantId, page, null, ct);
    }
    public async Task<PagedResult<SalesPayment>> Payments(Guid invoiceId, PageRequest page, CancellationToken ct)
    {
        var a = await Guard.RequireAsync(ct);
        await Required(_invoices, a.TenantId, invoiceId, ct);
        return await _payments.PageAsync(a.TenantId, page, x => x.InvoiceId == invoiceId, ct);
    }
}
