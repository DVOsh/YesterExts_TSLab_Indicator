using System;
using System.Collections.Generic;

namespace YesterExts
{
    public class DayLevelsData
    {
        readonly IList<double> HighPrices;
        readonly IList<double> LowPrices;

        double dayHigh, dayLow;
        int hlStartBar, hlEndBar;
        int llStartBar, llEndBar;

        public bool isHLVirgin, isLLVirgin;
        public bool isWeekEnd;

        public double[] highLevel;
        public double[] lowLevel;

        //Конструктор экземпляра класса
        public DayLevelsData(IList<double> highs, IList<double> lows, int firstBarDay)
        {
            HighPrices = highs;
            LowPrices = lows;

            dayHigh = highs[firstBarDay];
            dayLow = lows[firstBarDay];
            hlStartBar = firstBarDay;
            llStartBar = firstBarDay;
            highLevel = new double[highs.Count];
            lowLevel = new double[lows.Count];

            isHLVirgin = true;
            isLLVirgin = true;
            isWeekEnd = false;
        }

        //Инициализация объекта дневных уровней
        public void Execute(int startBar, int endBar)
        {
            CalcDayExts(startBar, endBar);
            SetLevelsStartPoints(endBar);
        }

        //Вычисление экстремумов дня
        void CalcDayExts(int startBarNum, int endBarNum)
        {
            for (int i = startBarNum; i <= endBarNum; i++)
            {
                if (HighPrices[i] > dayHigh)
                {
                    dayHigh = HighPrices[i];
                    hlStartBar = i;
                }

                if (LowPrices[i] < dayLow)
                {
                    dayLow = LowPrices[i];
                    llStartBar = i;
                }
            }
        }

        //Установка начальных значений для уровней
        void SetLevelsStartPoints(int fdEndBar)
        {
            Array.Fill(highLevel, dayHigh, hlStartBar, fdEndBar - hlStartBar + 1);
            Array.Fill(lowLevel, dayLow, llStartBar, fdEndBar - llStartBar + 1);
        }

        //Продление уровней
        public void ExtendLevels(int curBar)
        {
            if (isHLVirgin)
            {
                SetHLEndPoint(curBar, dayHigh);
                hlEndBar = curBar;
            }

            if (isLLVirgin)
            {
                SetLLEndPoint(curBar, dayLow);
                llEndBar = curBar;
            }
        }

        //Установка конечный точек для построения уровней
        void SetHLEndPoint(int endBarNum, double price)
        {
            highLevel[endBarNum] = price;
        }

        void SetLLEndPoint(int endBarNum, double price)
        {
            lowLevel[endBarNum] = price;
        }

        //Получение значений MarketPoint для отрисовки уровней
        public int GetHLStartBarNum()
        {
            return hlStartBar;
        }

        public int GetHLEndBarNum()
        {
            return hlEndBar;
        }

        public int GetLLStartBarNum()
        {
            return llStartBar;
        }

        public int GetLLEndBarNum()
        {
            return llEndBar;
        }

        //Получение значений экстремумов для проверки на девственность
        public double GetHighLevelPrice()
        {
            return dayHigh;
        }

        public double GetLowLevelPrice()
        {
            return dayLow;
        }
    }
}
