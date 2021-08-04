using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Utilities.Encryption
{
    public static class Xor
	{
		public static string XorBD(string s)
		{
			List<byte> h = new List<byte>();
			for (int i = 0; i < s.Length; i++)
			{
				var f = (byte)(s[i] ^ 11);
				if (f != 0) h.Add(f);
			}
			return Encoding.UTF8.GetString(h.ToArray());
		}
	}
}
