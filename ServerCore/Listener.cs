using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    class Listener
    {
        Socket _listenSocket;
        Action<Socket> _onacceptHandler; //클라이언트 접속 되면 접속 유무 콜백

        
        public void init(IPEndPoint endPoint, Action<Socket> onacceptHandler)
        {
            //문지기
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _onacceptHandler += onacceptHandler;

            //문지기 교육
            _listenSocket.Bind(endPoint);

            //최대 대기수
            _listenSocket.Listen(10);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            //클라 접속 되면 연결 해준 함수로 콜백 해줌
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Onacceptcompleted);
            RegisterAccept(args);

        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;
            bool pending = _listenSocket.AcceptAsync(args);
            if (!pending) // pending true면 아직 접속된 클라가 없다는 뜻.
                Onacceptcompleted(null,args);
        }

        void Onacceptcompleted(object sender , SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                _onacceptHandler.Invoke(args.AcceptSocket);
            }
            else
                Console.WriteLine(SocketError.SocketError);

            RegisterAccept(args);
        }

        public Socket Accept()
        {
            return _listenSocket.Accept();
        }
    }
}
