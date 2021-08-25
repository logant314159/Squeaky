using System.Net;
using System.Text;
using System.Net.Sockets;

namespace Squeaky.Client
{
    public static class Connection
    {
        private static Socket Sock = null;
        private static IPHostEntry HEntry = Dns.GetHostEntry(Settings.SERVER);

        public static void BlockUntilConnected()
        {
            do
            {
                foreach (IPAddress address in HEntry.AddressList)
                {
                    IPEndPoint ipe = new IPEndPoint(address, Settings.SERVERPORT);
                    Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    tempSocket.Connect(ipe);

                    if (tempSocket.Connected)
                    {
                        Sock = tempSocket;
                        break;
                    }
                    continue;
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
                Command += Encoding.UTF8.GetString(BytesReceived, 0, bytes);
            }
            while (bytes > 0);

            return Command;
        }

        public static void SendMessage(string message)
        {
            Sock.Send(Encoding.UTF8.GetBytes(message));
        }
    }
}
