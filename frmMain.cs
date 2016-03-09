using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;
using MapleLib.PacketLib;



namespace UniversalRedirect
{
    public enum MapleMode
    {
        MSEA,
        EMS,
        GMS
    }

    public partial class frmMain : Form
    {
        public string installpath = "";
        private string TempFolder = "";

        int tries = 0;
        Process Maple;
        private NotifyIcon trayIcon;
        public ContextMenu trayMenu;
        public Dictionary<ushort, LinkServer> Servers = new Dictionary<ushort, LinkServer>();
        public Dictionary<ushort, Listener> Listeners = new Dictionary<ushort, Listener>();

        public frmMain()
        {
            //Check if the ini files are present, load their data.
            Tools.FileChecker.checkIniFiles();

            //Retrieve key's from diamondo's server.
            tools.MapleKeys.Initialize();

            //Create/start Form. (aka the window of the application)
            InitializeComponent();
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Bisque;
            this.TransparencyKey = Color.Bisque;
            TempFolder = Path.GetTempPath();
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Launch " + Program.serverName + "!", OnStartButton);
            trayMenu.MenuItems.Add("Website", OnSiteButton);
            trayMenu.MenuItems.Add("Close", OnExit);
            trayIcon = new NotifyIcon();
            trayIcon.Text = Program.serverName + ((Program.DevMode) ? " - DEV" : "");
            trayIcon.Icon = new Icon(this.Icon, 40, 40);
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
            
            //Check session type
            if (Program.loginServerIP != "127.0.0.1")
            {
                //Init tunnels
                new Thread(StartLoading).Start(); 
            }
            else
            {
                //Only init netsh
                StartLoading();
            }
            if (Program.showUI == false)
            {
                LaunchMaple();
            }
        }

        private void OnSiteButton(object sender, EventArgs e)
        {
            Process.Start(Program.registerUrl);
        }

        private void OnShow(object sender, EventArgs e)
        {
            this.Show();
            this.Visible = true;
        }

        private void OnStartButton(object sender, EventArgs e)
        {
            LaunchMaple();
        }

        private void OnExit(object sender, EventArgs e)
        {
            try
            {
                if (Maple != null)
                    Maple.Kill();
            }
            catch { }
            frmMain_FormClosed(null, null);
            Environment.Exit(0);
        }

        void StartLoading()
        {
            routeIP();
            if (Program.loginServerIP != "127.0.0.1")
            {
                StartTunnels();
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            btnLogin.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }

        public void LaunchMaple()
        {
            Program.OnLaunch();
        }
          
        public void StartTunnels()
        {
            try
            {
                ushort lowPort = Program.lowPort;
                ushort highPort = Program.highPort;
                string toIP = Program.loginServerIP;
                bool AllLink = false;
                if (AllLink)
                {
                    LinkServer loginServer = new LinkServer(8484, toIP);
                    ushort count = (ushort)(highPort - lowPort);
                    for (ushort i = 0; i <= count; i++)
                    {
                        LinkServer server = new LinkServer((ushort)(lowPort + i), toIP);
                    }
                }
                else
                {
                    Listener lListener = new Listener();
                    Debug.WriteLine("Listening on 8484");
                    lListener.OnClientConnected += new Listener.ClientConnectedHandler(listener_OnClientConnected);
                    lListener.Listen(8484);

                    ushort count = (ushort)(highPort - lowPort);
                    for (ushort i = 0; i <= count; i++)
                    {
                        Listener listener = new Listener();
                        listener.OnClientConnected += new Listener.ClientConnectedHandler(listener_OnClientConnected);
                        listener.Listen((ushort)(lowPort + i));
                        Debug.WriteLine("Listening on " + (lowPort + i).ToString());
                        Listeners.Add((ushort)(lowPort + i), listener);
                    }
                }
            }
            catch
            {
                Debug.WriteLine("Unable to start tunnels.");
            }
        }

        void listener_OnClientConnected(Session session, ushort port)
        {
            Debug.WriteLine("Accepted connection on " + port);
            InterceptedLinkedClient lClient = new InterceptedLinkedClient(session, Program.loginServerIP, port);
        }

        
        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            FixALL();
        }

        public void FixALL()
        {
            trayIcon.Visible = false;
            trayIcon.Dispose();
            derouteIP();
        }

        private void routeIP()
        {
            string arguments = "int ip add addr 1 address=" + nexonIp() + "mask=255.255.255.0 st=ac";
            ProcessStartInfo procStartInfo = new ProcessStartInfo("netsh", arguments);

            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;

            Process.Start(procStartInfo);
        }

        private void derouteIP()
        {
            string arguments = "int ip delete addr 1" + nexonIp();
            ProcessStartInfo procStartInfo = new ProcessStartInfo("netsh", arguments);

            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;

            Process.Start(procStartInfo);
        }
        
        private void btnLogin_Click_1(object sender, EventArgs e)
        {
            btnLogin.Enabled = true;
            LaunchMaple();
            this.Hide();
            ShowInTaskbar = false;
            btnLogin.Enabled = false;
        }

        private string nexonIp()
        {
            if (Program.gameVersion >= 135)
            {
                return "8.31.99.141";
            }
            else {
                return "8.31.98.52";
            }
        }
    }
}
