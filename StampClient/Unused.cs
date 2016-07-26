using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StampClient
{

    /* Graph-stuff
            private void CreateDepthGraph(StampLink s, DepthGraph d)
            {
                dg.OrderBook = s.GetOrderBook(true);
            }

            private void CreateDepthGraphCompleted()
            {
                bwLink.Dispose();
                dg.Status = GraphStatus.Active;
                pnlDepthGraph_Paint(this, null);
            }

            private void CreatePriceGraphCompleted()
            {
                MessageBox.Show("Yolo!");
                bwLink2.Dispose();
                pg.Status = GraphStatus.Active;
                pg.Draw(chart1);
            }

            private void CreatePriceGraph(StampLink s, CandleStickGraph c)
            {
                c.TL = s.GetTransactions(true);
            }

            private void CreateGraphs()
            {
                dg = new DepthGraph(5, pnlDepthGraph.Width, pnlDepthGraph.Height);
                bwLink = new BackgroundWorker();
                bwLink.DoWork += (obj, ea) => CreateDepthGraph(sl, dg);
                bwLink.RunWorkerCompleted += (obj, ea) => CreateDepthGraphCompleted();
                bwLink.RunWorkerAsync();
                pg = new CandleStickGraph(5, 0, 0);
                BackgroundWorker bwLink2 = new BackgroundWorker();
                bwLink2.DoWork += (obj, ea) => CreatePriceGraph(sl, pg);
                bwLink2.RunWorkerCompleted += (obj, ea) => CreatePriceGraphCompleted();
                bwLink2.RunWorkerAsync();
                dg.Status = GraphStatus.LoadingData;
                pg.Status = GraphStatus.LoadingData;
            }

            private void pnlDepthGraph_Paint(object sender, PaintEventArgs e)
            {
                Graphics g = pnlDepthGraph.CreateGraphics();
                g.Clear(this.BackColor);

                if (dg.Status == GraphStatus.Active)
                {
                    dg.Draw(g);
                }

                if (dg.Status == GraphStatus.LoadingData)
                {
                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    g.DrawString("Loading order book...", this.Font, Brushes.Black, new RectangleF(0, 0, pnlDepthGraph.Width, pnlDepthGraph.Height), sf);
                }

                if (dg.Status == GraphStatus.Inactive)
                {
                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    g.DrawString("No ticker selected.", this.Font, Brushes.Black, new RectangleF(0, 0, pnlDepthGraph.Width, pnlDepthGraph.Height), sf);
                }
            }

            private void pnlDepthGraph_Resize(object sender, EventArgs e)
            {
                dg.CanvasWidth = pnlDepthGraph.Width;
                dg.CanvasHeight = pnlDepthGraph.Height;
            }

            private void button2_Click(object sender, EventArgs e)
            {
                CreateGraphs();
            }

            private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
            {

              
            }

            private void hScrollBar1_ValueChanged(object sender, EventArgs e)
            {
                if (dg != null)
                {
                    dg.Range = ((double)hScrollBar1.Value) / 100;
                    pnlDepthGraph_Paint(this, null);
                }
            }

            private void button3_Click(object sender, EventArgs e)
            {
                BitcoinChartsData bcd = new BitcoinChartsData("bitstampUSD");
                bcd.Update();
            } */

}
