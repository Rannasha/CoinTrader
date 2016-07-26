using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using ExchangeHandlers;

namespace StampClient
{

    [DataContract]
    public class Ticker
    {
        [DataMember(Name = "last")]
        public double Last;
        [DataMember(Name = "high")]
        public double High;
        [DataMember(Name = "low")]
        public double Low;
        [DataMember(Name = "volume")]
        public double Volume;
        [DataMember(Name = "bid")]
        public double Bid;
        [DataMember(Name = "ask")]
        public double Ask;
    }

    [DataContract]
    public class Transaction
    {
        [DataMember(Name = "price")]
        public double Rate;
        [DataMember(Name = "amount")]
        private double _amount;
        [DataMember(Name = "tid")]
        public long Id;
        [DataMember(Name = "date")]
        public long TimestampUnix;

        public long Amount;
        public DateTime Timestamp;

        [OnDeserialized()]
        internal void ProcessDeserializedFields(StreamingContext context)
        {
            Amount = ExchangeUtils.DoubleToSatoshi(_amount);
            Timestamp = ExchangeUtils.UnixTimeStampToDateTime(TimestampUnix);
        }

        public Transaction()
        {
        }
        public Transaction(long timestamp, double price, long amount)
        {
            TimestampUnix = timestamp;
            Timestamp = ExchangeUtils.UnixTimeStampToDateTime(TimestampUnix);
            Rate = price;
            Amount = amount;
        }
    }

    [DataContract]
    public class UserTransaction
    {
        [DataMember(Name = "usd")]
        public double AmountUSD;
        [DataMember(Name = "btc")]
        public double AmountBTC;
        [DataMember(Name = "btc_usd")]
        public double Rate;
        [DataMember(Name = "order_id")]
        public int? OrderId;
        [DataMember(Name = "fee")]
        public double Fee;
        [DataMember(Name = "type")]
        public int Type;
        [DataMember(Name = "id")]
        public int Id;
        [DataMember(Name = "datetime")]
        private string _datetime;
        public DateTime Timestamp;

        [OnDeserialized()]
        internal void ProcessDeserializedFields(StreamingContext context)
        {
            Timestamp = DateTime.Parse(_datetime);
        }
    }

    [DataContract]
    public class Order
    {
        [DataMember(Name = "id")]
        public int OrderId;
        [DataMember(Name = "type")]
        public int Type;
        [DataMember(Name = "datetime")]
        public string Timestamp;
        [DataMember(Name = "price")]
        public double Price;
        [DataMember(Name = "amount")]
        public double Amount;
    }

    [DataContract]
    public class StampBalance : Balance
    {
        [DataMember(Name = "usd_balance")]
        private double _balanceUSD;
        [DataMember(Name = "btc_balance")]
        private double _balanceBTC;
        [DataMember(Name = "usd_available")]
        private double _availableUSD;
        [DataMember(Name = "btc_available")]
        private double _availableBTC;
        [DataMember(Name = "fee")]
        public double Fee;

        public Dictionary<Currency, long> AvailableBalance;

        [OnDeserialized()]
        internal void ProcessDeserializedFields(StreamingContext context)
        {
            TotalBalance = new Dictionary<Currency, long>();
            AvailableBalance = new Dictionary<Currency, long>();

            TotalBalance.Add(Currency.USD, ExchangeUtils.DoubleToSatoshi(_balanceUSD));
            TotalBalance.Add(Currency.XBT, ExchangeUtils.DoubleToSatoshi(_balanceBTC));
            AvailableBalance.Add(Currency.USD, ExchangeUtils.DoubleToSatoshi(_availableUSD));
            AvailableBalance.Add(Currency.XBT, ExchangeUtils.DoubleToSatoshi(_availableBTC));
        }
    }

    [DataContract]
    public class ExchangeRate
    {
        [DataMember(Name = "sell")]
        public double Sell;
        [DataMember(Name = "buy")]
        public double Buy;

