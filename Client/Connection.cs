using System.Net;
using System.Text;
using System.Net.Sockets;

namespace Client
{
    class Connection
    {
        public static string IP = "127.0.0.1";
        public static int port = 2000;

        public static Socket Sock = null;
        public static IPHostEntry HostEntry = Dns.GetHostEntry(IP);

        public static void ConnectLoop()
        {


            do
            {
                foreach (IPAddress address in HostEntry.AddressList)
                {
                    IPEndPoint ipe = new IPEndPoint(address, port);
                    Socket tempSocket =
                        new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    tempSocket.Connect(ipe);

                    if (tempSocket.Connected)
                    {
                        Sock = tempSocket;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            while (!Sock.Connected);
        }

        public static string RetrieveMessage()
        {
            int bytes = 0;
            byte[] BytesReceived = new byte[256];
            string Command = "";

            do
            {
                bytes = Sock.Receive(BytesReceived, BytesReceived.Length, 0);
                Command += Encoding.ASCII.GetString(BytesReceived, 0, bytes);
            }
            while (bytes > 0);

            return Command;
        }

        public static void Send(string message)
        {
            Sock.Send(Encoding.ASCII.GetBytes(message));
        }
    }
}
