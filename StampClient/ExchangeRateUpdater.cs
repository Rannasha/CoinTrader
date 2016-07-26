using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;


namespace StampClient
{
    class ExchangeRateUpdater : Updater
    {
        public ExchangeRateUpdater(ExchangeLink l, UpdateCallback cb, int interval) : base(l, cb, interval)
        {}

        protected override UpdaterResult fetchUpdateResult()
        {
            if (_link.GetType() == typeof(StampLink))
            {
                UpdaterResult r = new UpdaterResult();
                ExchangeRate e = ((StampLink)_link).GetEurUsdRate();

                r.DoubleVals.Add(e.Buy);
                r.DoubleVals.Add(e.Sell);

                return r;
            }
            else
            {
                return null;
            }
        }
    }
}
