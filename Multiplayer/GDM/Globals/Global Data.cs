using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Multiplayer.GDM.Globals
{
    public static class Global_Data
    {
        public static double Version = 5;
        public static int KeySize = 4; // for now, somehow secure
        public static int GIFDelay = 10;
        public static int RefreshRate = 30;
        public static string LanguageFile = "Language.json";
        public static string InjectorName = "Injector.exe";
        public static bool IsGDThere = false;
        public static bool AllowCache = true;
        public static MainWindow Main;
        public static Server Connection;
        public static Initialize Initializer;
        public static Utilities.JSON_Models.Language Lang = new Utilities.JSON_Models.Language();

        public static bool ShowUsernames = false;
        public static bool IsPlayingLevel = false;
        public static bool ReceiveNewClients = true;
        public static bool IsConnected = false;
        public static bool IsInjected = false;
        public static bool VIPKeyOk = true;
        public static int LevelID = 0;
        public static byte[] ClientID;
        public static string ClientName;
        public static int StandardPort = 7010;
        public static string ActiveServer = ""; // 194.233.71.142
        public static string[] ServerIPs = new string[] { "158.69.122.197", "194.233.71.142", "104.248.33.114"};
        public static int VipKey = 0;

        public static bool PlayerIDLoaded = false;

        public static int PlayerID = 0;
        public static string Username = "";
        public static JSONData JSONCommunication;
        public static string VersionLink = "https://adaf.xyz/gdm/update.version";
        public static WindowState MainState = WindowState.Normal;
        public static short Room = 0; // public
        public static string DLLPath = AppDomain.CurrentDomain.BaseDirectory + @"\Multiplayer.dll";
        public static Utilities.JSON_Models.Players ActiveModel;
        public static bool LevelStatsShow = false;
        public static void HandleException(Exception ex)
        {
            Utilities.Utils.HandleException(ex);
        }
    }
}
