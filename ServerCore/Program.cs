using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        static Listener _listener = new Listener();
        static void OnacceptHandler(Socket clientSocket)
        {
            try
            {
                byte[] sendBuffe = Encoding.UTF8.GetBytes("welcome to MMORPG Server !...");

                Session session = new Session();
                session.Start(clientSocket);
                session.sendData(sendBuffe);

                Thread.Sleep(1000);

                session.Disconneted();
                session.Disconneted();
                //session.Disconneted();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipaddr = ipHost.AddressList[0]; //아이피가 여러개 있을수 있으며 배열로 ip를 반환함
            IPEndPoint endPoint = new IPEndPoint(ipaddr, 7777);

            _listener.init(endPoint, OnacceptHandler);

            while (true)
            {

            }
        }
    }
}
