using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesFlow.Api.Models;
using SalesFlow.Application.Common.Pagination;
using SalesFlow.Application.Features.Members;

namespace SalesFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/members")]
public sealed class MembersController : ControllerBase
{
    private readonly MemberService _service;
    public MembersController(MemberService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool? active = null, CancellationToken ct = default) => Ok(await _service.ListAsync(new PageRequest(page, pageSize), active, ct));

    [HttpPost]
    public async Task<IActionResult> Add(AddMemberRequest request, CancellationToken ct) => StatusCode(201, await _service.AddExistingUserAsync(request, ct));

    [HttpPut("{id:guid}/role")]
    public async Task<IActionResult> ChangeRole(Guid id, ChangeMemberRoleRequest request, CancellationToken ct) => Ok(await _service.ChangeRoleAsync(id, request, ct));

    [HttpPut("{id:guid}/active")]
    public async Task<IActionResult> SetActive(Guid id, SetActiveRequest request, CancellationToken ct) => Ok(await _service.SetActiveAsync(id, request.Active, ct));
}
