using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using MapleLib.MapleCryptoLib;
using MapleLib.PacketLib;

namespace UniversalRedirect
{
    public sealed class InterceptedLinkedClient
    {
        Session inSession;
        Session outSession;

        bool gotEnc = false;
        ushort Port;
        bool connected = true;
        bool block = false;
        int charID = -1;

        public InterceptedLinkedClient(Session inside, string toIP, ushort toPort)
        {
            this.Port = toPort;
            Debug.WriteLine("New linkclient to " + toIP);
            inSession = inside;
            inside.OnPacketReceived += new Session.PacketReceivedHandler(inside_OnPacketReceived);
            inside.OnClientDisconnected += new Session.ClientDisconnectedHandler(inside_OnClientDisconnected);
            ConnectOut(toIP, toPort);

            Debug.WriteLine("Connecting out to port " + toPort);
        }

        void inside_OnClientDisconnected(Session session)
        {
            if(outSession != null)
            outSession.Socket.Shutdown(SocketShutdown.Both);
            connected = false;
        }

        void ConnectOut(string ip, int port)
        {
            try
            {
                Socket outSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                outSocket.BeginConnect(ip, port, new AsyncCallback(OnOutConnectCallback), outSocket);
            }
            catch { outSession_OnClientDisconnected(null); }
        }

        private void OnOutConnectCallback(IAsyncResult ar)
        {
            Socket sock = (Socket)ar.AsyncState;
            try
            {
                sock.EndConnect(ar);
            }
            catch
            {
                connected = false;
                inSession.Socket.Shutdown(SocketShutdown.Both);
                return;
            }

            if (outSession != null)
            {
                outSession.Socket.Close();
                outSession.Connected = false;
            }
            Session session = new Session(sock, SessionType.CLIENT_TO_SERVER);
            outSession = session;
            outSession.OnInitPacketReceived += new Session.InitPacketReceived(outSession_OnInitPacketReceived);
            outSession.OnPacketReceived += new Session.PacketReceivedHandler(outSession_OnPacketReceived);
            outSession.OnClientDisconnected += new Session.ClientDisconnectedHandler(outSession_OnClientDisconnected);
            session.WaitForDataNoEncryption();
        }

        private volatile Mutex mutex = new Mutex();

        void inside_OnPacketReceived(byte[] packet)
        {
            if (!connected || block)
            {
                return;
            }
            mutex.WaitOne();
            try
            {
                short opcode = BitConverter.ToInt16(packet, 0);
                if (opcode == 0x27)
                {
                    charID = BitConverter.ToInt32(packet, 2);
                }
                else if (opcode == 0x38)
                {
                    //LoginData not needed due to MSAuth
                }
                else
                {
                    Debug.WriteLine("Unhandled Opcode (" + opcode + ")");
                }
                outSession.SendPacket(packet);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        void outSession_OnClientDisconnected(Session session)
        {
            if (block){ // simply changing channels, shouldn't happen though
                return;
            }
            inSession.Socket.Shutdown(SocketShutdown.Both);
            Debug.WriteLine("out disconnected (" + Port + ")");
            connected = false;
        }

        private volatile Mutex mutex2 = new Mutex();

        void outSession_OnPacketReceived(byte[] packet)
        {
            if (!gotEnc || !connected)
            {
                return;
            }
            mutex2.WaitOne();
            try
            {
                short opcode = BitConverter.ToInt16(packet, 0);
                Debug.WriteLine("Got a packet from server: " + opcode);
                if (opcode == 0x10)
                {
                    block = true;
                    short newPort = BitConverter.ToInt16(packet, 7);
                    ConnectOut(Program.loginServerIP, newPort);
                    return;
                }
                if (opcode == 0x0B)
                {
                    block = true;
                }
                inSession.SendPacket(packet);
            }
            finally
            {
                mutex2.ReleaseMutex();
            }
        }

        void outSession_OnInitPacketReceived(short version, byte locale)
        {
            Debug.WriteLine("Init packet: v" + version + "ident: " + locale);
            if (block)
            {
                connected = true;
                ChannelCompleteLogin();
                return;
            }
            SendHandShake(version, locale);
        }

        void ChannelCompleteLogin()
        {
            PacketWriter writer = new PacketWriter();
            writer.WriteShort(0x27);
            writer.WriteInt(charID);
            outSession.SendPacket(writer.ToArray());
            block = false;
            Debug.WriteLine("change channel complete.");
        }

        private void SendHandShake(short version, byte locale)
        {
            PacketWriter writer = new PacketWriter();
            writer.WriteShort(14);
            writer.WriteShort(Program.gameVersion);
            writer.WriteMapleString(Program.patchVersion);
            byte[] riv = new byte[4];
            byte[] siv = new byte[4];
            Random rand = new Random();
            rand.NextBytes(riv);
            rand.NextBytes(siv);
            inSession.RIV = new MapleCrypto(riv, version);
            inSession.SIV = new MapleCrypto(siv, version);
            writer.WriteBytes(riv);
            writer.WriteBytes(siv);

            //session --> byte locale = reader.ReadByte(); should match the locale automatically.
            writer.WriteByte(locale);//program.locale

            if (Program.gameVersion >= 160) writer.WriteByte(0);
            gotEnc = true;
            inSession.SendRawPacket(writer.ToArray());
        }
    }
}
