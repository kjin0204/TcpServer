using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    public abstract class Session
    {
        Socket _socket;
        int _disconneted = 0; //커넥션 확인 하는 플레그

        RecvBuffer _recvBuffer = new RecvBuffer(1024);

        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        Queue<byte[]> _sendQue = new Queue<byte[]>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _receiveArgs = new SocketAsyncEventArgs();
        object _sendLock = new object();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        public void Start(Socket clientSocket)
        {
            _socket = clientSocket;

            _receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
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

        public void Send(byte[] sendBuffe)
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
            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _receiveArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);
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
                    //Write 커서 이동
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconneted();
                        return;
                    }

                    //컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리햇는지 받는다.
                    int ProcessLen = OnRecv(_recvBuffer.Readsegment);
                    if (ProcessLen < 0 || _recvBuffer.DataSize < ProcessLen)
                    {
                        Disconneted();
                        return;
                    }

                    //Read 커서이동
                    if (_recvBuffer.OnRead(ProcessLen) == false)
                    {
                        Disconneted();
                        return;
                    }


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
