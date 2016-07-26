using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace StampClient
{
    class ArbAgent
    {
        private System.Timers.Timer _timer;

        private BackgroundWorker _bwBuyCheck;
        private BackgroundWorker _bwSellCheck;

        private BackgroundWorker _bwBuyOrder;
        private BackgroundWorker _bwSellOrder;

        private ExchangeLink _sellLink;
        private ExchangeLink _buyLink;

        private BalanceUpdater _sellBalanceUpdater;
        private BalanceUpdater _buyBalanceUpdater;

        private FeeUpdater _sellFeeUpdater;
        private FeeUpdater _buyFeeUpdater;

        private ExchangeRateUpdater _exchangeRateUpdater;

        private long _sellBalance;
        private long _buyBalance;

        private bool _buyCheckCompleted;
        private bool _sellCheckCompleted;
        private bool _buyOrderCompleted;
        private bool _sellOrderCompleted;
        private bool _updateInProgress;

        private double _buyFee = 5;
        private double _sellFee = 5;

        private ExchangeRate _exchangeRate;

        private double _minProfitPercentage;

        public double MinProfitPercentage
        {
            get
            {
                return _minProfitPercentage;
            }
            set
            {
                if (value > 0)
                {
                    _minProfitPercentage = value;
                    Log("Profit percentage updated to " + value.ToString("0.000") + "%", 0);
                }
                else
                {
                    Log("Invalid value for min profit.", 0);
                }
            }
        }

        private long _volume = 1000000;
        private long _maxVol = 10000000;
        private long vol;

        private Tuple<double, double> _buyData;
        private Tuple<double, double> _sellData;

        private OrderBook _buyBook;
        private OrderBook _sellBook;

        public DebugHandler Logger;

        private void Log(string msg, int type)
        {
            if (Logger != null)
            {
                Logger(msg, type);
            }
        }

        private void UpdateSellFee(UpdaterResult result)
        {
            double fee = result.DoubleVals[0];
            if (fee != _sellFee)
            {
                Log("Kraken fee changed to " + fee.ToString() + "%", 0);
                _sellFee = fee;
            }
        }

        private void UpdateBuyFee(UpdaterResult result)
        {
            double fee = result.DoubleVals[0];
            if (fee != _buyFee)
            {
                Log("Bitstamp fee changed to " + fee.ToString() + "%", 0);
                _buyFee = fee;
            }
        }

        private void UpdateSellBalance(UpdaterResult result)
        {
            long balance = result.LongVals[0];
            if (balance == -1)
            {
                Log("ERROR", 1);
                _sellBalance = 0;
            }
            else
            {
                Log(ExchangeUtils.SatoshiToString(balance) + " XBT", 1);
                _sellBalance = balance;
            }
        }

        private void UpdateBuyBalance(UpdaterResult result)
        {
            long balance = result.LongVals[0];
            Log(ExchangeUtils.SatoshiToString(balance) + " USD", 2);
            _buyBalance = balance;
        }

        private void UpdateExchangeRate(UpdaterResult result)
        {
            _exchangeRate.Buy = result.DoubleVals[0];
            _exchangeRate.Sell = result.DoubleVals[1];
            Log("EUR/USD rates: (Buy) " + _exchangeRate.Buy.ToString() + " - (Sell) " + _exchangeRate.Sell.ToString(), 0);
        }

        public ArbAgent(ExchangeLink sell, ExchangeLink buy, System.ComponentModel.ISynchronizeInvoke s)
        {
            _sellLink = sell;
            _buyLink = buy;

            _timer = new System.Timers.Timer();
            _timer.Interval = 10000;
            _timer.SynchronizingObject = s;
            _timer.Elapsed += new ElapsedEventHandler(DoUpdate);
            _timer.Enabled = false;

            _updateInProgress = false;

            _exchangeRate = new ExchangeRate();

            _sellBalanceUpdater = new BalanceUpdater(_sellLink, Currency.XBT, UpdateSellBalance, 60000);
            _buyBalanceUpdater = new BalanceUpdater(_buyLink, Currency.USD, UpdateBuyBalance, 60000);

            _sellFeeUpdater = new FeeUpdater(_sellLink, UpdateSellFee, 1000 * 3600 * 4);
            _buyFeeUpdater = new FeeUpdater(_buyLink, UpdateBuyFee, 1000 * 3600 * 4);

            _exchangeRateUpdater = new ExchangeRateUpdater(_buyLink, UpdateExchangeRate, 1000 * 3600 * 6);
        }

        public void StartAgent()
        {
            _timer.Enabled = true;
            _sellBalanceUpdater.Start(true);
            _buyBalanceUpdater.Start(true);
            _exchangeRateUpdater.Start(true);
            _sellFeeUpdater.Start(true);
            _buyFeeUpdater.Start(true);
        }

        public void StopAgent(bool graceful)
        {
            _timer.Enabled = false;
            _sellBalanceUpdater.Stop();
            _buyBalanceUpdater.Stop();
            _exchangeRateUpdater.Stop();
            _sellFeeUpdater.Stop();
            _buyFeeUpdater.Stop();
            if (graceful)
            {
                Log("Arbitrage Agent halted gracefully.", 0);
            }
            else
            {
                Log("FATAL: Arbitrage agent halted due to error.", 0);
            }
        }

        public void DoUpdate(object sender, ElapsedEventArgs e)
        {
            if (_updateInProgress)
            {
                return;
            }
            _updateInProgress = true;
            _buyCheckCompleted = false;
            _sellCheckCompleted = false;
            _bwBuyCheck = new BackgroundWorker();
            _bwBuyCheck.DoWork += (obj, ea) => FetchBuyData();
            _bwBuyCheck.RunWorkerCompleted += (obj, ea) => DataGatherCompleted(BasicOrderType.BUY);
            _bwBuyCheck.RunWorkerAsync();
            _bwSellCheck = new BackgroundWorker();
            _bwSellCheck.DoWork += (obj, ea) => FetchSellData();
            _bwSellCheck.RunWorkerCompleted += (obj, ea) => DataGatherCompleted(BasicOrderType.SELL);
            _bwSellCheck.RunWorkerAsync();
        }

        public void FetchBuyData()
        {
            try
            {
                _buyBook = _buyLink.GetOrderBook();
            }
            catch (Exception e)
            {
                Log("ERROR obtaining orderbook: " + e.Message, 0);
                _buyBook = null;
            }
        }

        public void FetchSellData()
        {
            try
            {
                _sellBook = _sellLink.GetOrderBook();
            }
            catch (Exception e)
            {
                Log("ERROR obtaining orderbook: " + e.Message, 0);
                _sellBook = null;
            }
        }

        public void PlaceBuyOrder(long p)
        {
            try
            {
                _buyLink.SubmitLimitOrder(BasicOrderType.BUY, vol, p);
            }
            catch (Exception e)
            {
                Log("ERROR placing buy order: " + e.Message, 0);
                StopAgent(false);
            }
        }

        public void PlaceSellOrder(long p)
        {
            try
            {
                _sellLink.SubmitMarketOrder(BasicOrderType.SELL, vol);
            }
            catch (Exception e)
            {
                Log("ERROR placing sell order: " + e.Message, 0);
                StopAgent(false);
            }
        }

        public void OrderPlacementCompleted(BasicOrderType type)
        {
            if (type == BasicOrderType.BUY)
            {
                _buyOrderCompleted = true;
            }
            else
            {
                _sellOrderCompleted = true;
            }

            if (_buyOrderCompleted && _sellOrderCompleted)
            {
                Log("V: " + ExchangeUtils.SatoshiToString(vol) + ", B: " + _buyData.Item2.ToString("0.00") + ", S: " + _sellData.Item2.ToString("0.00"), 0);
            }
        }

        public void DataGatherCompleted(BasicOrderType type)
        {
            if (type == BasicOrderType.BUY)
            {
                _buyCheckCompleted = true;
            }
            else
            {
                _sellCheckCompleted = true;
            }

            if (_buyCheckCompleted && _sellCheckCompleted)
            {
                if (_buyBook != null && _sellBook != null)
                {
                    ComputeArbitrage();
                }
                _updateInProgress = false;
            }
        }


        public void ComputeArbitrage()
        {
            double multiplier = (1.0 + _buyFee / 100.0) / (1.0 - _sellFee / 100.0) / _exchangeRate.Sell;

            vol = 0;
            _buyData = _buyBook.GetPriceDepth(_volume, BasicOrderType.BUY);
            _sellData = _sellBook.GetPriceDepth(_volume, BasicOrderType.SELL);

            double sellTarget = _buyData.Item1 * multiplier;
            double profitPercentage = 100.0 * _sellData.Item1 / sellTarget - 100.0;
            double rateDiff = _sellData.Item2 / (_buyData.Item2 / _exchangeRate.Rate);
            Log("B: " + _buyData.Item1.ToString() + " K: " + _sellData.Item1.ToString() + " = " + profitPercentage.ToString("0.000") + "%", 3);

            while (profitPercentage > _minProfitPercentage && vol <= _maxVol - _volume)
            {
                vol += _volume;
                _buyData = _buyBook.GetPriceDepth(vol + _volume, BasicOrderType.BUY);
                _sellData = _sellBook.GetPriceDepth(vol + _volume, BasicOrderType.SELL);

                sellTarget = _buyData.Item1 * multiplier;
                profitPercentage = 100.0 * _sellData.Item1 / sellTarget - 100.0;
            }                                  

            long balanceReqBuy = Convert.ToInt64(2 * vol * _buyData.Item1);
            long balanceReqSell = 2 * vol;
            
            if (vol > 0
                && _buyBalance > balanceReqBuy
                && _sellBalance > balanceReqSell
                && _buyData.Item1 > 0)
            {
                _buyBalance -= Convert.ToInt64(vol * _buyData.Item1);
                _sellBalance -= vol;
                _sellOrderCompleted = false;
                _buyOrderCompleted = false;
                _bwBuyOrder = new BackgroundWorker();
                _bwBuyOrder.DoWork += (obj, ea) => PlaceBuyOrder(ExchangeUtils.DoubleToSatoshi(_buyData.Item1 * 1.1));
                _bwBuyOrder.RunWorkerCompleted += (obj, ea) => OrderPlacementCompleted(BasicOrderType.BUY);
                _bwBuyOrder.RunWorkerAsync();
                _bwSellOrder = new BackgroundWorker();
                _bwSellOrder.DoWork += (obj, ea) => PlaceSellOrder(0);
                _bwSellOrder.RunWorkerCompleted += (obj, ea) => OrderPlacementCompleted(BasicOrderType.SELL);
                _bwSellOrder.RunWorkerAsync();
            }
        }
    }


}
