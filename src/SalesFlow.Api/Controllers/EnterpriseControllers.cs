using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesFlow.Application.Common.Pagination;
using SalesFlow.Application.Features.Enterprise;
using SalesFlow.Domain.Enums;

namespace SalesFlow.Api.Controllers;

[ApiController, Authorize, Route("api/catalog/products")]
public sealed class EnterpriseProductsController : ControllerBase
{
    private readonly EnterpriseProductService _service;
    public EnterpriseProductsController(EnterpriseProductService service) => _service = service;
    [HttpGet] public async Task<IActionResult> List(int page = 1, int pageSize = 20, CancellationToken ct = default) => Ok(await _service.List(new PageRequest(page, pageSize), ct));
    [HttpPost] public async Task<IActionResult> Create(CreateProductRequest req, CancellationToken ct) => StatusCode(201, await _service.Create(req, ct));
    [HttpPut("{id:guid}")] public async Task<IActionResult> Update(Guid id, CreateProductRequest req, CancellationToken ct) => Ok(await _service.Update(id, req, ct));
    [HttpPatch("{id:guid}/archive")] public async Task<IActionResult> Archive(Guid id, CancellationToken ct) => Ok(await _service.Archive(id, ct));
}

[ApiController, Authorize, Route("api/deals/{dealId:guid}/products")]
public sealed class EnterpriseDealProductsController : ControllerBase
{
    private readonly EnterpriseDealProductService _service;
    public EnterpriseDealProductsController(EnterpriseDealProductService service) => _service = service;
    [HttpGet] public async Task<IActionResult> List(Guid dealId, int page = 1, int pageSize = 20, CancellationToken ct = default) => Ok(await _service.List(dealId, new PageRequest(page, pageSize), ct));
    [HttpPost] public async Task<IActionResult> Add(Guid dealId, CreateDealProductRequest req, CancellationToken ct) => StatusCode(201, await _service.Add(dealId, req, ct));
    [HttpDelete("{lineId:guid}")] public async Task<IActionResult> Remove(Guid dealId, Guid lineId, CancellationToken ct) => Ok(await _service.Remove(dealId, lineId, ct));
}

[ApiController, Authorize, Route("api/quotes")]
public sealed class EnterpriseQuotesController : ControllerBase
{
    private readonly EnterpriseQuoteService _service;
    public EnterpriseQuotesController(EnterpriseQuoteService service) => _service = service;
    [HttpGet] public async Task<IActionResult> List(int page = 1, int pageSize = 20, CancellationToken ct = default) => Ok(await _service.List(new PageRequest(page, pageSize), ct));
    [HttpGet("{id:guid}")] public async Task<IActionResult> Get(Guid id, CancellationToken ct) => Ok(await _service.Get(id, ct));
    [HttpPost] public async Task<IActionResult> Create(CreateQuoteRequest req, CancellationToken ct) => StatusCode(201, await _service.Create(req, ct));
    [HttpGet("{id:guid}/lines")] public async Task<IActionResult> Lines(Guid id, int page = 1, int pageSize = 20, CancellationToken ct = default) => Ok(await _service.Lines(id, new PageRequest(page, pageSize), ct));
    [HttpPost("{id:guid}/lines")] public async Task<IActionResult> AddLine(Guid id, AddQuoteLineRequest req, CancellationToken ct) => StatusCode(201, await _service.AddLine(id, req, ct));
    [HttpPatch("{id:guid}/send")] public async Task<IActionResult> Send(Guid id, CancellationToken ct) => Ok(await _service.SetStatus(id, QuoteStatus.Sent, ct));
    [HttpPatch("{id:guid}/accept")] public async Task<IActionResult> Accept(Guid id, CancellationToken ct) => Ok(await _service.SetStatus(id, QuoteStatus.Accepted, ct));
    [HttpPatch("{id:guid}/reject")] public async Task<IActionResult> Reject(Guid id, CancellationToken ct) => Ok(await _service.SetStatus(id, QuoteStatus.Rejected, ct));
}

