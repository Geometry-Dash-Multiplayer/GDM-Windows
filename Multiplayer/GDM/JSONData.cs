using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.GDM
{
    public class JSONData
    {
        public string FilePath;
        public FileStream Stream;
        public JSONData(string filepath) {
            Stream = new FileStream(Globals.Paths.JsonInputFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        }
        public void Write(byte[] s, bool reset = true) {
            if (reset) Clear();
            Stream.Position = 0;
            Stream.SetLength(s.Length);
            Stream.Write(s, 0, s.Length);
            Stream.Flush();
        }
        public void Write(List<Client> o) {
            string output = JsonConvert.SerializeObject(o);
            var bytes = Encoding.UTF8.GetBytes(output);
            Write(bytes, false);
        }
        public void Clear() {
            Stream.SetLength(0);
            Stream.Flush();
        }
    }
}
