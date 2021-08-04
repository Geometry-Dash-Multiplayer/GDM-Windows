using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Utilities.Encryption
{
    public static class GZip
    {
        public static byte[] GZipDecompress(byte[] data)
        {
            byte[] array;
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    using (MemoryStream memoryStream1 = new MemoryStream())
                    {
                        byte[] numArray = new byte[4096];
                        while (true)
                        {
                            int num = gZipStream.Read(numArray, 0, (int)numArray.Length);
                            int num1 = num;
                            if (num <= 0)
                            {
                                break;
                            }
                            memoryStream1.Write(numArray, 0, num1);
                        }
                        array = memoryStream1.ToArray();
                    }
                }
            }
            return array;
        }
    }
}
