using System;
using System.IO;
using System.Windows.Forms;

namespace UniversalRedirect.Tools
{
    /*
     * @Author Novak
     *
     * Check/load all .ini files for this thing to work!
     *
     */

    class FileChecker
    {
        public static void isMaplePresent(string currentDirectory)
        {
            if (!File.Exists(currentDirectory + "/Maplestory.exe"))
            {
                MessageBox.Show("Please place this launcher in your Maplestory folder.");
                Application.Exit();
            }
        }

        public static void checkIniFiles(string currentDirectory)
        {
            if (!File.Exists(currentDirectory + "/RedirectionSettings.ini"))
            {
                //Init values
                Program.loginServerIP = "127.0.0.1";//loginServerIP : IP of your loginserver.
                Program.serverWebsite = "http://127.0.0.1/";//URL to your server's website.
                Program.registerUrl = "http://127.0.0.1/register";//URL to your site's register page.
                Program.serverName = "LocalHostedMS";//Server name to be shown on the application screen.

                //DNS settings
                Program.resolveDNS = false;

                //Game settings
                Program.gameVersion = 146;
                Program.patchVersion = "1"; //subversion is after the dot. AKA 146.1 means: subversion = "1";
                Program.locale = 8; //GMS locale = 8, change for other regions.

                //Port range (lowPort and highPort should cover all your channels and misc. servers like cash shop and farm)
                Program.lowPort = 8585;
                Program.highPort = 8605;

                //Create new Settings Ini file.
                IniHandler settings = new IniHandler(currentDirectory + "/RedirectionSettings.ini");
                settings.IniWriteValue("Config", "serverIP", Program.loginServerIP);
                settings.IniWriteValue("Config", "website", Program.serverWebsite);
                settings.IniWriteValue("Config", "registerUrl", Program.registerUrl);
                settings.IniWriteValue("Config", "serverName", Program.serverName);
                settings.IniWriteValue("Config", "gameVersion", "146");
                settings.IniWriteValue("Config", "patchVersion", Program.patchVersion);
                settings.IniWriteValue("Config", "locale", Program.locale.ToString());
                settings.IniWriteValue("Config", "portRangeMin", "8585");
                settings.IniWriteValue("Config", "portRangeMax", "8605");
            }
        }

        public static void loadInifiles(string currentDirectory)
        {
            if (Program.useIniFiles && File.Exists(currentDirectory + "/RedirectionSettings.ini"))
            {
                //Read from .ini file
                IniHandler settings = new IniHandler(currentDirectory + "/RedirectionSettings.ini");
                //Init values
                Program.loginServerIP = settings.IniReadValue("Config", "serverIP");
                Program.serverWebsite = settings.IniReadValue("Config", "website");
                Program.registerUrl = settings.IniReadValue("Config", "registerUrl");
                Program.serverName = settings.IniReadValue("Config", "serverName");

                //DNS settings
                Program.resolveDNS = false;

                //Game settings
                Program.gameVersion = Int32.Parse(settings.IniReadValue("Config", "gameVersion"));
                Program.patchVersion = settings.IniReadValue("Config", "patchVersion");
                Program.locale = (byte)Int32.Parse(settings.IniReadValue("Config", "locale"));

                //Port range (lowPort and highPort should cover all your channels and misc. servers like cash shop and farm)
                Program.lowPort = (ushort)Int32.Parse(settings.IniReadValue("Config", "portRangeMin"));
                Program.highPort = (ushort)Int32.Parse(settings.IniReadValue("Config", "portRangeMax"));
            }
        }

        public static void checkAuthIni(string currentDirectory)
        {
            if (!File.Exists(currentDirectory + "/nmconew.ini"))
            {
                //Create new MSAuth Ini file.
                IniHandler msauth = new IniHandler(currentDirectory + "/nmconew.ini");
                msauth.IniWriteValue("Settings", "ServerIp", Program.authServerIP);
                msauth.IniWriteValue("Settings", "ServerPort", Program.authServerPort);
            }
        }

    }
}
