using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesFlow.Application.Common.Pagination;
using SalesFlow.Application.Features.Products;

namespace SalesFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    public ProductsController(IProductService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] bool? active = null, CancellationToken ct = default)
        => Ok(await _service.ListAsync(new PageRequest(page, pageSize), search, active, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct) => Ok(await _service.GetAsync(id, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var product = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [HttpPatch("{id:guid}/archive")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct) => Ok(await _service.ArchiveAsync(id, ct));
}
