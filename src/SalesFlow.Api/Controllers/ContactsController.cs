using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesFlow.Application.Common.Pagination;
using SalesFlow.Application.Features.Customers;

namespace SalesFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/contacts")]
public sealed class ContactsController : ControllerBase
{
    private readonly ContactService _service;
    public ContactsController(ContactService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid customerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) => Ok(await _service.ListAsync(customerId, new PageRequest(page, pageSize), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct) => Ok(await _service.GetAsync(id, ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateContactRequest request, CancellationToken ct) => StatusCode(201, await _service.CreateAsync(request, ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateContactRequest request, CancellationToken ct) => Ok(await _service.UpdateAsync(id, request, ct));
}
