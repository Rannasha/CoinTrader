using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;


namespace ExchangeHandlers
{
    public enum Currency { EUR, USD, GBP, KRW, XBT, LTC, NMC, XVN, XRP };
    public enum BasicOrderType { SELL, BUY };

    public delegate void DebugHandler(string msg, int type);

    public sealed class ReverseComparer<T> : IComparer<T>
    {
        private IComparer<T> originalComparer;

        public IComparer<T> OriginalComparer
        {
            get { return originalComparer; }
        }

        public ReverseComparer(IComparer<T> original)
        {
            this.originalComparer = original;
        }

        public int Compare(T x, T y)
        {
            return originalComparer.Compare(y, x);
        }
    }

    public static class ExchangeUtils
    {
        public const double SatoshiPerBTC = 100000000.0;

        public static long DoubleToSatoshi(double t)
        {
            return Convert.ToInt64(SatoshiPerBTC * t);
        }

        public static double SatoshiToDouble(long i)
        {
            return ((double)i) / SatoshiPerBTC;
        }

        public static long StringToSatoshi(string s)
        {
            double t;

            try
            {
                t = double.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                t = 0.0;
            }

            return Convert.ToInt64(SatoshiPerBTC * t);
        }

        public static string SatoshiToString(long i)
        {
            double t = ((double)i) / SatoshiPerBTC;
            NumberFormatInfo n = new CultureInfo("en-US", false).NumberFormat;

            return t.ToString(n);
        }

        public static DateTime UnixTimeStampToDateTime(long t)
        {
            DateTime d = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            d = d.AddSeconds(t).ToLocalTime();

            return d;
        }

        public static long DateTimeToUnixTimeStamp(DateTime dt)
        {
            TimeSpan span = (dt - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

            return (long)span.TotalSeconds;
        }

        public static string OrderTypeToString(BasicOrderType bot)
        {
            if (bot == BasicOrderType.SELL)
            {
                return "sell";
            }
            else
            {
                return "buy";
            }       
        }

        public static Currency StringToCurrency(string curr)
        {
            switch (curr)
            {
                case "XBT":
                    return Currency.XBT;
                case "LTC":
                    return Currency.LTC;
                case "USD":
                    return Currency.USD;
                case "GBP":
                    return Currency.GBP;
                case "BTC":
                    return Currency.XBT;
                case "EUR":
                    return Currency.EUR;
                case "KRW":
                    return Currency.KRW;
                case "NMC":
                    return Currency.NMC;
                case "XRP":
                    return Currency.XRP;
                case "XVN":
                    return Currency.XVN;
            }
            throw (new ArgumentException("Unknown currency"));
        }
    }

    [DataContract]
    public class Balance
    {
        public Dictionary<Currency, long> TotalBalance;

        public long this[Currency c]
        {
            get
            {
                return TotalBalance[c];
            }
        }
    }



    public abstract class ExchangeLink
    {
        public abstract OrderBook GetOrderBook();
        public abstract Balance GetBalance();
        public abstract bool SubmitLimitOrder(BasicOrderType ot, long amount, long price);
        public abstract bool SubmitMarketOrder(BasicOrderType ot, long amount);
        public abstract double GetFee();
    }
}