        public double Rate
        {
            get
            {
                return (Buy + Sell) / 2;
            }
        }
    }

    [DataContract]
    public class StampExceptionDetails
    {
        [DataMember]
        private KeyValuePair<string, string> error { get; set; }

        public string ErrorType;
        public string ErrorMessage;

        [OnDeserialized()]
        internal void ProcessDeserializedFields(StreamingContext context)
        {
            ErrorType = error.Key;
            ErrorMessage = error.Value;
        }
    }

    public class StampException : Exception
    {
        public StampExceptionDetails Details { get; set; }
        public override string Message
        {
            get
            {
                return "(" + Details.ErrorType + ") " + Details.ErrorMessage;
            }
        }
        
        public StampException() : base() { }
        public StampException(string message) : base(message) { }
        public StampException(string message, System.Exception inner) : base(message, inner) { }
        public StampException(StampExceptionDetails details) : this()
        {
            Details = details;
        }
    }

    class StampLink : ExchangeLink
    {
        private string _apiSecret;
        private string _apiKey;
        private string _clientId;

        private string hexDigestUpper(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "").ToUpper();
        }

        private string generateSignature(string secret, string message)
        {
            HMACSHA256 h = new HMACSHA256();
            h.Key = Encoding.UTF8.GetBytes(secret);
            byte[] msg = Encoding.UTF8.GetBytes(message);
            byte[] hash = h.ComputeHash(msg);

            return hexDigestUpper(hash);
        }

        private string sendGetRequest(string uri, Dictionary<string, string> parameters)
        {
            string getStr = uri;
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

        private string sendPostRequest(string uri, Dictionary<string, string> parameters)
        {
            Dictionary<string, string> postParams = new Dictionary<string, string>();
            postParams.Add("key", _apiKey);
            long nonce = DateTime.Now.Ticks;
            string signature = generateSignature(_apiSecret, nonce.ToString() + _clientId + _apiKey);
            postParams.Add("signature", signature);

            foreach (KeyValuePair<string, string> p in parameters)
            {
                postParams.Add(p.Key, p.Value);
            }
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
            try
            {
                System.Net.HttpWebRequest request = System.Net.HttpWebRequest.Create(uri) as System.Net.HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
                request.ContentType = "application/x-www-form-urlencoded";
                System.Diagnostics.Debug.WriteLine(postStr);
                request.Method = "POST";
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

        private T parseResponseJson<T>(string json)
        {
            if (json.IndexOf("Incapsula") > 0)
            {
                StampExceptionDetails sed = new StampExceptionDetails();
                sed.ErrorType = "Network";
                sed.ErrorMessage = "API returned Incapsula-page, Bitstamp down.";

                throw new StampException(sed);
            }
            if (json.IndexOf("IP address not allowed") > 0)
            {
                StampExceptionDetails sed = new StampExceptionDetails();
                sed.ErrorType = "Unauthorized";
                sed.ErrorMessage = "IP address not allowed";

                throw new StampException(sed);
            }
            T res = default(T);
            DataContractJsonSerializerSettings s = new DataContractJsonSerializerSettings();
            s.UseSimpleDictionaryFormat = true;
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T), s);
            using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                object obj = ser.ReadObject(ms);
                res = (T)Convert.ChangeType(obj, typeof(T));
            }
            return res;
        }

        // Would prefer to use a DataContract-style deserialization here,
        // but the structure of the error-json prevents this from being done
        // without including external libraries such as Json.NET.
        // So we use the dirty approach.
        //
        // Layout of the json-string:
        // {"error": {"TYPE": ["MESSAGE"]}}
        private StampExceptionDetails parseErrorJson(string json)
        {
            StampExceptionDetails sed = new StampExceptionDetails();

            string temp = json.Substring(12);
            int pos = temp.IndexOf('"');
            sed.ErrorType = temp.Substring(0, pos);
            pos = temp.IndexOf('[');
            int pos2 = temp.IndexOf(']');
            sed.ErrorMessage = temp.Substring(pos + 2, pos2 - pos - 4);

        return sed;
        }

