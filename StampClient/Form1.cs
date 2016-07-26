using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StampClient
{
    public partial class StampClientMainWindow : Form
    {
        private ArbAgent KrakenStampArb;

        //private DepthGraph dg;
        //private CandleStickGraph pg;

        private List<Transaction> fullHist;

        StampLink sl;
        KrakenLink kl;

        public StampClientMainWindow()
        {
            InitializeComponent();
            Tuple<string, string> stampKeys = ReadKeyFromFile("bitstamp.key");
            Tuple<string, string> krakenKeys = ReadKeyFromFile("kraken.key");
            sl = new StampLink("03634", stampKeys.Item1, stampKeys.Item2);
            kl = new KrakenLink(krakenKeys.Item1, krakenKeys.Item2);
            KrakenStampArb = new ArbAgent(kl, sl, this);
        }

        private Tuple<string, string> ReadKeyFromFile(string filename)
        {
            System.IO.StreamReader f = new System.IO.StreamReader(filename);
            string line1 = f.ReadLine();
            string line2 = f.ReadLine();

            return Tuple.Create<string, string>(line1, line2);
        }

        private void LogToFile(string msg)
        {
            const string fileName = "difflog.txt";

            using (System.IO.StreamWriter sw = System.IO.File.AppendText(fileName))
            {
                sw.WriteLine(msg);
            }
        }

        private void LogToTextBox(string msg, int type)
        {
            TextBox target;

            switch (type)
            {
                case 0:
                    MethodInvoker action = delegate
                    {
                        tbOutput.AppendText("[" + DateTime.Now.ToString() + "] " + msg + Environment.NewLine);
                    };
                    tbOutput.BeginInvoke(action);
                    break;
                case 1:
                    MethodInvoker action2 = delegate { tbXBTBalance.Text = msg; };
                    tbXBTBalance.BeginInvoke(action2);
                    break;
                case 2:
                    MethodInvoker action3 = delegate { tbUSDBalance.Text = msg; };
                    tbUSDBalance.BeginInvoke(action3);
                    break;
                case 3:
                    MethodInvoker action4 = delegate { tbProfit.Text = msg; };
                    tbProfit.BeginInvoke(action4);
                    break;
                case 4:
                    LogToFile(msg);
                    break;

            }


        }

        private void button4_Click(object sender, EventArgs e)
        {
            KrakenStampArb.Logger = LogToTextBox;
            btnUpdateMinProfit_Click(this, null);
            KrakenStampArb.StartAgent();
        }

        private void btnUpdateMinProfit_Click(object sender, EventArgs e)
        {
            double minProfit = 0.0;
            try
            {
                minProfit = Convert.ToDouble(tbMinProfit.Text);
                KrakenStampArb.MinProfitPercentage = minProfit;
            }
            catch (Exception ex)
            {
                LogToTextBox("Invalid value for min profit.", 0);
                minProfit = KrakenStampArb.MinProfitPercentage;
            }
            finally
            {
                tbMinProfit.Text = minProfit.ToString();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tbOutput.AppendText(kl.GetFee().ToString() + Environment.NewLine);
            tbOutput.AppendText(sl.GetFee().ToString());
        }
    }
}