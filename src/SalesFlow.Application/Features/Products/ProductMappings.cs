using SalesFlow.Domain.Entities;

namespace SalesFlow.Application.Features.Products;

public static class ProductMappings
{
    public static ProductDto ToDto(this Product product) => new(product.Id, product.TenantId, product.Sku,
        product.Name, product.Description, product.UnitPrice, product.Currency, product.IsActive,
        product.CreatedAt, product.UpdatedAt);
}