[ApiController, Authorize, Route("api/orders")]
public sealed class EnterpriseOrdersController : ControllerBase
{
    private readonly EnterpriseBillingService _service;
    public EnterpriseOrdersController(EnterpriseBillingService service) => _service = service;
    [HttpGet] public async Task<IActionResult> List(int page = 1, int pageSize = 20, CancellationToken ct = default) => Ok(await _service.Orders(new PageRequest(page, pageSize), ct));
    [HttpPost] public async Task<IActionResult> Create(CreateOrderRequest req, CancellationToken ct) => StatusCode(201, await _service.CreateOrder(req, ct));
    [HttpPatch("{id:guid}/fulfill")] public async Task<IActionResult> Fulfill(Guid id, CancellationToken ct) => Ok(await _service.ChangeOrder(id, true, ct));
    [HttpPatch("{id:guid}/cancel")] public async Task<IActionResult> Cancel(Guid id, CancellationToken ct) => Ok(await _service.ChangeOrder(id, false, ct));
}

[ApiController, Authorize, Route("api/invoices")]
public sealed class EnterpriseInvoicesController : ControllerBase
{
    private readonly EnterpriseBillingService _service;
    public EnterpriseInvoicesController(EnterpriseBillingService service) => _service = service;
    [HttpGet] public async Task<IActionResult> List(int page = 1, int pageSize = 20, CancellationToken ct = default) => Ok(await _service.Invoices(new PageRequest(page, pageSize), ct));
    [HttpPost] public async Task<IActionResult> Create(CreateInvoiceRequest req, CancellationToken ct) => StatusCode(201, await _service.CreateInvoice(req, ct));
    [HttpPatch("{id:guid}/issue")] public async Task<IActionResult> Issue(Guid id, CancellationToken ct) => Ok(await _service.SetInvoiceStatus(id, true, ct));
    [HttpPatch("{id:guid}/void")] public async Task<IActionResult> Void(Guid id, CancellationToken ct) => Ok(await _service.SetInvoiceStatus(id, false, ct));
    [HttpGet("{id:guid}/payments")] public async Task<IActionResult> Payments(Guid id, int page = 1, int pageSize = 20, CancellationToken ct = default) => Ok(await _service.Payments(id, new PageRequest(page, pageSize), ct));
    [HttpPost("{id:guid}/payments")] public async Task<IActionResult> Pay(Guid id, RecordPaymentRequest req, CancellationToken ct) => StatusCode(201, await _service.RecordPayment(id, req, ct));
}

[ApiController, Authorize, Route("api/pipelines")]
public sealed class EnterprisePipelinesController : ControllerBase
{
    private readonly EnterprisePipelineService _service;
    public EnterprisePipelinesController(EnterprisePipelineService service) => _service = service;
    [HttpGet] public async Task<IActionResult> List(int page = 1, int pageSize = 20, CancellationToken ct = default) => Ok(await _service.List(new PageRequest(page, pageSize), ct));
    [HttpPost] public async Task<IActionResult> Create(CreatePipelineRequest req, CancellationToken ct) => StatusCode(201, await _service.Create(req, ct));
    [HttpGet("{id:guid}/stages")] public async Task<IActionResult> Stages(Guid id, int page = 1, int pageSize = 20, CancellationToken ct = default) => Ok(await _service.Stages(id, new PageRequest(page, pageSize), ct));
    [HttpPost("{id:guid}/stages")] public async Task<IActionResult> AddStage(Guid id, CreatePipelineStageRequest req, CancellationToken ct) => StatusCode(201, await _service.CreateStage(id, req, ct));
    [HttpPut("deals/{dealId:guid}")] public async Task<IActionResult> Place(Guid dealId, PlaceDealRequest req, CancellationToken ct) => Ok(await _service.PlaceDeal(dealId, req, ct));
}

