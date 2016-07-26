using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;

namespace ExchangeHandlers
{
    public enum KrakenOrderType { Market, Limit, TakeProfit, TakeProfitLimit, StopLoss,
                                  StopLossLimit, TakeProfitStopLoss, TakeProfitStopLossLimit,
                                  TrailingStop, TrailingStopLimit, StopLossAndLimit, Unknown };

    public class KrakenOrderFlags
    {
        public bool VolumeQuoteCurrency;
        public bool ProfitLossBasecurrency;
        public bool NoMarketPriceProtection;

        public KrakenOrderFlags(bool v, bool p, bool n)
        {
            VolumeQuoteCurrency = v;
            ProfitLossBasecurrency = p;
            NoMarketPriceProtection = n;
        }

        public override string ToString()
        {
 	        string s = "";
            bool first = true;
            
            if (VolumeQuoteCurrency)
            {
                s = "viqc";
                first = false;
            }
            if (ProfitLossBasecurrency)
            {
                if (!first)
                {
                    s += ",";
                }
                s += "plbc";
                first = false;
            }
            if (NoMarketPriceProtection)
            {
                if (!first)
                {
                    s += ",";
                }
                s += "nompp";
            }
            
            return s;
        }
    }

    public static class KrakenUtils
    {
        public static string KrakenOrderTypeToString(KrakenOrderType kot)
        {
            switch (kot)
            {
                case KrakenOrderType.Market:
                    return "market";
                case KrakenOrderType.Limit:
                    return "limit";
                case KrakenOrderType.TakeProfit:
                    return "take-profit";
                case KrakenOrderType.StopLoss:
                    return "stop-loss";
                case KrakenOrderType.TakeProfitLimit:
                    return "take-profit-limit";
                case KrakenOrderType.StopLossLimit:
                    return "stop-loss-limit"; 
                case KrakenOrderType.TakeProfitStopLoss:
                    return "stop-loss-profit";
                case KrakenOrderType.TakeProfitStopLossLimit:
                    return "stop-loss-profit-limit";
                case KrakenOrderType.TrailingStop:
                    return "trailing-stop";
                case KrakenOrderType.TrailingStopLimit:
                    return "trailing-stop-limit";
                case KrakenOrderType.StopLossAndLimit:
                    return "stop-loss-and-limit";
                default:
                    return "";
            }
        }

        private static string GetCurrencyPrefix(string curr)
        {
            if (curr == "EUR" || curr == "USD" || curr == "KRW")
            {
                return "Z" + curr;
            }
            else
            {
                return "X" + curr;
            }
        }

        public static string GetCurrencyPairName(Currency bCurr, Currency qCurr)
        {
            return GetCurrencyPrefix(bCurr.ToString()) + GetCurrencyPrefix(qCurr.ToString());
        }

        public static KrakenOrderType StringToKrakenOrderType(string _ordertype)
        {
            if (_ordertype == "market")
            {
                return KrakenOrderType.Market;
            }
            if (_ordertype == "limit")
            {
                return KrakenOrderType.Limit;
            }
            if (_ordertype == "stop-loss")
            {
                return KrakenOrderType.StopLoss;
            }
            if (_ordertype == "take-profit")
            {
                return KrakenOrderType.TakeProfit;
            }
            if (_ordertype == "stop-loss-limit")
            {
                return KrakenOrderType.StopLossLimit;
            }
            if (_ordertype == "take-profit-limit")
            {
                return KrakenOrderType.TakeProfitLimit;
            }
            if (_ordertype == "stop-loss-profit")
            {
                return KrakenOrderType.TakeProfitStopLoss;
            }
            if (_ordertype == "stop-loss-profit-limit")
            {
                return KrakenOrderType.TakeProfitStopLossLimit;
            }
            if (_ordertype == "trailing-stop")
            {
                return KrakenOrderType.TrailingStop;
            }
            if (_ordertype == "trailing-stop-limit")
            {
                return KrakenOrderType.TrailingStopLimit;
            }
            if (_ordertype == "stop-loss-and-limit")
            {
                return KrakenOrderType.StopLossAndLimit;
            }
            return KrakenOrderType.Unknown;
        }

    }

    [DataContract]
    public class KrakenFee
    {
        [DataMember(Name = "fee")]
        public double CurrentFee;
        [DataMember(Name = "minfee")]
        public double MinFee;
        [DataMember(Name = "nextfee")]
        public double NextFee;
        [DataMember(Name = "nextvolume")]
        public double NextVolume;
        [DataMember(Name = "tiervolume")]
        public double TierVolume;
    }

