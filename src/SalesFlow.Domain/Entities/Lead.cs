using SalesFlow.Domain.Common;
using SalesFlow.Domain.Enums;
using SalesFlow.Domain.Exceptions;

namespace SalesFlow.Domain.Entities;
public sealed class Lead : TenantEntity
{
    public string FullName { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? CompanyName { get; private set; }
    public LeadStatus Status { get; private set; }
    public Guid? ConvertedCustomerId { get; private set; }

    private Lead() { }

    public Lead(Guid tenantId, string fullName, string? email = null, string? companyName = null) : base(tenantId)
    {
        FullName = DomainRules.Required(fullName, nameof(FullName), 160);
        Email = DomainRules.Email(email);
        CompanyName = DomainRules.Optional(companyName, nameof(CompanyName), 200);
        Status = LeadStatus.New;
    }
    public void MarkContacted()
    {
        if (Status != LeadStatus.New) throw new DomainException("Only a new lead can be marked as contacted.");
        Status = LeadStatus.Contacted;
        Touch();
    }
    public void Qualify()
    {
        if (Status != LeadStatus.Contacted) throw new DomainException("A lead must be contacted before qualification.");
        Status = LeadStatus.Qualified;
        Touch();
    }
    public void MarkLost()
    {
        if (Status is LeadStatus.Converted or LeadStatus.Lost) throw new DomainException("A closed lead cannot be marked as lost again.");
        Status = LeadStatus.Lost;
        Touch();
    }
    public void ConvertToCustomer(Guid customerId)
    {
        DomainRules.RequiredId(customerId, nameof(customerId));
        if (Status != LeadStatus.Qualified) throw new DomainException("Only qualified leads may be converted.");
        ConvertedCustomerId = customerId;
        Status = LeadStatus.Converted;
        Touch();
    }
}