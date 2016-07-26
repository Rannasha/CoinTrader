using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace StampClient
{
    class BalanceUpdater : Updater
    {
        private Currency _currency;

        public BalanceUpdater(ExchangeLink l, Currency c, UpdateCallback cb, int interval) : base(l, cb, interval)
        {
            _currency = c;
        }

        protected override UpdaterResult fetchUpdateResult()
        {
            UpdaterResult r = new UpdaterResult();
            Balance b;
            try
            {
                b = _link.GetBalance();
            }
            catch (Exception e)
            {
                r.LongVals.Add(-1);
                return r;
            }
            r.LongVals.Add(b.TotalBalance[_currency]);
            return r;
        }
    }
}