    [DataContract]
    public class KrakenTradeVolume
    {
        [DataMember(Name = "currency")]
        private string _currecy;
        [DataMember(Name = "volume")]
        public double Volume;
        [DataMember(Name = "fees")]
        public Dictionary<string, KrakenFee> Fee;
    }

    [DataContract]
    public class KrakenTradeVolumeResponse
    {
        [DataMember(Name = "result")]
        public KrakenTradeVolume Result;
    }

    [DataContract]
    public class KrakenOrderDescription
    {
        [DataMember(Name = "pair")]
        public string Pair;
        [DataMember(Name = "type")]
        private string _type;
        [DataMember(Name = "ordertype")]
        private string _ordertype;
        [DataMember(Name = "price")]
        private double _price;
        [DataMember(Name = "price2")]
        private double _price2;
        
        public BasicOrderType BuySell;
        public KrakenOrderType Type;
        public long Price;
        public long Price2;

        public KrakenOrderDescription(KrakenOrderType t, long p, long p2)
        {
            Type = t;
            Price = p;
            Price2 = p2;
        }

        [OnDeserialized()]
        internal void ProcessDeserializedFields(StreamingContext context)
        {
            if (_type == "buy")
            {
                BuySell = BasicOrderType.BUY;
            }
            else
            {
                BuySell = BasicOrderType.SELL;
            }

            Price = ExchangeUtils.DoubleToSatoshi(_price);
            Price2 = ExchangeUtils.DoubleToSatoshi(_price2);
            Type = KrakenUtils.StringToKrakenOrderType(_ordertype);
        }

    }

    [DataContract]
    public class KrakenOpenOrder
    {
        [DataMember(Name = "status")]
        public string Status;
        [DataMember(Name = "opentm")]
        private double _opentm;
        [DataMember(Name = "vol")]
        private double _vol;
        [DataMember(Name = "vol_exec")]
        private double _vol_exec;
        [DataMember(Name = "cost")]
        private double _cost;
        [DataMember(Name = "fee")]
        private double _fee;

        [DataMember(Name = "descr")]
        public KrakenOrderDescription Description;

    }

    [DataContract]
    public class KrakenOpenOrdersResult
    {
        [DataMember(Name = "open")]
        public Dictionary<string, KrakenOpenOrder> Orders;
    }

    [DataContract]
    public class KrakenOpenOrdersResponse
    {
        [DataMember(Name = "result")]
        public KrakenOpenOrdersResult Result;
    }

    [DataContract]
    public class KrakenBalance : Balance
    {
        [DataMember(Name = "ZEUR")]
        private double _eur;
        [DataMember(Name = "ZUSD")]
        private double _usd;
        [DataMember(Name = "ZKRW")]
        private double _krw;
        [DataMember(Name = "ZGBP")]
        private double _gbp;
        [DataMember(Name = "XXBT")]
        private double _xbt;
        [DataMember(Name = "XLTC")]
        private double _ltc;
        [DataMember(Name = "XNMC")]
        private double _nmc;
        [DataMember(Name = "XXRP")]
        private double _xrp;
        [DataMember(Name = "XXVN")]
        private double _xvn;

        [OnDeserialized()]
        internal void ProcessDeserializedFields(StreamingContext context)
        {
            TotalBalance = new Dictionary<Currency, long>();

            TotalBalance.Add(Currency.EUR, ExchangeUtils.DoubleToSatoshi(_eur));
            TotalBalance.Add(Currency.USD, ExchangeUtils.DoubleToSatoshi(_usd));
            TotalBalance.Add(Currency.KRW, ExchangeUtils.DoubleToSatoshi(_krw));
            TotalBalance.Add(Currency.GBP, ExchangeUtils.DoubleToSatoshi(_gbp));
            TotalBalance.Add(Currency.XBT, ExchangeUtils.DoubleToSatoshi(_xbt));
            TotalBalance.Add(Currency.LTC, ExchangeUtils.DoubleToSatoshi(_ltc));
            TotalBalance.Add(Currency.NMC, ExchangeUtils.DoubleToSatoshi(_nmc));
            TotalBalance.Add(Currency.XVN, ExchangeUtils.DoubleToSatoshi(_xvn));
            TotalBalance.Add(Currency.XRP, ExchangeUtils.DoubleToSatoshi(_xrp));
        }
    }

