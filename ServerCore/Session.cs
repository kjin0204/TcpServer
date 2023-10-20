using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    class Session
    {
        Socket _socket;
        int _disconneted = 0; //커넥션 확인 하는 플레그

        public void Start(Socket clientSocket)
        {
            _socket = clientSocket;

            SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
            receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            receiveArgs.SetBuffer(new byte[1024], 0, 1024);
            Registerrecv(receiveArgs);
        }

        public void Disconneted()
        {
            if ( Interlocked.CompareExchange(ref _disconneted, 1, 0) != 0)
                return;
            //예고 
            _socket.Shutdown(SocketShutdown.Both);
            //연결끊기
            _socket.Close();
        }

        public void sendData(byte[] sendBuffe)
        {
            _socket.Send(sendBuffe);
        }

        #region 네트워크 통신
        void Registerrecv(SocketAsyncEventArgs args)
        {
            bool pending = _socket.ReceiveAsync(args);
            if (!pending)
                OnRecvCompleted(null, args);
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    string ReceiveData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"[From Client] {ReceiveData}");
                    Registerrecv(args);
                }
                catch(Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e}");
                }
            }
            else
            {
                //Console.WriteLine($"SoketError {args.SocketError}");
            }

        }
        #endregion
    }
}
