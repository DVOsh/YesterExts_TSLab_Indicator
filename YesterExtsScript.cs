using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Optimization;


namespace YesterExts
{
    public class YesterExtsScript : IExternalScript
    {
        public BoolOptimProperty ShowPrice = new(false);
        public BoolOptimProperty ShowVola = new(false);

        public void Execute(IContext ctx, ISecurity sec)
        {
            LevelsContainer lvlsCont = new(ShowPrice, ShowVola)
            {
                BarsList = sec.Bars,
                highPrices = sec.HighPrices,
                lowPrices = sec.LowPrices,
                priceTick = sec.Tick,
            };

            lvlsCont.Execute(ctx);
        }
    }
}
