using System;
using System.Collections.Generic;
using TSLab.DataSource;
using TSLab.Script;
using TSLab.Script.GraphPane;
using TSLab.Script.Handlers;


namespace YesterExts
{
    public class LevelsContainer
    {
        public required double priceTick;                   //Шаг изменения цены
        public required IReadOnlyList<IDataBar> BarsList;   //Список баров
        public required IList<double> highPrices;           //Список максимумов баров для расчета максимального значения дня
        public required IList<double> lowPrices;            //Список минимумов баров для расчета минимального значения дня

        readonly bool isShowPriceLabel;                     //Показывать ли значение цены уровня
        readonly bool isShowVolaLabel;                      //Показывать ли значение волатильности дня
        readonly Color HLColor;                             //Цвет уровня максимума дня
        readonly Color LLColor;                             //Цвет уровня минимума дня

        int newDayBarNum;                                   //Номер бара срабатывания условия isNewDay

        int barsCount;                                      //Количество баров
        readonly List<DayLevelsData> LevelsDataList;        //Список объектов LineData

        public LevelsContainer(bool isShowPrice, bool isShowVola)
        {
            BarsList = [];
            highPrices = [];
            lowPrices = [];
            priceTick = 0;

            HLColor = ScriptColors.Green;
            LLColor = ScriptColors.Red;
            isShowPriceLabel = isShowPrice;
            isShowVolaLabel = isShowVola;

            newDayBarNum = 0;

            LevelsDataList = [];
        }

        /// <summary>
        /// Инициализация объекта LinesContainer
        /// </summary>
        /// <param name="context">Контекст</param>
        public void Execute(IContext context)
        {
            context.First.ClearInteractiveObjects();
            this.FillLineDataList();
            this.DrawLevels(context.First);
            if (isShowPriceLabel)
                DrawPriceLabels(context.First);
            if (isShowVolaLabel)
                DrawVolaLabels(context.First);
        }

        /// <summary>
        /// Получение списка объектов LineData
        /// </summary>
        public void FillLineDataList()
        {
            bool isNewDay = false;
            bool isNewWeek = true;
            bool isNewMonth = true;

            barsCount = BarsList.Count;

            for (int i = 0; i < barsCount; i++)
            {
                //Определение условия нового дня/недели/месяца
                isNewDay = BarsList[i].Date.Day > BarsList[newDayBarNum].Date.Day;
                isNewWeek = BarsList[i].Date.Day - BarsList[newDayBarNum].Date.Day > 1;
                isNewMonth = BarsList[i].Date.Month > BarsList[newDayBarNum].Date.Month;


                //Вычисление начальных значений уровней
                if (isNewDay || isNewMonth)
                {
                    //Инициализация объекта LineData
                    DayLevelsData ld = new(highPrices, lowPrices, newDayBarNum);
                    ld.Execute(newDayBarNum, i - 1);

                    LevelsDataList.Add(ld);

                    newDayBarNum = i;
                }

                UpdateLines(LevelsDataList, BarsList[i].High, BarsList[i].Low, isNewWeek || isNewMonth, i);

                isNewWeek = false;
                isNewDay = false;
                isNewMonth = false;
            }
        }

        /// <summary>
        /// Обновление уровней (если девственный - продление, если нет - установка конечных точек)
        /// </summary>
        /// <param name="levelsList">Список объектов DayLevelsData</param>
        /// <param name="curHigh">Текущий максимум</param>
        /// <param name="curLow">Текущий минимум</param>
        /// <param name="isStopLevels">Закончились ли уровни?</param>
        /// <param name="curBar">Номер текущего бара</param>
        private void UpdateLines(List<DayLevelsData> levelsList, double curHigh, double curLow, bool isStopLevels, int curBar)
        {
            if (LevelsDataList.Count < 1)
                return;

            foreach (DayLevelsData levelsObj in levelsList)
            {
                if (levelsObj.isHLVirgin)
                {
                    if (!isStopLevels)
                        levelsObj.ExtendLevels(curBar);

                    levelsObj.isHLVirgin = levelsObj.GetHighLevelPrice() >= curHigh && !isStopLevels;
                }
                
                if (levelsObj.isLLVirgin)
                {
                    if (!isStopLevels)
                        levelsObj.ExtendLevels(curBar);

                    levelsObj.isLLVirgin = levelsObj.GetLowLevelPrice() <= curLow && !isStopLevels;
                }
            }
        }