[ApiController, Authorize, Route("api/crm-notes")]
public sealed class EnterpriseNotesController : ControllerBase
{
    private readonly EnterpriseCollaborationService _service;
    public EnterpriseNotesController(EnterpriseCollaborationService service) => _service = service;
    [HttpGet] public async Task<IActionResult> List(CrmRecordKind recordKind, Guid recordId, int page = 1, int pageSize = 20, CancellationToken ct = default)
        => Ok(await _service.Notes(recordKind, recordId, new PageRequest(page, pageSize), ct));
    [HttpPost] public async Task<IActionResult> Create(CreateNoteRequest req, CancellationToken ct) => StatusCode(201, await _service.AddNote(req, ct));
    [HttpPut("{id:guid}")] public async Task<IActionResult> Edit(Guid id, EditNoteRequest req, CancellationToken ct) => Ok(await _service.EditNote(id, req, ct));
}

[ApiController, Authorize, Route("api/crm-tags")]
public sealed class EnterpriseTagsController : ControllerBase
{
    private readonly EnterpriseCollaborationService _service;
    public EnterpriseTagsController(EnterpriseCollaborationService service) => _service = service;
    [HttpPost] public async Task<IActionResult> Create(CreateTagRequest req, CancellationToken ct) => StatusCode(201, await _service.CreateTag(req, ct));
    [HttpPost("assign")] public async Task<IActionResult> Assign(AssignTagRequest req, CancellationToken ct) => Ok(await _service.AssignTag(req, ct));
    [HttpGet("records")] public async Task<IActionResult> For(CrmRecordKind recordKind, Guid recordId, int page = 1, int pageSize = 20, CancellationToken ct = default)
        => Ok(await _service.TagsFor(recordKind, recordId, new PageRequest(page, pageSize), ct));
}

[ApiController, Authorize, Route("api/custom-fields")]
public sealed class EnterpriseCustomFieldsController : ControllerBase
{
    private readonly EnterpriseCustomFieldService _service;
    public EnterpriseCustomFieldsController(EnterpriseCustomFieldService service) => _service = service;
    [HttpPost] public async Task<IActionResult> Create(CreateCustomFieldRequest req, CancellationToken ct) => StatusCode(201, await _service.Define(req, ct));
    [HttpPut("values")] public async Task<IActionResult> Set(SetCustomValueRequest req, CancellationToken ct) => Ok(await _service.Set(req, ct));
    [HttpGet("values")] public async Task<IActionResult> Values(CrmRecordKind recordKind, Guid recordId, int page = 1, int pageSize = 20, CancellationToken ct = default)
        => Ok(await _service.Values(recordKind, recordId, new PageRequest(page, pageSize), ct));
}

[ApiController, Authorize, Route("api/sales-goals")]
public sealed class EnterpriseGoalsController : ControllerBase
{
    private readonly EnterpriseGoalService _service;
    public EnterpriseGoalsController(EnterpriseGoalService service) => _service = service;
    [HttpGet] public async Task<IActionResult> List(int page = 1, int pageSize = 20, CancellationToken ct = default) => Ok(await _service.List(new PageRequest(page, pageSize), ct));
    [HttpPost] public async Task<IActionResult> Create(CreateSalesGoalRequest req, CancellationToken ct) => StatusCode(201, await _service.Create(req, ct));
    [HttpPut("{id:guid}")] public async Task<IActionResult> Update(Guid id, ChangeSalesGoalRequest req, CancellationToken ct) => Ok(await _service.Update(id, req, ct));
}

[ApiController, Authorize, Route("api/sales-dashboard")]
public sealed class EnterpriseDashboardController : ControllerBase
{
    private readonly EnterpriseDashboardService _service;
    public EnterpriseDashboardController(EnterpriseDashboardService service) => _service = service;
    [HttpGet] public async Task<IActionResult> Get(CancellationToken ct) => Ok(await _service.Get(ct));
}

[ApiController, Authorize, Route("api/crm-audit")]
public sealed class EnterpriseAuditController : ControllerBase
{
    private readonly EnterpriseAuditService _service;
    public EnterpriseAuditController(EnterpriseAuditService service) => _service = service;
    [HttpGet] public async Task<IActionResult> List(int page = 1, int pageSize = 20, CancellationToken ct = default) => Ok(await _service.List(new PageRequest(page, pageSize), ct));
}
