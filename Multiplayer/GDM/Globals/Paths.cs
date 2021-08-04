using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.GDM.Globals
{
    public static class Paths
    {
        public static string DataFolder = Path.GetFullPath(Path.GetTempPath() + "/gdm");
        public static string GDMTempDataFile = Path.GetFullPath(DataFolder + "/gdm.dat");
        public static string JsonInputFile = Path.GetFullPath(DataFolder + "/in.dat");
        public static string IconsFolder = Path.GetFullPath(DataFolder + "/icons");
        public static string TempIcons = Path.GetFullPath(DataFolder + "/temp_icons");
        public static string SelfIconsFolder = Path.GetFullPath(IconsFolder + "/0") + "/";

        public static string LevelsCache = Path.GetFullPath(DataFolder + "/levels_cache.dat");
        public static string UsernamesCache = Path.GetFullPath(DataFolder + "/usernames_cache.dat");
        public static string UserDataFile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Alizer/gdm.dat";
    }
}