        public StampLink(string clientId, string apiKey, string apiSecret)
        {
            _clientId = clientId;
            _apiKey = apiKey;
            _apiSecret = apiSecret;
        }

        public List<UserTransaction> GetUserTransactions()
        {
            string uri = "https://www.bitstamp.net/api/user_transactions/";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("offset", "0");
            parameters.Add("limit", "100");
            parameters.Add("sort", "desc");

            string response = sendPostRequest(uri, parameters);

            return parseResponseJson<List<UserTransaction>>(response);
        }

        public List<Order> GetOrders()
        {
            string uri = "https://www.bitstamp.net/api/open_orders/";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            string response = sendPostRequest(uri, parameters);

            return parseResponseJson<List<Order>>(response);
        }

        public override Balance GetBalance()
        {
            string uri = "https://www.bitstamp.net/api/balance/";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            string response = sendPostRequest(uri, parameters);

            return parseResponseJson<StampBalance>(response);
        }

        public override OrderBook GetOrderBook()
        {
            return GetOrderBook(true);
        }
        public OrderBook GetOrderBook(bool groupOrders)
        {return null;}
    /*        string uri = "https://www.bitstamp.net/api/order_book/";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (!groupOrders)
            {
                parameters.Add("group", "0");
            }

            string response = sendGetRequest(uri, parameters);
            */
       //     return parseResponseJson<StampOrderBook>(response);
        //}

        public List<Transaction> GetTransactions(bool hour)
        {
            string uri = "https://www.bitstamp.net/api/transactions/";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (!hour)
            {
                parameters.Add("time", "minute");
            }

            string response = sendGetRequest(uri, parameters);

            List<Transaction> tl = parseResponseJson<List<Transaction>>(response);
            tl.Reverse();

            return tl;
        }

        public ExchangeRate GetEurUsdRate()
        {
            string uri = "https://www.bitstamp.net/api/eur_usd/";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            string response = sendGetRequest(uri, parameters);

            return parseResponseJson<ExchangeRate>(response);
        }

        public Ticker GetTicker()
        {
            string uri = "https://www.bitstamp.net/api/ticker/";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            string response = sendGetRequest(uri, parameters);

            return parseResponseJson<Ticker>(response);
        }

        public bool CancelOrder(long orderId)
        {
            string uri = "https://www.bitstamp.net/api/cancel_order/";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("id", orderId.ToString());

            string response = sendPostRequest(uri, parameters);

            System.Diagnostics.Debug.WriteLine(response);

            if (response == "true")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool SubmitLimitOrder(BasicOrderType ot, long amount, long price)
        {
            SubmitOrder(amount, ExchangeUtils.SatoshiToDouble(price), ot);
            return true;
        }

        public override bool SubmitMarketOrder(BasicOrderType ot, long amount)
        {
            try
            {
                if (ot == BasicOrderType.BUY)
                {
                    SubmitOrder(amount, 2000, ot);
                }
                else
                {
                    SubmitOrder(amount, 1, ot);
                }
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public Order SubmitOrder(long amount, double price, BasicOrderType ot)
        {
            string orderType = ot == BasicOrderType.BUY ? "buy" : "sell";
            string uri = "https://www.bitstamp.net/api/" + orderType + "/";

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("amount", ExchangeUtils.SatoshiToString(amount));
            parameters.Add("price", Math.Round(price, 2).ToString());

            string response = sendPostRequest(uri, parameters);
            System.Diagnostics.Debug.WriteLine(response);

            if (response.Contains("error"))
            {
                StampExceptionDetails sed = parseErrorJson(response);
                throw (new StampException(sed));
            }
            else
            {
                return parseResponseJson<Order>(response);
            }
        }

        public override double GetFee()
        {
            StampBalance b = (StampBalance)GetBalance();
            return b.Fee;
        }
    }
}
