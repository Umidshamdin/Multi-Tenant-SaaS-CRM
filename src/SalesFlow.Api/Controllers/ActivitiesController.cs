using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesFlow.Application.Common.Pagination;
using SalesFlow.Application.Features.Activities;
using SalesFlow.Domain.Enums;

namespace SalesFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/activities")]
public sealed class ActivitiesController : ControllerBase
{
    private readonly ActivityService _service;

    public ActivitiesController(ActivityService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? dealId = null,
        [FromQuery] ActivityKind? kind = null,
        CancellationToken ct = default)
    {
        var result = await _service.ListAsync(
            new PageRequest(page, pageSize),
            customerId,
            dealId,
            kind,
            ct);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateActivityRequest request,
        CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }
}
