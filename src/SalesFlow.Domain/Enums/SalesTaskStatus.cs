using System.ComponentModel;

namespace SalesFlow.Domain.Enums;
public enum SalesTaskStatus
{
    [Description("Tapşırıq açıqdır və icrası gözlənilir")]
    Open = 1,

    [Description("Tapşırıq uğurla tamamlanıb")]
    Completed = 2
}