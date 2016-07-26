using System;
using System.Windows.Forms;
using ExchangeHandlers;

namespace StampSocketTester
{
    public partial class Form1 : Form
    {
        private StampSocket _socket;

        public Form1()
        {
            InitializeComponent();
        }

        public void MessageHandler(dynamic data)
        {
            Invoke(new Action(() =>
            {
                tbOutput.AppendText(data.ToString());
            })); 
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            _socket = new StampSocket(MessageHandler);
            _socket.InitPusher();
        }
    }
}
