using System;
using System.Net;
using System.Text;
using System.Threading;
using ServerCore;

namespace Server
{

    class Knight
    {
        public int hp;
        public int attack;
    }

    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            byte[] sendBuffe = Encoding.UTF8.GetBytes("welcome to MMORPG Server !...");
            Send(sendBuffe);

            Thread.Sleep(1000);

            Disconneted();
            Disconneted();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string ReceiveData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Client] {ReceiveData}");

            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"SendData Byte : {numOfBytes}");
        }
    }


    class Program
    {
        static Listener _listener = new Listener();


        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipaddr = ipHost.AddressList[0]; //아이피가 여러개 있을수 있으며 배열로 ip를 반환함
            IPEndPoint endPoint = new IPEndPoint(ipaddr, 7777);

            _listener.init(endPoint, () => { return new GameSession(); });

            while (true)
            {

            }
        }
    }
}
