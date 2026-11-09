
using SalesFlow.Domain.Common;
using SalesFlow.Domain.Exceptions;

namespace SalesFlow.Domain.Entities;

public sealed class Product : TenantEntity
{
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string Currency { get; private set; } = "AZN";
    public bool IsActive { get; private set; }

    private Product() { }

    public Product(
        Guid tenantId,
        string sku,
        string name,
        string? description,
        decimal unitPrice,
        string currency) : base(tenantId)
    {
        SetDetails(sku, name, description, unitPrice, currency);
        IsActive = true;
    }

    public void Update(
        string sku,
        string name,
        string? description,
        decimal unitPrice,
        string currency)
    {
        if (!IsActive)
            throw new DomainException("Archived product cannot be edited.");

        SetDetails(sku, name, description, unitPrice, currency);
        Touch();
    }

    public void Archive()
    {
        if (!IsActive)
            throw new DomainException("Product is already archived.");

        IsActive = false;
        Touch();
    }

    private void SetDetails(
        string sku,
        string name,
        string? description,
        decimal unitPrice,
        string currency)
    {
        if (unitPrice < 0 || unitPrice > 9999999999999999m)
            throw new DomainException("Invalid product price.");

        Sku = DomainRules.Required(sku, nameof(Sku), 40).ToUpperInvariant();
        Name = DomainRules.Required(name, nameof(Name), 200);
        Description = DomainRules.Optional(description, nameof(Description), 2000);
        UnitPrice = unitPrice;
        Currency = DomainRules.Currency(currency);
    }
}
