using System;
using System.Net.Sockets;
using DGScope.Library;
using System.Threading;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Receivers.Beast
{
    public abstract class TcpClientReceiver : AdsbReceiver
    {
        private TcpClient _client = new TcpClient();
        private bool stop = true;
        private NetworkStream networkStream;
        //private System.Timers.Timer tmrReceiveTimeout = new System.Timers.Timer();
        //private System.Timers.Timer tmrSendTimeout = new System.Timers.Timer();
        private System.Timers.Timer tmrConnectTimeout = new System.Timers.Timer(5000);
        private byte[] dataBuffer = new byte[5000];
        
        public string Host { get; set; }
        public int Port { get; set; } = 30005;
        public bool AutoReconnect { get; set; } = true;
        public int ReconnectInterval { get; set; } = 5000;
        public int ConnectTimeout 
        {
            get
            {
                return (int)tmrConnectTimeout.Interval;
            }
            set
            {
                tmrConnectTimeout.Interval = value;
            }
        }
        public override void Start()
        {
            stop = false;
            _client.BeginConnect(Host, Port, new AsyncCallback(ConnectCallback), _client);
        }

        public override void Stop()
        {
            stop = true;
            _client.Client.Disconnect(false);
        }
        private void ConnectCallback(IAsyncResult result)
        {
            var client = result.AsyncState as TcpClient;
            if (client == null)
                throw new InvalidOperationException("Invalid IAsyncResult - Could not interpret as a client object");
            if (!client.Connected)
            {
                if (AutoReconnect)
                {
                    Thread.Sleep(ReconnectInterval);
                    Action reconnect = new Action(Reconnect);
                    reconnect.Invoke();
                    tmrConnectTimeout.Start();
                    return;
                }
                else
                    return;
            }
            client.EndConnect(result);
            var callBack = new Action(cbConnectComplete);
            callBack.Invoke();
        }
        
        private void Reconnect()
        {
            if (!_client.Client.Connected)
                _client.BeginConnect(Host, Port, new AsyncCallback(ConnectCallback), _client);
        }
        private void cbConnectComplete()
        {
            if (_client.Connected)
                _client.Client.BeginReceive(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, new AsyncCallback(cbDataReceived), _client.Client);
        }
        private void cbDataReceived(IAsyncResult result)
        {
            byte[] buffer = new byte[_client.Client.EndReceive(result)];
            Array.Copy(dataBuffer, buffer, buffer.Length);
            DataReceived(buffer);
            if (_client.Connected)
                _client.Client.BeginReceive(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, new AsyncCallback(cbDataReceived), _client.Client);
        }

        protected abstract void DataReceived(byte[] buffer);
    }
}
