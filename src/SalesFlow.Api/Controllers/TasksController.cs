using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesFlow.Application.Common.Pagination;
using SalesFlow.Application.Features.Tasks;
using SalesFlow.Domain.Enums;

namespace SalesFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public sealed class TasksController : ControllerBase
{
    private readonly TaskService _service;
    public TasksController(TaskService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] SalesTaskStatus? status = null, [FromQuery] Guid? memberId = null, CancellationToken ct = default) => Ok(await _service.ListAsync(new PageRequest(page, pageSize), status, memberId, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct) => Ok(await _service.GetAsync(id, ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskRequest request, CancellationToken ct) => StatusCode(201, await _service.CreateAsync(request, ct));

    [HttpPut("{id:guid}/assign")]
    public async Task<IActionResult> Assign(Guid id, AssignTaskRequest request, CancellationToken ct) => Ok(await _service.AssignAsync(id, request, ct));

    [HttpPut("{id:guid}/due-date")]
    public async Task<IActionResult> DueDate(Guid id, SetTaskDueDateRequest request, CancellationToken ct) => Ok(await _service.SetDueDateAsync(id, request, ct));

    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct) => Ok(await _service.CompleteAsync(id, ct));
}