        //Отрисовка уровней
        public void DrawLevels(IGraphPane pane)
        {
            foreach (DayLevelsData levelsObj in LevelsDataList)
            {
                pane.AddList($"HighLevel${levelsObj.GetHashCode()}", levelsObj.highLevel,
                    ListStyles.LINE_WO_ZERO, ScriptColors.Aqua, LineStyles.SOLID, PaneSides.RIGHT);
                pane.AddList($"LowLevel${levelsObj.GetHashCode()}", levelsObj.lowLevel,
                    ListStyles.LINE_WO_ZERO, ScriptColors.OrangeRed, LineStyles.SOLID, PaneSides.RIGHT);
            }
        }

        //Отрисовка ценовых меток
        public void DrawPriceLabels(IGraphPane pane)
        {
            foreach (DayLevelsData levelsObj in LevelsDataList)
            {
                MarketPoint highPricePoint = new(BarsList[levelsObj.GetHLEndBarNum()].Date, levelsObj.GetHighLevelPrice());
                MarketPoint lowPricePoint = new(BarsList[levelsObj.GetLLEndBarNum()].Date, levelsObj.GetLowLevelPrice());

                IInteractiveText hlPriceText = pane.AddInteractiveText($"high_price{levelsObj.GetHashCode()}",
                    PaneSides.RIGHT, false, HLColor, highPricePoint);
                hlPriceText.Text = levelsObj.GetHighLevelPrice().ToString();
                hlPriceText.IsMoving = false;

                IInteractiveText llPriceText = pane.AddInteractiveText($"low_price{levelsObj.GetHashCode()}",
                    PaneSides.RIGHT, false, LLColor, lowPricePoint);
                llPriceText.Text = levelsObj.GetLowLevelPrice().ToString();
                llPriceText.IsMoving = false;
            }
        }

        //Отрисовка дневной волатильности
        public void DrawVolaLabels(IGraphPane pane)
        {
            foreach (DayLevelsData levelsObj in LevelsDataList)
            {
                double priceVola = levelsObj.GetHighLevelPrice() - levelsObj.GetLowLevelPrice();
                MarketPoint volaPoint = new(BarsList[levelsObj.GetLLStartBarNum()].Date, levelsObj.GetLowLevelPrice());

                IInteractiveText dayVolaText = pane.AddInteractiveText($"day_vola{levelsObj.GetHashCode()}",
                    PaneSides.RIGHT, false, LLColor, volaPoint);
                dayVolaText.Text = Math.Round(priceVola / priceTick).ToString() + " pips";
                dayVolaText.IsMoving = false;
            }
        }

        //Получение значений уровней для обработчика
        //Для реального рынка - берутся только последние уровни
        public IList<double> GetHighLevels()
        {
            IList<double> valuesList = [];

            LevelsDataList.ForEach(levelsObj =>
            {
                if (levelsObj.isHLVirgin)
                    valuesList.Add(levelsObj.GetHighLevelPrice());
            });

            return valuesList;
        }

        public IList<double> GetLowLevels()
        {
            IList<double> valuesList = [];

            LevelsDataList.ForEach(levelsObj =>
            {
                if (levelsObj.isLLVirgin)
                    valuesList.Add(levelsObj.GetLowLevelPrice());
            });

            return valuesList;
        }

        //Для тестирования роботов - берутся только уровни, соответствующие номеру текущего бара
        public IList<double> GetHighLevels(int barNum)
        {
            IList<double> valuesList = [];

            LevelsDataList.ForEach(levelsObj =>
            {
                if (levelsObj.GetHLStartBarNum() <= barNum && levelsObj.GetHLEndBarNum() >= barNum)
                    valuesList.Add(levelsObj.GetHighLevelPrice());
            });

            return valuesList;
        }

        public IList<double> GetLowLevels(int barNum)
        {
            IList<double> valuesList = [];

            LevelsDataList.ForEach(levelsObj =>
            {
                if (levelsObj.GetLLStartBarNum() <= barNum && levelsObj.GetLLEndBarNum() >= barNum)
                    valuesList.Add(levelsObj.GetLowLevelPrice());
            });

            return valuesList;
        }
    }
}
