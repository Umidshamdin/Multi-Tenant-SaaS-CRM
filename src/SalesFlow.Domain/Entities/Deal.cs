using SalesFlow.Domain.Common;
using SalesFlow.Domain.Enums;
using SalesFlow.Domain.Exceptions;

namespace SalesFlow.Domain.Entities;

public sealed class Deal : TenantEntity
{
    public Guid CustomerId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DealStage Stage { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    private Deal() { }

    public Deal(Guid tenantId, Guid customerId, string title, decimal amount, string currency = "USD") : base(tenantId)
    {
        DomainRules.RequiredId(customerId, nameof(CustomerId));
        CustomerId = customerId;
        Title = DomainRules.Required(title, nameof(Title), 200);
        SetAmount(amount);
        Currency = DomainRules.Currency(currency);
        Stage = DealStage.Prospecting;
    }
    public void UpdateTitle(string title)
    {
        EnsureOpen();
        Title = DomainRules.Required(title, nameof(Title), 200);
        Touch();
    }
    public void ChangeAmount(decimal amount)
    {
        EnsureOpen();
        SetAmount(amount);
        Touch();
    }
    public void MoveTo(DealStage stage)
    {
        EnsureOpen();
        if (stage is not (DealStage.Prospecting or DealStage.Qualification or
            DealStage.Proposal or DealStage.Negotiation))
            throw new DomainException("Use MarkWon or MarkLost to close a deal.");
        Stage = stage;
        Touch();
    }
    public void MarkWon() => Close(DealStage.Won);
    public void MarkLost() => Close(DealStage.Lost);
    private void Close(DealStage stage)
    {
        EnsureOpen();
        Stage = stage;
        ClosedAt = DateTime.Now;
        Touch();
    }
    private void EnsureOpen()
    {
        if (Stage is DealStage.Won or DealStage.Lost) throw new DomainException("A closed deal cannot be changed.");
    }
    private void SetAmount(decimal amount)
    {
        if (amount <= 0) throw new DomainException("Deal amount must be greater than zero.");
        Amount = amount;
    }
}