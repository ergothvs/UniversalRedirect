using System;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace UniversalRedirect
{
    static class Program
    {
        //Server connection Settings
        public static string loginServerIP;//loginServerIP : IP of your loginserver.
        public static string serverWebsite;//URL to your server's website.
        public static string registerUrl;//URL to your site's register page.
        public static string serverName;//Server name to be shown on the application screen.
        public static string authServerIP = "mc.craftnet.nl";//If you're a scrub without a MSauth server, use Diamondo's: mc.craftnet.nl
        public static string authServerPort = "47611";//If you're a scrub without a MSauth server, use Diamondo's: 47611

        //DNS settings
        public static bool resolveDNS;

        //Game settings
        public static int gameVersion;
        public static string patchVersion; //subversion is after the dot. AKA 146.1 means: subversion = "1";
        public static byte locale = 8; //GMS locale = 8, change for other regions.

        //Port range (lowPort and highPort should cover all your channels and misc. servers like cash shop and farm)
        public static ushort lowPort;
        public static ushort highPort;

        //Launcher Mode, if showUI is false, it will just redirect without an extra launcher screen. (looks like a localhost!)
        public static bool showUI = false;
        public static bool useIniFiles = true;

        //etc...
        public static frmMain form;
        public static bool DevMode = false;

        [STAThread]
        static void Main()
        {
            if (isrunning())
            {
                Environment.Exit(0);
                return;
            }
            if (resolveDNS) getIP();
            
            //Could do something with launch params, but i don't..
            string[] launchaprams = Environment.GetCommandLineArgs();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            form = new frmMain();
            if (showUI)
            {
                Application.Run(form);
            }
            else
            {
                Application.Run();
            }
        }
        
        public static void OnLaunch()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            if (!File.Exists(currentDirectory + "/Maplestory.exe"))
            {
                MessageBox.Show("Please place this launcher in your Maplestory folder.");
                Application.Exit();
            }
            Process Maple = new Process();
            Maple.StartInfo.FileName = Path.Combine(currentDirectory, "Maplestory.exe");
            Maple.StartInfo.Arguments = "GameLaunching";
            Maple.Start();
        }

        public static bool isrunning()
        {
            string procName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            return System.Diagnostics.Process.GetProcessesByName(procName).Length > 1;
        }
        
        public static void getIP()
        {
            IPHostEntry entry = Dns.GetHostEntry(loginServerIP);
            if (entry.AddressList.Length > 0)
                loginServerIP = entry.AddressList[0].ToString();
        }
    }
}
