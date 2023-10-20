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
        static void OnacceptHandler(Socket socket)
        {
            try
            {
                Console.WriteLine("Listening...");
                //손님입장
                Socket clientSocket = socket;

                // 받는다.(블로킹 함수)
                byte[] recvBuff = new byte[1024];
                int recvBytes = clientSocket.Receive(recvBuff);
                string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                Console.WriteLine(recvData);

                //전송 한다.(블로킹 함수)
                byte[] sendBuffe = Encoding.UTF8.GetBytes("welcome to MMORPG Server !...");
                clientSocket.Send(sendBuffe);

                //예고 
                clientSocket.Shutdown(SocketShutdown.Both);
                //연결끊기
                clientSocket.Close();
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
