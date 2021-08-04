using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Multiplayer.Utilities.SaveFile
{
    public static class Save_File_Descryptor
    {
        public static string UserSaveFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/GeometryDash/CCGameManager.dat";

        public static int GetPlayerID() {
			if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/GeometryDash/"))
				UserSaveFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Geometry Dash/CCGameManager.dat";
			try
			{
				string j = File.ReadAllText(UserSaveFilePath);
				string h = XorBD(j).Replace("_", "/").Replace("-", "+");
				var b = Convert.FromBase64String(h);
				var g = GZipDecompress(b);
				string a = Encoding.UTF8.GetString(g);

				GDM.Player_Watcher.Memory.Icons = new byte[] {
					(byte)int.Parse(Utils.FindTextBetween(a, "<k>playerFrame</k><i>", "</i>")),
					(byte)int.Parse(Utils.FindTextBetween(a, "<k>playerShip</k><i>", "</i>")),
					(byte)int.Parse(Utils.FindTextBetween(a, "<k>playerBall</k><i>", "</i>")),
					(byte)int.Parse(Utils.FindTextBetween(a, "<k>playerBird</k><i>", "</i>")),
					(byte)int.Parse(Utils.FindTextBetween(a, "<k>playerDart</k><i>", "</i>")),
					(byte)int.Parse(Utils.FindTextBetween(a, "<k>playerRobot</k><i>", "</i>")),
					(byte)int.Parse(Utils.FindTextBetween(a, "<k>playerSpider</k><i>", "</i>")),
				};
				GDM.Player_Watcher.Memory.Col1 = (byte)int.Parse(Utils.FindTextBetween(a, "<k>playerColor</k><i>", "</i>"));
				GDM.Player_Watcher.Memory.Col2 = (byte)int.Parse(Utils.FindTextBetween(a, "<k>playerColor2</k><i>", "</i>"));

				string playerID = Utils.FindTextBetween(a, "<k>playerUserID</k><i>", "</i>");
				int q;
				if (int.TryParse(playerID, out q))
				{
					return q;
				}
				return 0;
			}
			catch {
				return 0;
			}
        }

        public static string XorBD(string s) {
			List<byte> h = new List<byte>();
			for (int i = 0; i < s.Length; i++)
			{
				var f = (byte)(s[i] ^ 11);
				if (f != 0) h.Add(f);
			}
			return Encoding.UTF8.GetString(h.ToArray());
		}
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
