using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PusherClient;

namespace ExchangeHandlers
{
    public class StampSocket
    {
        const string _key = "de504dc5763aeef9ff52";
        private Pusher _pusher;
        private Channel _orderBookChannel;
        private Action<dynamic> _msgHandler;

        private void ConnectionStateChanged(object sender, ConnectionState state)
        {
        }

        private void Error(object sender, PusherException e)
        {
        }

        private void OrderBookSubscribed(object sender)
        {
        }

        public StampSocket(Action<dynamic> msgHandler)
        {
            _msgHandler = msgHandler;

            _pusher = new Pusher(_key);
            _pusher.ConnectionStateChanged += ConnectionStateChanged;
            _pusher.Error += Error;

            _orderBookChannel = _pusher.Subscribe("order_book");
            _orderBookChannel.Subscribed += OrderBookSubscribed;
            _orderBookChannel.Bind("data", _msgHandler);
        }

        public void InitPusher()
        {
            _pusher.Connect();
        }
    }
}
