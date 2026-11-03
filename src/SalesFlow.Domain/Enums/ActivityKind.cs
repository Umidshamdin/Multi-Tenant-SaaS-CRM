using System.ComponentModel;

namespace SalesFlow.Domain.Enums;
public enum ActivityKind
{
    [Description("Zəng")]
    Call = 1,

    [Description("Görüş")]
    Meeting = 2,

    [Description("Elektron poçt")]
    Email = 3,

    [Description("Qeyd")]
    Note = 4
}
