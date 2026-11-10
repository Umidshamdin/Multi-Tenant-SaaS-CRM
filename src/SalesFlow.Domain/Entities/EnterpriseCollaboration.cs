using System.Globalization;
using SalesFlow.Domain.Common;
using SalesFlow.Domain.Enums;
using SalesFlow.Domain.Exceptions;

namespace SalesFlow.Domain.Entities;

public sealed class CrmNote : TenantEntity
{
    public CrmRecordKind RecordKind { get; private set; }
    public Guid RecordId { get; private set; }
    public Guid AuthorMemberId { get; private set; }
    public string Body { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    private CrmNote() { }
    public CrmNote(Guid tenantId, CrmRecordKind recordKind, Guid recordId, Guid authorMemberId, string body) : base(tenantId)
    {
        if (!Enum.IsDefined(recordKind)) throw new DomainException("Invalid record type.");
        DomainRules.RequiredId(recordId, nameof(recordId));
        DomainRules.RequiredId(authorMemberId, nameof(authorMemberId));
        RecordKind = recordKind;
        RecordId = recordId;
        AuthorMemberId = authorMemberId;
        Body = DomainRules.Required(body, nameof(Body), 4000);
        IsActive = true;
    }
    public void Edit(string body) { if (!IsActive) throw new DomainException("Note has been removed."); Body = DomainRules.Required(body, nameof(Body), 4000); Touch(); }
    public void Remove() { if (!IsActive) return; IsActive = false; Touch(); }
}

public sealed class CrmTag : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Color { get; private set; }
    private CrmTag() { }
    public CrmTag(Guid tenantId, string name, string? color = null) : base(tenantId)
    {
        Name = DomainRules.Required(name, nameof(Name), 80);
        Color = DomainRules.Optional(color, nameof(Color), 7);
        if (Color is not null && !System.Text.RegularExpressions.Regex.IsMatch(Color, "^#[0-9a-fA-F]{6}$"))
            throw new DomainException("Color must be a hexadecimal RGB value.");
    }
}

public sealed class CrmRecordTag : TenantEntity
{
    public Guid TagId { get; private set; }
    public CrmRecordKind RecordKind { get; private set; }
    public Guid RecordId { get; private set; }
    public bool IsActive { get; private set; }
    private CrmRecordTag() { }
    public CrmRecordTag(Guid tenantId, Guid tagId, CrmRecordKind recordKind, Guid recordId) : base(tenantId)
    {
        DomainRules.RequiredId(tagId, nameof(tagId));
        DomainRules.RequiredId(recordId, nameof(recordId));
        if (!Enum.IsDefined(recordKind)) throw new DomainException("Invalid record type.");
        TagId = tagId;
        RecordKind = recordKind;
        RecordId = recordId;
        IsActive = true;
    }
    public void Activate() { if (IsActive) return; IsActive = true; Touch(); }
    public void Remove() { if (!IsActive) return; IsActive = false; Touch(); }
}

public sealed class CrmCustomFieldDefinition : TenantEntity
{
    public CrmRecordKind RecordKind { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public CustomFieldType FieldType { get; private set; }
    public bool IsActive { get; private set; }
    private CrmCustomFieldDefinition() { }
    public CrmCustomFieldDefinition(Guid tenantId, CrmRecordKind recordKind, string key, string label, CustomFieldType type) : base(tenantId)
    {
        if (!Enum.IsDefined(recordKind) || !Enum.IsDefined(type)) throw new DomainException("Invalid field configuration.");
        var keyValue = DomainRules.Required(key, nameof(Key), 64);
        if (!System.Text.RegularExpressions.Regex.IsMatch(keyValue, "^[a-z][a-z0-9_]*$")) throw new DomainException("Field key must use lowercase letters, numbers, and underscores.");
        RecordKind = recordKind;
        Key = keyValue;
        Label = DomainRules.Required(label, nameof(Label), 120);
        FieldType = type;
        IsActive = true;
    }
    public void Disable() { if (!IsActive) return; IsActive = false; Touch(); }
}

public sealed class CrmCustomFieldValue : TenantEntity
{
    public Guid FieldId { get; private set; }
    public CrmRecordKind RecordKind { get; private set; }
    public Guid RecordId { get; private set; }
    public string Value { get; private set; } = string.Empty;
    private CrmCustomFieldValue() { }
    public CrmCustomFieldValue(Guid tenantId, CrmCustomFieldDefinition definition, Guid recordId, string value) : base(tenantId)
    {
        if (definition.TenantId != tenantId || !definition.IsActive) throw new DomainException("Invalid custom field.");
        DomainRules.RequiredId(recordId, nameof(recordId));
        FieldId = definition.Id;
        RecordKind = definition.RecordKind;
        RecordId = recordId;
        SetValue(definition.FieldType, value);
    }
    public void Change(CustomFieldType type, string value) { SetValue(type, value); Touch(); }
    private void SetValue(CustomFieldType type, string value)
    {
        var text = DomainRules.Required(value, nameof(Value), 2000);
        var valid = type switch
        {
            CustomFieldType.Text => true,
            CustomFieldType.Number => decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out _),
            CustomFieldType.Boolean => bool.TryParse(text, out _),
            CustomFieldType.Date => DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _),
            _ => false
        };
        if (!valid) throw new DomainException($"Invalid {type} custom field value.");
        Value = text;
    }
}
