using SalesFlow.Domain.Common;

namespace SalesFlow.Domain.Entities;
public sealed class CustomerContact : TenantEntity
{
    public Guid CustomerId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }

    private CustomerContact() { }

    public CustomerContact(Guid tenantId, Guid customerId, string fullName, string? email = null, string? phone = null) : base(tenantId)
    {
        DomainRules.RequiredId(customerId, nameof(CustomerId));
        CustomerId = customerId;
        UpdateDetails(fullName, email, phone);
    }
    public void UpdateDetails(string fullName, string? email, string? phone)
    {
        FullName = DomainRules.Required(fullName, nameof(FullName), 160);
        Email = DomainRules.Email(email);
        Phone = DomainRules.Optional(phone, nameof(Phone), 40);
        Touch();
    }
}
