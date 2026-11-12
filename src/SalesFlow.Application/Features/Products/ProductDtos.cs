namespace SalesFlow.Application.Features.Products;

public sealed record CreateProductRequest(string Sku, string Name, string? Description, decimal UnitPrice, string Currency = "AZN");
public sealed record UpdateProductRequest(string Sku, string Name, string? Description, decimal UnitPrice, string Currency);
public sealed record ProductDto(Guid Id, Guid TenantId, string Sku, string Name, string? Description, decimal UnitPrice,
    string Currency, bool IsActive, DateTime CreatedAt, DateTime? UpdatedAt);
