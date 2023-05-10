using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.GDM
{
    public class Preferences
    {
        public double Version = 1.9f;
        public string WindowName = "Geometry Dash";
        public int NewThreadDelay = 20;
        public double FileSystemversion = 2;
        public string MainModule = "GeometryDash.exe";
        public int PlayerID = 0;
        public DateTime Used = DateTime.Now;
        public byte[] Key = new byte[] { };
        public List<int> BlockedIDs = new List<int>();
        public int IsAlizer = 0;
        public bool CachedIcons = true;
        public bool CachedLevels = true;
        public string ServerIp = "";
        public bool CachedUsernames = true;
        public float PlayersOpacity = 1.0f;
        public bool ShowSelfUsername = true;
        public bool ShowSelfRainbow = false;
        public bool ShowSelfRainbowPastel = true;

        public byte R = 255;
        public byte G = 255;
        public byte B = 255;

        public string Lang = null;
    }
}