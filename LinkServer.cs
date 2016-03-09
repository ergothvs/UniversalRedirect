using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace UniversalRedirect
{
    public sealed class LinkServer
    {
        public Socket sListener;
        public string LinkIP = "127.0.0.1";
        public ushort Port;

        public LinkServer(ushort port, string toIP)
        {
            LinkIP = toIP;
            Port = port;
            try
            {
                sListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sListener.Bind(new IPEndPoint(IPAddress.Any, port));
                sListener.Listen(10);
                sListener.BeginAccept(new AsyncCallback(OnConnect), sListener);
            }
            catch
            {
                Debug.WriteLine("Unable to establish a listener in linkServer() at port: " + port + " and ip: " + toIP);
            }
        }

        private void OnConnect(IAsyncResult ar)
        {
            Socket client = sListener.EndAccept(ar);
            Debug.WriteLine("Created normal linkedclient");
            LinkClient lClient = new LinkClient(client, Port, LinkIP);
            sListener.BeginAccept(new AsyncCallback(OnConnect), sListener);
        }
    }
}
