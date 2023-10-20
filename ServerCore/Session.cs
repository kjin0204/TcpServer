using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    abstract class Session
    {
        Socket _socket;
        int _disconneted = 0; //커넥션 확인 하는 플레그

        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        Queue<byte[]> _sendQue = new Queue<byte[]>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _receiveArgs = new SocketAsyncEventArgs();
        object _sendLock = new object();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        public void Start(Socket clientSocket)
        {
            _socket = clientSocket;

            _receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _receiveArgs.SetBuffer(new byte[1024], 0, 1024);
            RegisterRecv();

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
        }

        public void Disconneted()
        {
            if (Interlocked.CompareExchange(ref _disconneted, 1, 0) != 0)
                return;

            OnDisconnected(_socket.RemoteEndPoint);

            //예고 
            _socket.Shutdown(SocketShutdown.Both);
            //연결끊기
            _socket.Close();
        }

        public void sendData(byte[] sendBuffe)
        {
            lock (_sendLock)
            {
                _sendQue.Enqueue(sendBuffe);
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
            //_socket.Send(sendBuffe);
        }

        #region 네트워크 통신

        void RegisterSend()
        {

            while (_sendQue.Count > 0)
            {
                byte[] buff = _sendQue.Dequeue();
                _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
            }
            _sendArgs.BufferList = _pendingList;

            bool pending = _socket.SendAsync(_sendArgs);
            if (!pending)
                OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_sendLock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _pendingList.Clear();
                        _sendArgs.BufferList = null;

                        OnSend(args.BytesTransferred);

                        if (_sendQue.Count > 0)
                            RegisterSend();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }
                else
                {
                    Disconneted();
                }
            }
        }

        void RegisterRecv()
        {
            bool pending = _socket.ReceiveAsync(_receiveArgs);
            if (!pending)
                OnRecvCompleted(null, _receiveArgs);
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));
                    RegisterRecv();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e}");
                }
            }
            else
            {
                Disconneted();
            }

        }
        #endregion
    }
}
