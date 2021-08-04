using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Utilities
{
    public static class Files_and_Pathing
    {
        public static void ValidateDirectory(string dirname)
        {
            if (!Directory.Exists(dirname)) Directory.CreateDirectory(dirname);
        }
        public static void ValidateFile(string filename)
        {
            if (!File.Exists(filename)) File.Create(filename).Close();
        }
    }
}
