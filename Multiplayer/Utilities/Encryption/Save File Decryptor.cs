using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Multiplayer.Utilities.Encryption
{
    public static class Save_File_Decryptor
    {
        public static string UserSaveFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/GeometryDash/CCGameManager.dat";

        public static int GetPlayerID() {
			if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/GeometryDash/"))
				UserSaveFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Geometry Dash/CCGameManager.dat";
			try
			{
				string j = File.ReadAllText(UserSaveFilePath);
				string h = Xor.XorBD(j).Replace("_", "/").Replace("-", "+");
				var b = Convert.FromBase64String(h);
				var g = GZip.GZipDecompress(b);
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

				GDM.Player_Watcher.Memory.AccountID = int.Parse(Utils.FindTextBetween(a, "<k>GJA_003</k><i>", "</i>"));

				try {
					// player colors are not in the savefile by default this is fixed in newer gdm
					GDM.Player_Watcher.Memory.Col1 = (byte)int.Parse(Utils.FindTextBetween(a, "<k>playerColor</k><i>", "</i>"));
					GDM.Player_Watcher.Memory.Col2 = (byte)int.Parse(Utils.FindTextBetween(a, "<k>playerColor2</k><i>", "</i>"));

				} catch { }
			
			
			
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
	}
}
