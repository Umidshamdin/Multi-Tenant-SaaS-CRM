using System.ComponentModel;

namespace SalesFlow.Domain.Enums;
public enum DealStage
{
    [Description("Potensial satış imkanlarının müəyyən edilməsi")]
    Prospecting = 1,

    [Description("Müştərinin ehtiyaclarının və satış potensialının qiymətləndirilməsi")]
    Qualification = 2,

    [Description("Kommersiya təklifinin hazırlanması və təqdim edilməsi")]
    Proposal = 3,

    [Description("Qiymət və müqavilə şərtlərinin müzakirəsi")]
    Negotiation = 4,

    [Description("Satış uğurla tamamlanıb")]
    Won = 5,

    [Description("Satış baş tutmayıb")]
    Lost = 6
}