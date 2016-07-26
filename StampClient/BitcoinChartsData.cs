using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace StampClient
{
    public class BitcoinChartsData
    {
        private const string _fullHistUrl = "http://api.bitcoincharts.com/v1/csv/";
        private const string _histUrl = "http://api.bitcoincharts.com/v1/trades.csv?symbol=";

        private long _lastData;
        private List<Transaction> _txList;

        public string Symbol { get; set; }
        public long LastData { get { return _lastData; } }

        private string GetFullHistUrl()
        {
            return _fullHistUrl + Symbol + ".csv";
        }

        // startTime >= 0: All trades since "startTime" (UNIX timestamp)
        // startTime <0: last 2000 trades
        private string GetHistUrl(long startTime)
        {
            return _histUrl + Symbol + (startTime >= 0 ? "&start=" + startTime.ToString() : "");
        }

        private long GetHistByteSize()
        {
            FileInfo fi = new FileInfo(Symbol + ".csv");
            if (fi.Exists)
            {
                return fi.Length;
            }
            else
            {
                return 0;
            }
        }

        public BitcoinChartsData(string symbol)
        {
            Symbol = symbol;
            _lastData = 0;
            _txList = new List<Transaction>();
        }

        public void Update()
        {
            string c;
            long now = ExchangeUtils.DateTimeToUnixTimeStamp(DateTime.Now);
            long dt = now - _lastData;

            // If our last data-point is more than 5 days in the past (or non-existent), we need to download the full
            // history from BitcoinCharts.
            if (dt > 5 * 24 * 3600)
            {
                System.Net.HttpWebRequest request = System.Net.HttpWebRequest.Create(GetFullHistUrl()) as System.Net.HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "GET";
                using (System.Net.HttpWebResponse response = request.GetResponse() as System.Net.HttpWebResponse)
                {
                    System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream());

                    c = reader.ReadToEnd();
                }
                string[] lines = c.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in lines)
                {
                    string[] fields = s.Split(new Char[] { ',' });

                    long timestamp = Convert.ToInt64(fields[0]);
                    double price = Convert.ToDouble(fields[1]);
                    long amount = ExchangeUtils.StringToSatoshi(fields[2]);
                }

            }
            // Do some stuff, build a transaction-list
            _lastData = ExchangeUtils.DateTimeToUnixTimeStamp(_txList.Last().Timestamp);

            now = ExchangeUtils.DateTimeToUnixTimeStamp(DateTime.Now);
            dt = now - _lastData;

            // If we're more than 50 minutes behind, chances are we won't be able to obtain what we need from just the
            // Bitstamp API, so we have to query BitcoinCharts for additional data, up to 5 days worth of history.
            if (dt > 3000)
            {
                System.Net.HttpWebRequest request = System.Net.HttpWebRequest.Create(GetHistUrl(_lastData)) as System.Net.HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "GET";
                using (System.Net.HttpWebResponse response = request.GetResponse() as System.Net.HttpWebResponse)
                {
                    System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream());

                    c = reader.ReadToEnd();
                }
                // Parse data here.
            }
        }

        public void SaveToFile(string fileName)
        {
        }

        public void LoadFromFile(string filename)
        {
        }
    }

}
