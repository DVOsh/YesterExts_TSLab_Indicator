using System.ComponentModel;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace YesterExts
{
    [HandlerCategory("MyIndicators")]
    [HandlerName("YesterExts")]
    [Description("Отрисовывает экстремумы прошлых дней в течение недели. Возвращает массив уровней")]
    [InputsCount(1)]
    [Input(0, TemplateTypes.SECURITY, Name = "SECURITYSource")]
    [OutputsCount(0)]
    public class YesterExtsHandler : IContextUses
    {
        [HandlerParameter(true, "true")]
        public bool ShowPrice { get; set; }

        [HandlerParameter(true, "true")]
        public bool ShowVola { get; set; }

        public IContext Context { get; set; }

        public void Execute(ISecurity sec)
        {
            LevelsContainer lvlsCont = new(ShowPrice, ShowVola)
            {
                BarsList = sec.Bars,
                highPrices = sec.HighPrices,
                lowPrices = sec.LowPrices,
                priceTick = sec.Tick,
            };

            lvlsCont.Execute(Context);
        }
    }
}