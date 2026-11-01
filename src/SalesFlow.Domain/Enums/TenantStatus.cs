using System.ComponentModel;

namespace SalesFlow.Domain.Enums;
public enum TenantStatus
{
    [Description("Şirkət aktivdir və sistemdən istifadə edə bilər")]
    Active = 1,

    [Description("Şirkətin fəaliyyəti müvəqqəti dayandırılıb və sistemə girişi məhdudlaşdırılıb")]
    Suspended = 2
}