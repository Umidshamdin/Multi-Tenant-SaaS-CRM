using SalesFlow.Domain.Common;
using SalesFlow.Domain.Enums;
using SalesFlow.Domain.Exceptions;

namespace SalesFlow.Domain.Entities;

public static class CommercialMath
{
    public static decimal Round(decimal amount) => decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    public static (decimal Net, decimal Tax, decimal Total) Calculate(decimal quantity, decimal unitPrice, decimal discountPercent, decimal taxPercent)
    {
        if (quantity <= 0 || quantity > 1_000_000) throw new DomainException("Quantity must be between 0 and 1,000,000.");
        if (unitPrice < 0 || unitPrice > 1_000_000_000) throw new DomainException("Invalid unit price.");
        if (discountPercent is < 0 or > 100 || taxPercent is < 0 or > 100) throw new DomainException("Discount and tax must be 0–100 percent.");
        var net = Round(quantity * unitPrice * (100 - discountPercent) / 100m);
        var tax = Round(net * taxPercent / 100m);
        return (net, tax, net + tax);
    }
}

public sealed class DealProduct : TenantEntity
{
    public Guid DealId { get; private set; }
    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Currency { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public decimal TaxPercent { get; private set; }
    public decimal NetAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public bool IsActive { get; private set; }
    private DealProduct() { }
    public DealProduct(Guid tenantId, Guid dealId, Product product, decimal quantity, decimal discountPercent, decimal taxPercent) : base(tenantId)
    {
        DomainRules.RequiredId(dealId, nameof(dealId));
        if (product.TenantId != tenantId || !product.IsActive) throw new DomainException("Product must belong to the tenant and be active.");
        DealId = dealId;
        ProductId = product.Id;
        Sku = product.Sku;
        Name = product.Name;
        Currency = product.Currency;
        UnitPrice = product.UnitPrice;
        Quantity = quantity;
        DiscountPercent = discountPercent;
        TaxPercent = taxPercent;
        var totals = CommercialMath.Calculate(quantity, UnitPrice, discountPercent, taxPercent);
        NetAmount = totals.Net;
        TaxAmount = totals.Tax;
        IsActive = true;
    }
    public void Remove()
    {
        if (!IsActive) throw new DomainException("Line is already removed.");
        IsActive = false;
        Touch();
    }
}

public sealed class SalesQuote : TenantEntity
{
    public Guid DealId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Number { get; private set; } = string.Empty;
    public string Currency { get; private set; } = string.Empty;
    public QuoteStatus Status { get; private set; }
    public DateOnly ValidUntil { get; private set; }
    public decimal NetAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount => NetAmount + TaxAmount;
    private SalesQuote() { }
    public SalesQuote(Guid tenantId, Guid dealId, Guid customerId, string number, string currency, DateOnly validUntil) : base(tenantId)
    {
        DomainRules.RequiredId(dealId, nameof(dealId));
        DomainRules.RequiredId(customerId, nameof(customerId));
        if (validUntil < DateOnly.FromDateTime(DateTime.Now)) throw new DomainException("Quote expiration date is in the past.");
        DealId = dealId;
        CustomerId = customerId;
        Number = DomainRules.Required(number, nameof(Number), 50);
        Currency = DomainRules.Currency(currency);
        ValidUntil = validUntil;
        Status = QuoteStatus.Draft;
    }
    public void AddAmounts(decimal netAmount, decimal taxAmount)
    {
        if (Status != QuoteStatus.Draft) throw new DomainException("Only draft quotes can be changed.");
        if (netAmount < 0 || taxAmount < 0 || NetAmount + netAmount + TaxAmount + taxAmount > 999_999_999_999m)
            throw new DomainException("Invalid quote amounts.");
        NetAmount += netAmount;
        TaxAmount += taxAmount;
        Touch();
    }
    public void Send()
    {
        if (Status != QuoteStatus.Draft || TotalAmount <= 0) throw new DomainException("Only a non-empty draft quote can be sent.");
        if (ValidUntil < DateOnly.FromDateTime(DateTime.Now)) throw new DomainException("Quote has expired.");
        Status = QuoteStatus.Sent;
        Touch();
    }
    public void Accept()
    {
        if (Status != QuoteStatus.Sent) throw new DomainException("Only sent quotes can be accepted.");
        if (ValidUntil < DateOnly.FromDateTime(DateTime.Now)) throw new DomainException("Quote has expired.");
        Status = QuoteStatus.Accepted;
        Touch();
    }
    public void Reject()
    {
        if (Status != QuoteStatus.Sent) throw new DomainException("Only sent quotes can be rejected.");
        Status = QuoteStatus.Rejected;
        Touch();
    }
}

public sealed class SalesQuoteLine : TenantEntity
{
    public Guid QuoteId { get; private set; }
    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public decimal TaxPercent { get; private set; }
    public decimal NetAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount => NetAmount + TaxAmount;
    private SalesQuoteLine() { }
    public SalesQuoteLine(Guid tenantId, Guid quoteId, Product product, decimal quantity, decimal discountPercent, decimal taxPercent) : base(tenantId)
    {
        DomainRules.RequiredId(quoteId, nameof(quoteId));
        if (product.TenantId != tenantId || !product.IsActive) throw new DomainException("Product must belong to the tenant and be active.");
        QuoteId = quoteId;
        ProductId = product.Id;
        Sku = product.Sku;
        Description = product.Name;
        Quantity = quantity;
        UnitPrice = product.UnitPrice;
        DiscountPercent = discountPercent;
        TaxPercent = taxPercent;
        var totals = CommercialMath.Calculate(quantity, UnitPrice, discountPercent, taxPercent);
        NetAmount = totals.Net;
        TaxAmount = totals.Tax;
    }
}

public sealed class SalesOrder : TenantEntity
{
    public Guid QuoteId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Number { get; private set; } = string.Empty;
    public string Currency { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public SalesOrderStatus Status { get; private set; }
    private SalesOrder() { }
    public SalesOrder(Guid tenantId, SalesQuote quote, string number) : base(tenantId)
    {
        if (quote.TenantId != tenantId || quote.Status != QuoteStatus.Accepted) throw new DomainException("An accepted quote from this tenant is required.");
        QuoteId = quote.Id;
        CustomerId = quote.CustomerId;
        Currency = quote.Currency;
        TotalAmount = quote.TotalAmount;
        Number = DomainRules.Required(number, nameof(Number), 50);
        Status = SalesOrderStatus.Pending;
    }
    public void Fulfill()
    {
        if (Status != SalesOrderStatus.Pending) throw new DomainException("Only pending orders may be fulfilled.");
        Status = SalesOrderStatus.Fulfilled;
        Touch();
    }
    public void Cancel()
    {
        if (Status != SalesOrderStatus.Pending) throw new DomainException("Only pending orders may be cancelled.");
        Status = SalesOrderStatus.Cancelled;
        Touch();
    }
}

public sealed class SalesInvoice : TenantEntity
{
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Number { get; private set; } = string.Empty;
    public string Currency { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public decimal OutstandingAmount => TotalAmount - PaidAmount;
    public InvoiceStatus Status { get; private set; }
    public DateOnly DueDate { get; private set; }
    private SalesInvoice() { }
    public SalesInvoice(Guid tenantId, SalesOrder order, string number, DateOnly dueDate) : base(tenantId)
    {
        if (order.TenantId != tenantId || order.Status == SalesOrderStatus.Cancelled) throw new DomainException("Cannot invoice a cancelled or foreign order.");
        if (dueDate < DateOnly.FromDateTime(DateTime.Now)) throw new DomainException("Invoice due date must not be in the past.");
        OrderId = order.Id;
        CustomerId = order.CustomerId;
        Number = DomainRules.Required(number, nameof(Number), 50);
        Currency = order.Currency;
        TotalAmount = order.TotalAmount;
        DueDate = dueDate;
        Status = InvoiceStatus.Draft;
    }
    public void Issue()
    {
        if (Status != InvoiceStatus.Draft || TotalAmount <= 0) throw new DomainException("Only a non-empty draft invoice can be issued.");
        Status = InvoiceStatus.Issued;
        Touch();
    }
    public void AddPayment(decimal amount)
    {
        if (Status != InvoiceStatus.Issued) throw new DomainException("Only issued invoices accept payments.");
        if (amount <= 0 || amount != CommercialMath.Round(amount) || amount > OutstandingAmount)
            throw new DomainException("Invalid payment amount.");
        PaidAmount += amount;
        if (PaidAmount == TotalAmount) Status = InvoiceStatus.Paid;
        Touch();
    }
    public void Void()
    {
        if (Status is not InvoiceStatus.Draft and not InvoiceStatus.Issued || PaidAmount > 0)
            throw new DomainException("Only unpaid draft or issued invoices can be voided.");
        Status = InvoiceStatus.Voided;
        Touch();
    }
}

public sealed class SalesPayment : TenantEntity
{
    public Guid InvoiceId { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateTime RecordedAt { get; private set; }
    private SalesPayment() { }
    public SalesPayment(Guid tenantId, Guid invoiceId, string reference, decimal amount) : base(tenantId)
    {
        DomainRules.RequiredId(invoiceId, nameof(invoiceId));
        if (amount <= 0 || amount != CommercialMath.Round(amount)) throw new DomainException("Payment amount must be a positive two-decimal amount.");
        InvoiceId = invoiceId;
        Reference = DomainRules.Required(reference, nameof(Reference), 100);
        Amount = amount;
        RecordedAt = DateTime.Now;
    }
}
