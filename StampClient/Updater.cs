using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;


namespace StampClient
{
    delegate void UpdateCallback(UpdaterResult result);

    class UpdaterResult
    {
        public List<long> LongVals;
        public List<double> DoubleVals;

        public UpdaterResult()
        {
            LongVals = new List<long>();
            DoubleVals = new List<double>();
        }
    }

    abstract class Updater
    {
        protected Timer _timer;
        protected ExchangeLink _link;
        
        private UpdateCallback _callback;

        public Updater(ExchangeLink l, UpdateCallback cb, int interval)
        {
            _link = l;
            _callback = cb;
            _timer = new Timer();
            _timer.Interval = interval;
            _timer.Elapsed += new ElapsedEventHandler(DoUpdate);
            _timer.Enabled = false;
        }

        public void Start(bool updateNow)
        {
            _timer.Enabled = true;
            if (updateNow)
            {
                DoUpdate(this, null);
            }
        }

        public void Stop()
        {
            _timer.Enabled = false;
        }
        
        protected abstract UpdaterResult fetchUpdateResult();

        private void DoUpdate(object sender, ElapsedEventArgs e)
        {
            UpdaterResult r = fetchUpdateResult();
            _callback?.Invoke(r);
        }        
    }
}
