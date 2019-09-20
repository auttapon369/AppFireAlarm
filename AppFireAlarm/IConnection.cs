using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppFireAlarm
{
    public delegate void ConnectionByteReceivedEventHandler(object sender, ConnectionByteReceivedEventArgs e);
    public delegate void OnMessageEventHandler(object sender, OnMessageEventArgs e);

    public interface IConnection
    {
        bool IsConnect { get; }

        event ConnectionByteReceivedEventHandler ByteReceived;

        event OnMessageEventHandler OnMessage;

        bool Connect();

        bool Disconnect();

        void SendData(Byte[] Data);
    }

    public class ConnectionByteReceivedEventArgs : EventArgs
    {
        private readonly byte _byteData;

        public ConnectionByteReceivedEventArgs(byte byteData)
        {
            _byteData = byteData;
        }

        public byte ByteData
        {
            get
            {
                return _byteData;
            }
        }
    }

    public class OnMessageEventArgs : EventArgs
    {
        public enum MsgType { Info, Warning, Error };
        private readonly string _msg;

        public string Msg
        {
            get { return _msg; }
        }

        private readonly MsgType _msgType;

        public MsgType MessageType
        {
            get { return _msgType; }
        }

        public OnMessageEventArgs(MsgType MessageType, String Msg)
        {
            this._msg = Msg;
            this._msgType = MessageType;
        }
    }

    public class SerialPortConnection : IConnection
    {
        private System.IO.Ports.SerialPort _port = new System.IO.Ports.SerialPort();

        public bool IsConnect
        {
            get
            {
                try
                {
                    return _port.IsOpen;
                }
                catch (Exception)
                {
                    //App.LogEvent("SerialPortConnection.IsConnect : " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
                    return false;
                }
            }
        }

        public event ConnectionByteReceivedEventHandler ByteReceived;

        public event OnMessageEventHandler OnMessage;

        public bool Connect()
        {
            if (IsConnect)
                Disconnect();
            try
            {
                _port.PortName = this.PortName;
                _port.BaudRate = this.Baud;
                _port.Open();
            }
            catch (Exception)
            {
                Disconnect();
                //App.LogEvent("SerialPortConnection.Connect : " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
                onMessage(new OnMessageEventArgs(OnMessageEventArgs.MsgType.Error, "เชื่อมต่อ " + PortName + " ไม่ได้"));
                return false;
            }
            return true;
        }

        private void onMessage(OnMessageEventArgs e)
        {
            if (OnMessage != null)
                OnMessage(this, e);
        }

        public bool Disconnect()
        {
            try
            {
                _port.Close();
            }
            catch (Exception)
            {
                //App.LogEvent("SerialPortConnection.Disconnect : " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
            return true;
        }

        public void SendData(byte[] Data)
        {
            throw new NotImplementedException();
        }

        public string PortName { get; set; }

        public int Baud { get; set; }

        public SerialPortConnection()
        {
            _port.ReceivedBytesThreshold = 1;
            _port.ReadTimeout = 200;
            _port.DataBits = 7;
            _port.Parity = System.IO.Ports.Parity.Even;
            _port.Handshake = System.IO.Ports.Handshake.RequestToSend;
            Baud = 9600;
            PortName = "COM4";
            _port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(_port_DataReceived);
        }

        void _port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            while (_port.BytesToRead > 0)
            {
                int byteData = _port.ReadByte();
                if (byteData >= 0)
                    OnByteReceived(new ConnectionByteReceivedEventArgs((byte)byteData));
                if (!_port.IsOpen)
                    break;
            }
        }

        private void OnByteReceived(ConnectionByteReceivedEventArgs e)
        {
            if (ByteReceived != null)
                ByteReceived(this, e);
        }
    }

    public class TcpConnection : IConnection
    {
        private System.Net.Sockets.TcpClient _tcpClient = new System.Net.Sockets.TcpClient();

        public bool IsConnect
        {
            get
            {
                try
                {
                    return _tcpClient.Connected;
                    //_socket.Connected; 
                }
                catch (Exception)
                {
                    //App.LogEvent("TcpConnection.IsConnect : " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
                    return false;
                }
            }
        }

        public event ConnectionByteReceivedEventHandler ByteReceived;

        public event OnMessageEventHandler OnMessage;
        private Byte[] buffer = new byte[1000];

        public bool Connect()
        {
            if (IsConnect)
                Disconnect();
            try
            {
                _tcpClient = new System.Net.Sockets.TcpClient();
                _tcpClient.BeginConnect(IP, Port, new AsyncCallback(OnConnected), _tcpClient.Client);
            }
            catch (Exception)
            {
                Disconnect();
                //App.LogEvent("TcpConnection.Connect : " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
                onMessage(new OnMessageEventArgs(OnMessageEventArgs.MsgType.Error, "เชื่อมต่อ " + IP.ToString() + ":" + Port.ToString() + " ไม่ได้"));
                return false;
            }
            onMessage(new OnMessageEventArgs(OnMessageEventArgs.MsgType.Info, "เปิดการเชื่อมต่อ " + IP.ToString() + ":" + Port.ToString()));
            return true;
        }

        public bool Disconnect()
        {
            try
            {
                _tcpClient.Close();
            }
            catch (Exception)
            {
                //App.LogEvent("TcpConnection.Disconnect : " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
            //onMessage(new OnMessageEventArgs(OnMessageEventArgs.MsgType.Info, "ปิดการเชื่อมต่อ " + IP.ToString() + ":" + Port.ToString()));
            return true;
        }

        public void SendData(byte[] Data)
        {
            throw new NotImplementedException();
        }

        public System.Net.IPAddress[] IP { get; set; }

        public int Port { get; set; }

        void OnConnected(IAsyncResult iar)
        {
            try
            {
                System.Net.Sockets.Socket remote = (System.Net.Sockets.Socket)iar.AsyncState;
                remote.EndConnect(iar);
                onMessage(new OnMessageEventArgs(OnMessageEventArgs.MsgType.Info, "การเชื่อมต่อ " + IP.ToString() + ":" + Port.ToString() + " เรียบร้อย"));
                remote.BeginReceive(buffer, 0, 1000, System.Net.Sockets.SocketFlags.None, new AsyncCallback(OnDataReceived), _tcpClient.Client);
            }
            catch (Exception)
            {
                //App.LogEvent("TcpConnection.OnDataReceived : " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
                onMessage(new OnMessageEventArgs(OnMessageEventArgs.MsgType.Error, "การสื่อสาร " + IP.ToString() + ":" + Port.ToString() + " ขัดข้อง"));
            }
        }

        void OnDataReceived(IAsyncResult iar)
        {
            try
            {
                System.Net.Sockets.Socket remote = (System.Net.Sockets.Socket)iar.AsyncState;
                int recv = remote.EndReceive(iar);
                for (int i = 0; i < recv; i++)
                {
                    OnByteReceived(new ConnectionByteReceivedEventArgs(buffer[i]));
                }
                if (remote.Connected)
                    remote.BeginReceive(buffer, 0, 1000, System.Net.Sockets.SocketFlags.None, new AsyncCallback(OnDataReceived), remote);
            }
            catch (Exception)
            {
                //App.LogEvent("TcpConnection.OnDataReceived : " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
                onMessage(new OnMessageEventArgs(OnMessageEventArgs.MsgType.Error, "การสื่อสาร " + IP.ToString() + ":" + Port.ToString() + " ขัดข้อง"));
            }
        }

        private void OnByteReceived(ConnectionByteReceivedEventArgs e)
        {
            if (ByteReceived != null)
                ByteReceived(this, e);
        }

        private void onMessage(OnMessageEventArgs e)
        {
            if (OnMessage != null)
                OnMessage(this, e);
        }
    }
}
