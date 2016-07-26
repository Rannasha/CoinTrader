using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace StampClient
{
    public enum GraphStatus { Inactive, LoadingData, Active }; 

    class Graph
    {
        private GraphStatus _status;

        public int CanvasWidth { get; set; }
        public int CanvasHeight { get; set; }
        public int GraphPadding { get; set; }

        public GraphStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
            }
        }

        public Graph()
        {
            _status = GraphStatus.Inactive;
        }
        public Graph(int padding)
            : this()
        {
            GraphPadding = padding;
        }
        public Graph(int padding, int width, int height)
            : this(padding)
        {
            CanvasWidth = width;
            CanvasHeight = height;
        }

        public virtual void Draw(Graphics g)
        {
        }
        public virtual void Draw(Chart c)
        {
        }
    }

    class Candle
    {
        public double Open {get; set;}
        public double Close {get; set;}
        public double High {get; set;}
        public double Low {get; set;}

        public long Vol {get; set;}

        public Candle(double open, double close, double high, double low, long vol)
        {
            Open = open;
            Close = close;
            High = high;
            Low = low;
            Vol = vol;
        }
        public Candle(double open, double close, double high, double low) : this(open, close, high, low, 0)
        {
        }
        public Candle() : this(0,0,0,0,0)
        {
        }
    }


    class CandleStickGraph : Graph
    {
        private List<Transaction> _tl;

        public List<Transaction> TL
        {
            get
            {
                return _tl;
            }
            set
            {
                _tl = value;
            }
        }

        private long _minPrice;
        private long _maxPrice;
        private long _maxBins;
        private long _numBins;
        private long _maxLength;
        private long _minLength = 3600 * 12;

        private long _currLength;

        private List<Candle> _candles;

        public CandleStickGraph() : base()
        {
            _maxBins = 10;
            _candles = new List<Candle>();
        }
        public CandleStickGraph(int padding)
            : base(padding)
        {
            _maxBins = 10;
            _candles = new List<Candle>();
        }
        public CandleStickGraph(int padding, int width, int height)
            : base(padding, width, height)
        {
            _maxBins = 10;
            _candles = new List<Candle>();
        }

        private void CalcMaxLength()
        {
            long d1 = _tl[0].TimestampUnix;
            long d0 = _tl[_tl.Count - 1].TimestampUnix;

            _maxLength = d0 - d1;
        }

        private void SetLength(double lengthFrac)
        {
            _currLength = _minLength + (long)(lengthFrac * ((double)(_maxLength - _minLength)));
        }

        private long LengthPerBin()
        {
            return _currLength / _numBins;
        }

        private void FilledRectangle(Graphics g, Pen p, Brush b, int x, int y, int width, int height)
        {
            g.DrawRectangle(p, x, y, width, height);
            g.FillRectangle(b, x + 1, y + 1, width - 2, height - 2);
        }

        private void FillBins()
        {
            Candle prev = null;
            Candle c;

            _candles.Clear();

            long currT = _tl[0].TimestampUnix;

            for (int i = 0; i < _numBins; i++)
            {
                System.Diagnostics.Debug.WriteLine("currT = " + currT.ToString() + " -- currT + LPB = " + (currT + LengthPerBin()).ToString());
                var tl = (from Transaction t in _tl
                          where (t.TimestampUnix >= currT && t.TimestampUnix < currT + LengthPerBin())
                          select t).ToList();
                if (tl.Count == 0)
                {
                    if (prev == null)
                    {
                        c = new Candle(0, 0, 0, 0, 0);
                    }
                    else
                    {
                        c = prev;
                    }
                }
                else
                {
                    c = new Candle();
                    c.High = tl.Max(t => t.Rate);
                    c.Low = tl.Min(t => t.Rate);
                    c.Open = tl.Last().Rate;
                    c.Close = tl.First().Rate;
                    c.Vol = tl.Sum(t => t.Amount);
                }
                _candles.Add(c);
                prev = c;

                currT += LengthPerBin();

            }
        }
        
        public override void Draw(Chart c)
        {   
            int i = 0;
            _numBins = _maxBins;
            CalcMaxLength();
            SetLength(1.0);
            FillBins();
            double maxPrice = _candles.Max(ca => ca.High);
            double minPrice = _candles.Min(ca => ca.Low);
           
            foreach (Candle ca in _candles)
            {
                c.Series[0].Points.AddXY(i, ca.High, ca.Low, ca.Open, ca.Close);
                i++;
            }
        }
    }

    class DepthGraph : Graph
    {
        private OrderBook _ob;

        public OrderBook OrderBook
        {
            get
            {
                return _ob;
            }
            set
            {
                _ob = value;
            }
        }
        public double Range { get; set; }

        private double _minPrice;
        private double _maxPrice;
        private long _maxVol;

        public DepthGraph() : base()
        {
            Range = 0.3;
        }
        public DepthGraph(int padding)
            : base(padding)
        {
            Range = 0.3;
        }
        public DepthGraph(int padding, int width, int height)
            : base(padding, width, height)
        {
            Range = 0.3;
        }

        public override void Draw(Graphics g)
        {
            if (_ob.Bids.Count + _ob.Asks.Count == 0)
            {
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                g.DrawString("Orderbook is empty.", new Font("Microsoft Sans Serif", 7.8f), Brushes.Black, new RectangleF(0, 0, CanvasWidth, CanvasHeight), sf);

                return;
            }

            _ob.ComputeDepth(Range);
            
            _maxPrice = _ob.CumulativeAsks.Last().Key;
            _minPrice = _ob.CumulativeBids.Last().Key;
            _maxVol = Math.Max(_ob.CumulativeBids.Last().Value, _ob.CumulativeAsks.Last().Value);

            int grWidth = CanvasWidth - 2 * GraphPadding;
            int grHeight = CanvasHeight - 2 * GraphPadding;

            Pen p = new Pen(System.Drawing.Color.Black, 2);
            
            g.DrawLine(p, 5, 5, 5, CanvasHeight - 5);
            g.DrawLine(p, 5, CanvasHeight - 5, CanvasWidth - 5, CanvasHeight - 5);
            g.DrawLine(p, CanvasWidth - 5, 5, CanvasWidth - 5, CanvasHeight - 5);

            long prevX = Convert.ToInt64(grWidth * (_ob.CumulativeBids.Last().Key - _minPrice) / (_maxPrice - _minPrice)) + GraphPadding;
            long prevY = GraphPadding;
            p.Color = System.Drawing.Color.Green;

            foreach (KeyValuePair<double, long> kv in _ob.CumulativeBids.Reverse())
            {
                long x = Convert.ToInt64(grWidth * (kv.Key - _minPrice) / (_maxPrice - _minPrice)) + GraphPadding;
                long y = ((long)grHeight * (_maxVol - kv.Value) / (_maxVol)) + GraphPadding;

                g.DrawLine(p, prevX, prevY, x, y);

                prevX = x;
                prevY =y;
            }

            prevX = Convert.ToInt64(grWidth * (_ob.CumulativeAsks.First().Key - _minPrice) / (_maxPrice - _minPrice)) + GraphPadding;
            prevY = CanvasHeight - GraphPadding;

            p.Color = System.Drawing.Color.Red;
            foreach (KeyValuePair<double, long> kv in _ob.CumulativeAsks)
            {
                long x = Convert.ToInt64(grWidth * (kv.Key - _minPrice) / (_maxPrice - _minPrice)) + GraphPadding;
                long y = Convert.ToInt64(grHeight * (_maxVol - kv.Value) / (_maxVol)) + GraphPadding;

                g.DrawLine(p, prevX, prevY, x, y);

                prevX = x;
                prevY = y;
            }

            base.Draw(g);
            p = null;
        }
    }
}
