using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    public class Connector
    {
        Func<Session> _seesionFactory; //클라이언트 접속 되면 접속 유무 콜백

        public void Connect(IPEndPoint endPoint, Func<Session> seesionFactory)
        {
            //휴대폰 설정(소켓)
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _seesionFactory = seesionFactory; //콜백 함수 연결

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Completeconnect);
            args.RemoteEndPoint = endPoint;
            args.UserToken = socket;
            RegisterConnect(args);
        }

        void RegisterConnect(SocketAsyncEventArgs args)
        {
            Socket socket = args.UserToken as Socket;
            if (socket == null)
                return;

            bool pending = socket.ConnectAsync(args);
            if (!pending)
                Completeconnect(null, args);
        }

        void Completeconnect(object send, SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success)
            {
                Session session = _seesionFactory.Invoke();
                session.Start(args.ConnectSocket);
                session.OnConnected(args.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine($"Completeconnect Faile : {args.SocketError}");
            }
        }


    }
}
