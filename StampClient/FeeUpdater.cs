namespace StampClient
{
    class FeeUpdater : Updater
    {
        private Currency _quoteCurrency;
        private Currency _baseCurrency;

        public FeeUpdater(ExchangeLink l, UpdateCallback cb, int interval)
            : base(l, cb, interval)
        { }

        protected override UpdaterResult fetchUpdateResult()
        {
            double b = _link.GetFee();
            UpdaterResult r = new UpdaterResult();
            r.DoubleVals.Add(b);
            return r;
        }
    }
}
