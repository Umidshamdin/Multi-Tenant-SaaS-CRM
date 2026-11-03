using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesFlow.Application.Common.Pagination;
using SalesFlow.Application.Features.Leads;
using SalesFlow.Domain.Enums;

namespace SalesFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/leads")]
public sealed class LeadsController : ControllerBase
{
    private readonly LeadService _service;
    public LeadsController(LeadService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] LeadStatus? status = null, CancellationToken ct = default) => Ok(await _service.ListAsync(new PageRequest(page, pageSize), status, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct) => Ok(await _service.GetAsync(id, ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateLeadRequest request, CancellationToken ct) => StatusCode(201, await _service.CreateAsync(request, ct));

    [HttpPatch("{id:guid}/contact")]
    public async Task<IActionResult> Contact(Guid id, CancellationToken ct) => Ok(await _service.MarkContactedAsync(id, ct));

    [HttpPatch("{id:guid}/qualify")]
    public async Task<IActionResult> Qualify(Guid id, CancellationToken ct) => Ok(await _service.QualifyAsync(id, ct));

    [HttpPatch("{id:guid}/lost")]
    public async Task<IActionResult> Lost(Guid id, CancellationToken ct) => Ok(await _service.MarkLostAsync(id, ct));

    [HttpPost("{id:guid}/convert")]
    public async Task<IActionResult> Convert(Guid id, CancellationToken ct) => Ok(await _service.ConvertAsync(id, ct));
}
