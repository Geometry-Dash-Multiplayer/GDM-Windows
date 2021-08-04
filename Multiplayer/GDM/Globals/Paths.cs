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
        public static readonly string DataFolder = Path.GetFullPath(Path.GetTempPath() + "/gdm");
        public static readonly string GDMTempDataFile = Path.GetFullPath(DataFolder + "/gdm.dat");
        public static readonly string JsonInputFile = Path.GetFullPath(DataFolder + "/in.dat");
        public static readonly string IconsFolder = Path.GetFullPath(DataFolder + "/icons");
        public static readonly string TempIcons = Path.GetFullPath(DataFolder + "/temp_icons");
        public static readonly string SelfIconsFolder = Path.GetFullPath(IconsFolder + "/0") + "/";

        public static readonly string LevelsCache = Path.GetFullPath(DataFolder + "/levels_cache.dat");
        public static readonly string UsernamesCache = Path.GetFullPath(DataFolder + "/usernames_cache.dat");
        public static readonly string UserDataFile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Alizer/gdm.dat";
    }
}
