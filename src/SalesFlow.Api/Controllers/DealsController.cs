using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesFlow.Application.Common.Pagination;
using SalesFlow.Application.Features.Deals;
using SalesFlow.Domain.Enums;

namespace SalesFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/deals")]
public sealed class DealsController : ControllerBase
{
    private readonly DealService _service;
    public DealsController(DealService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? customerId = null, [FromQuery] DealStage? stage = null, CancellationToken ct = default) => Ok(await _service.ListAsync(new PageRequest(page, pageSize), customerId, stage, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct) => Ok(await _service.GetAsync(id, ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateDealRequest request, CancellationToken ct) => StatusCode(201, await _service.CreateAsync(request, ct));

    [HttpPut("{id:guid}/title")]
    public async Task<IActionResult> Title(Guid id, UpdateDealTitleRequest request, CancellationToken ct) => Ok(await _service.UpdateTitleAsync(id, request, ct));

    [HttpPut("{id:guid}/amount")]
    public async Task<IActionResult> Amount(Guid id, ChangeDealAmountRequest request, CancellationToken ct) => Ok(await _service.ChangeAmountAsync(id, request, ct));

    [HttpPatch("{id:guid}/stage/{stage}")]
    public async Task<IActionResult> Stage(Guid id, DealStage stage, CancellationToken ct) => Ok(await _service.MoveAsync(id, stage, ct));

    [HttpPatch("{id:guid}/won")]
    public async Task<IActionResult> Won(Guid id, CancellationToken ct) => Ok(await _service.MarkWonAsync(id, ct));

    [HttpPatch("{id:guid}/lost")]
    public async Task<IActionResult> Lost(Guid id, CancellationToken ct) => Ok(await _service.MarkLostAsync(id, ct));
}
