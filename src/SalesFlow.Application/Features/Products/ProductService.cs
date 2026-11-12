using System.Linq.Expressions;
using SalesFlow.Application.Abstractions;
using SalesFlow.Application.Common.Errors;
using SalesFlow.Application.Common.Pagination;
using SalesFlow.Application.Security;
using SalesFlow.Domain.Entities;
using SalesFlow.Domain.Enums;

namespace SalesFlow.Application.Features.Products;

public sealed class ProductService : IProductService
{
    private readonly AccessGuard _guard;
    private readonly ITenantRepository<Product> _products;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(AccessGuard guard, ITenantRepository<Product> products, IUnitOfWork unitOfWork)
    {
        _guard = guard;
        _products = products;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ProductDto>> ListAsync(PageRequest page, string? search = null, bool? active = null, CancellationToken cancellationToken = default)
    {
        var access = await _guard.RequireAsync(cancellationToken);
        var term = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        var skuTerm = term?.ToUpperInvariant();
        Expression<Func<Product, bool>> filter = x =>
            (!active.HasValue || x.IsActive == active.Value) &&
            (term == null || x.Name.Contains(term) || x.Sku.Contains(skuTerm!));
        var result = await _products.PageAsync(access.TenantId, page, filter, cancellationToken);
        return result.Map(x => x.ToDto());
    }

    public async Task<ProductDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var access = await _guard.RequireAsync(cancellationToken);
        var product = await _products.GetAsync(access.TenantId, id, cancellationToken)
            ?? throw new NotFoundException("Product not found.");
        return product.ToDto();
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ValidationException("Product request is required.");
        var access = await _guard.RequireAsync(cancellationToken);
        ValidatePrice(request.UnitPrice);
        var product = new Product(access.TenantId, request.Sku, request.Name, request.Description, request.UnitPrice, request.Currency);
        if (await _products.AnyAsync(access.TenantId, x => x.Sku == product.Sku, cancellationToken))
            throw new ConflictException("A product with this SKU already exists in this tenant.");
        await _products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return product.ToDto();
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ValidationException("Product request is required.");
        var access = await _guard.RequireAsync(cancellationToken);
        var product = await _products.GetAsync(access.TenantId, id, cancellationToken)
            ?? throw new NotFoundException("Product not found.");
        if (!product.IsActive) throw new ConflictException("Archived product cannot be edited.");
        ValidatePrice(request.UnitPrice);
        product.Update(request.Sku, request.Name, request.Description, request.UnitPrice, request.Currency);
        if (await _products.AnyAsync(access.TenantId, x => x.Id != id && x.Sku == product.Sku, cancellationToken))
            throw new ConflictException("A product with this SKU already exists in this tenant.");
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return product.ToDto();
    }

    public async Task<ProductDto> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var access = await _guard.RequireAsync(cancellationToken, MemberRole.Owner, MemberRole.Admin, MemberRole.Manager);
        var product = await _products.GetAsync(access.TenantId, id, cancellationToken)
            ?? throw new NotFoundException("Product not found.");
        if (!product.IsActive) throw new ConflictException("Product is already archived.");
        product.Archive();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return product.ToDto();
    }

    private static void ValidatePrice(decimal value)
    {
        if (value < 0 || value > 9999999999999999m || value != decimal.Round(value, 2))
            throw new ValidationException("UnitPrice must be between 0 and 9999999999999999 and have at most two decimal places.");
    }
}
