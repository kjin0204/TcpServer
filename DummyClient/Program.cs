using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DummyClient
{
    class Program
    {


        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipaddr = ipHost.AddressList[0]; //아이피가 여러개 있을수 있으며 배열로 ip를 반환함
            IPEndPoint endPoint = new IPEndPoint(ipaddr, 7777);

            while (true)
            {
                try
                {
                    //휴대폰 설정(소켓)
                    Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    //문지기한테 입장 문의
                    socket.Connect(endPoint);
                    Console.WriteLine($"Conneted to {socket.RemoteEndPoint.ToString()}");

                    for (int i = 0; i < 5; i++)
                    {
                        //보낸다.(블로킹 함수)
                        byte[] sendbuff = Encoding.UTF8.GetBytes($"Hellow world! {i}");
                        int sendByte = socket.Send(sendbuff);
                    }

                    //받는다.(블로킹 함수)
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = socket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[From server] {recvData}");

                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                Thread.Sleep(100);
            }
        }
    }
}