    [DataContract]
    public class KrakenBalanceResponse
    {
        [DataMember(Name = "result")]
        public KrakenBalance Balance;
    }
    
    [DataContract]
    public class KrakenOrderBookResult
    {
        [DataMember(Name="currencypair")]
        public KrakenOrderBook Orderbook;
    }
    
    [DataContract]
    public class KrakenOrderBookResponse
    {
        [DataMember(Name = "result")]
        public KrakenOrderBookResult Result;
    }

    [DataContract]
    public class KrakenTransactionListResponse
    {
        [DataMember(Name = "result")]
        public KrakenTransactionListResult Result;
    }

    [DataContract]
    public class KrakenTransactionListResult
    {
        [DataMember(Name = "currencypair")]
        private List<object[]> _txList;

        public List<KrakenTransaction> TransactionList;

        [OnDeserialized()]
        internal void ProcessDeserializedFields(StreamingContext context)
        {
            TransactionList = new List<KrakenTransaction>();
            foreach (object[] o in _txList)
            {
                TransactionList.Add(new KrakenTransaction(o));
            }
        }


    }

    public class KrakenException : Exception
    {
        public string ErrorType { get; set; }
        public string ErrorMessage { get; set; }
        public override string Message
        {
            get
            {
                return "(" + ErrorType + ") " + ErrorMessage;
            }
        }
        
        public KrakenException() : base() { }
        public KrakenException(string message) : base(message) { }
        public KrakenException(string message, System.Exception inner) : base(message, inner) { }
        public KrakenException(string errorType, string errorMessage) : this()
        {
            ErrorType = errorType;
            ErrorMessage = errorMessage;
        }
    }

    public class KrakenLink : ExchangeLink
    {
        private string _apiSecret;
        private string _apiKey;

        private const string _baseUrl = "https://api.kraken.com";

        public Currency DefaultBaseCurrency { get; set; }
        public Currency DefaultQuoteCurrency { get; set; }

        public KrakenLink(string apiKey, string apiSecret)
        {
            _apiKey = apiKey;
            _apiSecret = apiSecret;
            DefaultBaseCurrency = Currency.XBT;
            DefaultQuoteCurrency = Currency.EUR;
        }

        private string generateSignature(string secret, string path, string message, long nonce)
        {
            HMACSHA512 h = new HMACSHA512();
            h.Key = System.Convert.FromBase64String(secret);
            byte[] sha256hash = getSHA256Hash(nonce.ToString() + message);
            byte[] msg = Encoding.UTF8.GetBytes(path);
            var fullmsg = new byte[sha256hash.Length + msg.Length];
            msg.CopyTo(fullmsg, 0);
            sha256hash.CopyTo(fullmsg, msg.Length);
            byte[] hash = h.ComputeHash(fullmsg);

            return System.Convert.ToBase64String(hash);
        }

        private byte[] getSHA256Hash(string text)
        {
            byte[] result;
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                result = hash.ComputeHash(enc.GetBytes(text));
            }

            return result;
        }

