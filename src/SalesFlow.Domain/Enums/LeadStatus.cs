using System.ComponentModel;

namespace SalesFlow.Domain.Enums;
public enum LeadStatus
{
    [Description("Yeni potensial müştəri")]
    New = 1,

    [Description("Potensial müştəri ilə əlaqə saxlanılıb")]
    Contacted = 2,

    [Description("Potensial müştəri qiymətləndirilib və uyğun hesab edilib")]
    Qualified = 3,

    [Description("Potensial müştəri real müştəriyə çevrilib")]
    Converted = 4,

    [Description("Potensial müştəri itirilib")]
    Lost = 5
}