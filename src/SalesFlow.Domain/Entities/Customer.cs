using SalesFlow.Domain.Common;

namespace SalesFlow.Domain.Entities;
public sealed class Customer : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public bool IsActive { get; private set; }

    private Customer() { }

    public Customer(Guid tenantId, string name, string? email = null, string? phone = null) : base(tenantId)
    {
        Name = DomainRules.Required(name, nameof(Name), 200);
        Email = DomainRules.Email(email);
        Phone = DomainRules.Optional(phone, nameof(Phone), 40);
        IsActive = true;
    }
    public void UpdateDetails(string name, string? email, string? phone)
    {
        Name = DomainRules.Required(name, nameof(Name), 200);
        Email = DomainRules.Email(email);
        Phone = DomainRules.Optional(phone, nameof(Phone), 40);
        Touch();
    }
    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        Touch();
    }
}