using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ExchangeHandlers
{
    public enum TransactionType { BUY, SELL };

    #region Kraken Classes
    [DataContract]
    public class KrakenOrderBook : OrderBook
    {
        [DataMember(Name = "bids")]
        protected List<double[]> _bids;
        [DataMember(Name = "asks")]
        protected List<double[]> _asks;

        [OnDeserialized()]
        internal void ProcessDeserializedFields(StreamingContext context)
        {
            Timestamp = DateTime.Now;

            double prev = -1;
            foreach (double[] o in _bids)
            {
                if (o[0] == prev)
                {
                    Bids[o[0]] += ExchangeUtils.DoubleToSatoshi(o[1]);
                }
                else
                {
                    Bids.Add(o[0], ExchangeUtils.DoubleToSatoshi(o[1]));
                    prev = o[0];
                }
            }

            foreach (double[] o in _asks)
            {
                if (o[0] == prev)
                {
                    Asks[o[0]] += ExchangeUtils.DoubleToSatoshi(o[1]);
                }
                else
                {
                    Asks.Add(o[0], ExchangeUtils.DoubleToSatoshi(o[1]));
                    prev = o[0];
                }
            }
        }
    }
    
    public class KrakenTransaction : Transaction
    {  // price, volume, datetime, buy/sell, market/limit, misc
       // ["605.99900","0.15140000",1469035571.3723,"b","m",""]

        public KrakenTransaction() { }
        public KrakenTransaction(object[] _items)
        {
            Rate = Convert.ToDouble(_items[0]);
            Amount = ExchangeUtils.DoubleToSatoshi(Convert.ToDouble(_items[1]));
            TimestampUnix = Convert.ToInt32((Convert.ToDouble(_items[2])));
            Type = _items[3].ToString().Equals("b") ? TransactionType.BUY : TransactionType.SELL;
        }

    }
    #endregion

    #region Bitstamp Classes
    [DataContract]
    public class BitstampOrderBook : OrderBook
    {
        [DataMember(Name = "timestamp")]
        private new long TimestampUnix;
        [DataMember(Name = "bids")]
        protected List<double[]> _bids;
        [DataMember(Name = "asks")]
        protected List<double[]> _asks;

        [OnDeserialized()]
        internal void ProcessDeserializedFields(StreamingContext context)
        {
            Bids = new SortedDictionary<double, long>(new ReverseComparer<double>(Comparer<double>.Default));
            Asks = new SortedDictionary<double, long>();

            foreach (double[] o in _bids)
            {
                Bids.Add(o[0], ExchangeUtils.DoubleToSatoshi(o[1]));
            }
            foreach (double[] o in _asks)
            {
                Asks.Add(o[0], ExchangeUtils.DoubleToSatoshi(o[1]));
            }
        }
    }

    [DataContract]
    public class BitstampTransaction : Transaction
    {
        [DataMember(Name = "price")]
        public new double Rate;
        [DataMember(Name = "amount")]
        private double _amount;
        [DataMember(Name = "tid")]
        public new long Id;
        [DataMember(Name = "date")]
        public new long TimestampUnix;
        [DataMember(Name = "type")]
        private int _type;
        
        [OnDeserialized()]
        internal void ProcessDeserializedFields(StreamingContext context)
        {
            Amount = ExchangeUtils.DoubleToSatoshi(_amount);
            Timestamp = ExchangeUtils.UnixTimeStampToDateTime(TimestampUnix);
            Type = _type == 0 ? TransactionType.BUY : TransactionType.SELL;
        }
    }
    #endregion

    #region Base Classes
    public class OrderBook
    {
        public DateTime Timestamp
        {
            get
            {
                return ExchangeUtils.UnixTimeStampToDateTime(TimestampUnix);
            }
            set
            {
                TimestampUnix = ExchangeUtils.DateTimeToUnixTimeStamp(value);
            }
        }
        public long TimestampUnix;
        public SortedDictionary<double, long> Bids;
        public SortedDictionary<double, long> Asks;

        protected SortedDictionary<double, long> _cumulBidDepth;
        protected SortedDictionary<double, long> _cumulAskDepth;
        public SortedDictionary<double, long> CumulativeBids
        {
            get
            {
                return _cumulAskDepth;
            }
        }
        public SortedDictionary<double, long> CumulativeAsks
        {
            get
            {
                return _cumulAskDepth;
            }
        }

        /* GetPriceDepth() -- Computes the price that is reached when a given
         * amount of coins is bought or sold.
         * 
         * amount -- amount of satoshis to buy or sell.
         * type -- type of order (buy or sell).
         * 
         * Returns Tuple<long, long> containing the final price reached (item 1)
         * and average price for the entire trade (item 2).
         */
        public Tuple<double, double> GetPriceDepth(long amount, BasicOrderType type)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException("[OrderBook.GetPriceDepth] Amount has to be strictly positive.");
            }
            double totalPrice = 0;
            long amountRemaining = amount;
            int index = 0;
            SortedDictionary<double, long> Book = type == BasicOrderType.BUY ? Asks : Bids;

            while (amountRemaining > 0)
            {
                if (Book.Values.ElementAt(index) > amountRemaining)
                {
                    totalPrice += ExchangeUtils.SatoshiToDouble(amountRemaining) * Book.Keys.ElementAt(index);
                    amountRemaining = 0;
                }
                else
                {
                    totalPrice += Book.Keys.ElementAt(index) * ExchangeUtils.SatoshiToDouble(Book.Values.ElementAt(index));
                    amountRemaining -= Book.Values.ElementAt(index);
                }
                index++;
            }

            double lastPrice = Book.Keys.ElementAt(index - 1);
            double avgPrice = totalPrice / ExchangeUtils.SatoshiToDouble(amount);

            return Tuple.Create(lastPrice, avgPrice);
        }

        public void ComputeDepth(double range)
        {
            long sum = 0;

            double midPrice = (Bids.Keys.Max() + Asks.Keys.Min()) / 2;
            double minPrice = (1.0 - range) * midPrice;
            double maxPrice = (1.0 + range) * midPrice;

            if (range == 0.0)
            {
                minPrice = 0;
                maxPrice = 100000000000;
            }

            _cumulBidDepth = new SortedDictionary<double, long>(new ReverseComparer<double>(Comparer<double>.Default));
            _cumulAskDepth = new SortedDictionary<double, long>();

            foreach (KeyValuePair<double, long> kv in Bids.Reverse())
            {
                if (kv.Key < minPrice)
                {
                    continue;
                }
                sum += kv.Value;
                _cumulBidDepth.Add(kv.Key, sum);
            }

            sum = 0;
            foreach (KeyValuePair<double, long> kv in Asks)
            {
                if (kv.Key > maxPrice)
                {
                    continue;
                }
                sum += kv.Value;
                _cumulAskDepth.Add(kv.Key, sum);
            }
        }

        public OrderBook()
        {
            Bids = new SortedDictionary<double, long>(new ReverseComparer<double>(Comparer<double>.Default));
            Asks = new SortedDictionary<double, long>();
        }
    }

    public class Transaction
    {
        public long Id;
        public double Rate;
        public long Amount;
        public long TimestampUnix;
        public TransactionType Type;
        public DateTime Timestamp
        {
            get
            {
                return ExchangeUtils.UnixTimeStampToDateTime(TimestampUnix);
            }
            set
            {
                TimestampUnix = ExchangeUtils.DateTimeToUnixTimeStamp(value);
            }
        }
        
        public Transaction()
        {
        }       
    }
    #endregion
}