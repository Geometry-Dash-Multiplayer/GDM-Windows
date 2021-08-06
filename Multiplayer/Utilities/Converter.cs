using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Multiplayer.Utilities
{
    public static class Converter
    {
        public static  string BytesToString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();
        }
        public static Brush HexToBrush(string hex) {
            var converter = new System.Windows.Media.BrushConverter();
            return (Brush)converter.ConvertFromString(hex);
        }
        public static string ToString(short y) {
            return BytesToString(BitConverter.GetBytes(y)).ToLower();
        }
        public static byte[] StringToByteArray(string hex)
        {
            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }
        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }
}
