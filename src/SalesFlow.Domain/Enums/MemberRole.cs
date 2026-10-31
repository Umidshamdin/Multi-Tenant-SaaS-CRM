using System.ComponentModel;

namespace SalesFlow.Domain.Enums;
public enum MemberRole
{
    [Description("Şirkət sahibi və bütün idarəetmə hüquqlarına malik istifadəçi")]
    Owner = 1,

    [Description("Sistemin idarə olunmasına və istifadəçi icazələrinə cavabdeh administrator")]
    Admin = 2,

    [Description("Satış komandasını, müştəriləri və iş proseslərini idarə edən menecer")]
    Manager = 3,

    [Description("Müştərilərlə işləyən və satış əməliyyatlarını həyata keçirən əməkdaş")]
    Sales = 4
}