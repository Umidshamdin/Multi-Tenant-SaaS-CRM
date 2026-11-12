using SalesFlow.Application.Common.Pagination;

namespace SalesFlow.Application.Features.Products;

public interface IProductService
{
    Task<PagedResult<ProductDto>> ListAsync(PageRequest page, string? search = null, bool? active = null, CancellationToken cancellationToken = default);
    Task<ProductDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDto> ArchiveAsync(Guid id, CancellationToken cancellationToken = default);
}