        private string sendPostRequest(string path, Dictionary<string, string> parameters)
        {
            Dictionary<string, string> postParams;
            if (parameters == null)
            {
                postParams = new Dictionary<string, string>();
            }
            else
            {
                postParams = parameters;
            }
            long nonce = DateTime.Now.Ticks;
            postParams.Add("nonce", nonce.ToString());
            

            string postStr = "";
            string c = "";
            bool isFirst = true;

            foreach (KeyValuePair<string, string> arg in postParams)
            {
                if (isFirst)
                {
                    postStr += arg.Key + "=" + arg.Value;
                    isFirst = false;
                }
                else
                {
                    postStr += "&" + arg.Key + "=" + arg.Value;
                }
            }

            string signature = generateSignature(_apiSecret, path, postStr, nonce);

            try
            {
                System.Net.HttpWebRequest request = System.Net.HttpWebRequest.Create(_baseUrl + path) as System.Net.HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
                request.ContentType = "application/x-www-form-urlencoded";

                request.Method = "POST";
                request.Headers.Add("API-Key", _apiKey);
                request.Headers.Add("API-Sign", signature);
                UTF8Encoding enc = new UTF8Encoding();
                Byte[] buffer = enc.GetBytes(postStr);
                request.ContentLength = buffer.Length;
                System.IO.Stream requestStream = request.GetRequestStream();
                requestStream.Write(buffer, 0, buffer.Length);
                requestStream.Close();
                using (System.Net.HttpWebResponse response = request.GetResponse() as System.Net.HttpWebResponse)
                {
                    System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream());

                    // Server runs on a Unix/Linux system. Need to insert a "proper" Windows linebreak.
                    c = (reader.ReadToEnd()).Replace("\n", Environment.NewLine);
                }
            }
            // TODO: Change exception-messages
            catch (Exception ex)
            {
                throw (new Exception("Network Error. Message: " + ex.Message));
            }
            finally
            {
                GC.Collect();
            }
            return c;
        }

        private string sendGetRequest(string path, Dictionary<string, string> parameters)
        {
            string getStr = _baseUrl + path;
            bool first = true;
            string c;

            if (parameters.Count > 0)
            {
                getStr += "?";
            }
            foreach (KeyValuePair<string, string> arg in parameters)
            {
                if (!first)
                {
                    getStr += "&";
                }
                getStr += arg.Key + "=" + arg.Value;
                first = false;
            }
            try
            {
                System.Net.HttpWebRequest request = System.Net.HttpWebRequest.Create(getStr) as System.Net.HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "GET";

                using (System.Net.HttpWebResponse response = request.GetResponse() as System.Net.HttpWebResponse)
                {
                    System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream());

                    // Server runs on a Unix/Linux system. Need to insert a "proper" Windows linebreak.
                    c = (reader.ReadToEnd()).Replace("\n", Environment.NewLine);
                }
            }
            // TODO: Change exception-messages
            catch (Exception ex)
            {
                throw (new Exception("Network Error. Message: " + ex.Message));
            }
            finally
            {
                GC.Collect();
            }
            return c;
        }

        private KrakenException parseKrakenException(string json)
        {
            if (json.Contains("\"error\":[]"))
            {
                return null;
            }

            string errString = json.Substring(json.IndexOf('[') + 2, json.Length - json.IndexOf('[') - 5);
            string errType = errString.Substring(0, errString.IndexOf(':'));
            string errMessage = errString.Substring(errString.IndexOf(':') + 1);
            KrakenException res = new KrakenException(errType, errMessage);
            return res;
        }

        private T parseResponseJson<T>(string json)
        {
            string strippedJson = "{" + json.Substring(12);
            T res = default(T);
            DataContractJsonSerializerSettings s = new DataContractJsonSerializerSettings();
            s.UseSimpleDictionaryFormat = true;
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T), s);
            using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(strippedJson)))
            {
                object obj = ser.ReadObject(ms);
                res = (T)Convert.ChangeType(obj, typeof(T));
            }
            return res;           
        }

        public override OrderBook GetOrderBook()
        {
            return GetOrderBook(DefaultBaseCurrency, DefaultQuoteCurrency);
        }
        public KrakenOrderBook GetOrderBook(Currency curr1, Currency curr2)
        {
            return GetOrderBook(curr1, curr2, -1);
        }
        public KrakenOrderBook GetOrderBook(Currency curr1, Currency curr2, int count)
        {
            string path = "/0/public/Depth";
            string currencyPair = KrakenUtils.GetCurrencyPairName(curr1, curr2);
            Dictionary<string, string> parameters = new Dictionary<string,string>();
            parameters.Add("pair", currencyPair);
            if (count > 0)
            {
                parameters.Add("count", count.ToString());
            }
                        
            string response = sendGetRequest(path, parameters).Replace(currencyPair, "currencypair");

            KrakenOrderBookResponse parsedResponse = parseResponseJson<KrakenOrderBookResponse>(response);
            return parsedResponse.Result.Orderbook;
        }

        public override Balance GetBalance()
        {
            string path = "/0/private/Balance";
            string response = sendPostRequest(path, null);

            KrakenException e = parseKrakenException(response);
            if (e == null)
            {
                return parseResponseJson<KrakenBalanceResponse>(response).Balance;
            }
            else
            {
                throw e;
            }
        }

        public override bool SubmitLimitOrder(BasicOrderType ot, long amount, long price)
        {
            KrakenOrderDescription kos = new KrakenOrderDescription(KrakenOrderType.Limit, price, 0);
            KrakenOrderFlags kof = new KrakenOrderFlags(false, false, false);

            try
            {
                SubmitOrder(DefaultBaseCurrency, DefaultQuoteCurrency, kos, null, ot, amount, kof, 0, 0);
            }
            catch (KrakenException e)
            {
                return false;
            }

            return true;
        }

        public override bool SubmitMarketOrder(BasicOrderType ot, long amount)
        {
            KrakenOrderDescription kos = new KrakenOrderDescription(KrakenOrderType.Market, 0, 0);
            KrakenOrderFlags kof = new KrakenOrderFlags(false, false, true);

            try
            {
                SubmitOrder(DefaultBaseCurrency, DefaultQuoteCurrency, kos, null, ot, amount, kof, 0, 0);
            }
            catch (KrakenException e)
            {
                throw e;
            }

            return true;
        }

        public void SubmitOrder(Currency baseCurr, Currency quoteCurr,
                                KrakenOrderDescription baseOrder, KrakenOrderDescription closeOrder,
                                BasicOrderType type, long volume, KrakenOrderFlags flags,
                                long startTime, long expireTime)
        {
            string path = "/0/private/AddOrder";
            string currencyPair = KrakenUtils.GetCurrencyPairName(baseCurr, quoteCurr);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("pair", currencyPair);
            parameters.Add("type", ExchangeUtils.OrderTypeToString(type));
            parameters.Add("ordertype", KrakenUtils.KrakenOrderTypeToString(baseOrder.Type));
            parameters.Add("volume", ExchangeUtils.SatoshiToString(volume));
            parameters.Add("price", ExchangeUtils.SatoshiToString(baseOrder.Price));
            if (baseOrder.Price2 > 0)
            {
                parameters.Add("price2", ExchangeUtils.SatoshiToString(baseOrder.Price2));
            }
            if (flags.ToString() != "")
            {
                parameters.Add("oflags", flags.ToString());
            }
            if (startTime > 0)
            {
                parameters.Add("starttm", startTime.ToString());
            }
            if (expireTime > 0)
            {
                parameters.Add("expiretm", expireTime.ToString());
            }

            if (closeOrder != null)
            {
                parameters.Add("close[ordertype]", closeOrder.Type.ToString());
                parameters.Add("close[price]", ExchangeUtils.SatoshiToString(closeOrder.Price));
                if (closeOrder.Price2 > 0)
                {
                    parameters.Add("close[price2]", ExchangeUtils.SatoshiToString(closeOrder.Price2));
                }
            }

            string response = sendPostRequest(path, parameters);

            KrakenException ke = parseKrakenException(response);
            if (ke != null)
            {
                throw ke;
            }
        }

        // UNFINISHED
        public void GetOpenOrders(bool trades)
        {
            string path = "/0/private/OpenOrders";

            Dictionary<string, string> par = new Dictionary<string, string>();
            par.Add("trades", trades.ToString().ToLower());

            string response = sendPostRequest(path, par);

            System.Diagnostics.Debug.WriteLine(response);

            KrakenOpenOrdersResponse r = parseResponseJson<KrakenOpenOrdersResponse>(response);
        }

        public KrakenTradeVolume GetTradeVolume(List<Tuple<Currency, Currency>> pairs)
        {
            string path = "/0/private/TradeVolume";
            string pairList = "";
            bool isFirst = true;

            foreach (Tuple<Currency, Currency> cp in pairs)
            {
                if (!isFirst)
                {
                    pairList += ",";
                }
                pairList += KrakenUtils.GetCurrencyPairName(cp.Item1, cp.Item2);
            }
            
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("pair", pairList);

            string response = sendPostRequest(path, parameters);

            KrakenTradeVolumeResponse r = parseResponseJson<KrakenTradeVolumeResponse>(response);

            return r.Result;
        }

        public override double GetFee()
        {
            List<Tuple<Currency, Currency>> l = new List<Tuple<Currency, Currency>>();
            l.Add(Tuple.Create(DefaultBaseCurrency, DefaultQuoteCurrency));
            KrakenTradeVolume ktv = GetTradeVolume(l);

            return ktv.Fee[KrakenUtils.GetCurrencyPairName(DefaultBaseCurrency, DefaultQuoteCurrency)].CurrentFee;
        }

        public List<KrakenTransaction> GetTransactions(Currency c1, Currency c2)
        {
            string path = "/0/public/Trades";
            string CurrencyPair = KrakenUtils.GetCurrencyPairName(c1, c2);
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("pair", CurrencyPair);

            string response = sendGetRequest(path, parameters).Replace(CurrencyPair, "currencypair");

            KrakenTransactionListResponse parsedResponse = parseResponseJson<KrakenTransactionListResponse>(response);
            return parsedResponse.Result.TransactionList;
        }
    }
}
