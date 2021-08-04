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
        public static double Version = 3.25;
        public static int KeySize = 4; // for now, somehow secure
        public static int GIFDelay = 10;
        public static int RefreshRate = 16;
        public static string LanguageFile = "Language.json";
        public static string InjectorName = "Injector.exe";
        public static bool IsGDThere = false;
        public static bool AllowCache = true;
        public static MainWindow Main;
        public static Server Connection;
        public static Initialize Initializer;
        public static Utilities.JSON_Models.Language Lang = new Utilities.JSON_Models.Language();

        public static bool HideUsernames = false;
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
        public static string[] ServerIPs = new string[] { "51.75.52.158", "192.99.245.3", "194.233.71.142" };
        public static int VipKey = 0;

        public static bool PlayerIDLoaded = false;
        public static int PlayerID = 0;
        public static int BaseAddr = 0x3222D0; //
        public static int Player1 = 0x224;
        public static int Player2 = 0x228;
        public static int[] RealTimePlayerOffsets1 = new int[] { 0x164, Player1 };
        public static int[] RealTimePlayerOffsets2 = new int[] { 0x164, Player2 };
        public static int Xpos = 0x67C;
        public static int Ypos = 0x680;
        public static int Xrotation = 0x020;
        public static int Yrotation = 0x024;
        public static int PlayerSize = 0x644;
        public static int PlayerForm = 0x638;
        public static int IsGlow = 0x27C;
        public static int AddrPlayerID = 0x1BC;
        public static int IconFormOffset = 0x638;
        public static int GravityOffset = 0x63E;
        public static int[] LevelObjectsCountOffsets = new int[] { 0x164, 0x22c, 0x114, 0x1d8 };
        public static int[] IsDead = new int[] { 0x164, 0x39C };
        public static int[] LevelLengthOffets1 = new int[] { 0x164, 0x3B4 };
        public static float LevelLength = 0;
        public static int[] AttemptOffsets1 = new int[] { 0x164, 0x39C };
        public static int[] AttemptOffsets2 = new int[] { 0x164, 0x39C };
        public static int LevelIDOffset = 0x2A0;
        public static int AttemptAddr = 0;
        public static string Username = "";
        public static JSONData JSONCommunication;
        public static string VersionLink = "https://adaf.xyz/gdm/update.version";
        public static WindowState MainState = WindowState.Normal;
        public static short Room = 0; // public
        public static string DLLPath = AppDomain.CurrentDomain.BaseDirectory + @"\Multiplayer.dll";
        public static Utilities.JSON_Models.Players ActiveModel;
        public static bool LevelStatsShow = false;
        public static string GDBrowserEndpoint = "95.111.251.138:2000"; // -1 if no shit
            // "194.233.71.142:8080";
        public static void HandleException(Exception ex)
        {
            Utilities.Utils.HandleException(ex);
        }
    }
}
