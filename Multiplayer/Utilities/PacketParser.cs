using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Utilities
{
    public static class PacketParser
    {
        public static byte[] IntToByteArr(int i) {
            return BitConverter.GetBytes(i);
        }
        public static byte[] CreateDisconnect()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Prefixes.Disconnext);
            bytes.AddRange(GDM.Globals.Global_Data.ClientID);
            
            bytes.AddRange(GDM.Globals.Global_Data.Main.UserPref.ClientKey_fix);
            return bytes.ToArray();
        }
        public static byte[] CreateHello() {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Prefixes.Hello);
            bytes.AddRange(GDM.Globals.Global_Data.ClientID);
            bytes.AddRange(GDM.Globals.Global_Data.Main.UserPref.ClientKey_fix);
            return bytes.ToArray();
        }
        public static byte[] CreatePing()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Prefixes.Ping);
            bytes.AddRange(GDM.Globals.Global_Data.ClientID);
            bytes.AddRange(GDM.Globals.Global_Data.Main.UserPref.ClientKey_fix);
            bytes.AddRange(Encoding.UTF8.GetBytes("i like sex"));
            return bytes.ToArray();
        }
        public static byte[] OutsideLevel()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Prefixes.OutsideLevel);
            bytes.AddRange(GDM.Globals.Global_Data.ClientID);
            bytes.AddRange(GDM.Globals.Global_Data.Main.UserPref.ClientKey_fix);
            return bytes.ToArray();
        }
        public static byte[] GetIcon(int clientID)
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Prefixes.Ping);
            bytes.AddRange(GDM.Globals.Global_Data.ClientID);
            bytes.AddRange(GDM.Globals.Global_Data.Main.UserPref.ClientKey_fix);
            bytes.AddRange(BitConverter.GetBytes(clientID));
            return bytes.ToArray();
        }
        public static byte[] CreateLobby(short lobbyCode) {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Prefixes.Ping);
            bytes.AddRange(GDM.Globals.Global_Data.ClientID);
            bytes.AddRange(GDM.Globals.Global_Data.Main.UserPref.ClientKey_fix);
            bytes.Add((byte)Prefixes.VipActions);
            bytes.AddRange(BitConverter.GetBytes(GDM.Globals.Global_Data.VipKey));
            bytes.AddRange(BitConverter.GetBytes(lobbyCode));
            return bytes.ToArray();
        }
        public static byte[] CreateIconsSend(byte[] Icons)
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Prefixes.ReceivedPlayerIcons);
            bytes.AddRange(GDM.Globals.Global_Data.ClientID);
            bytes.AddRange(GDM.Globals.Global_Data.Main.UserPref.ClientKey_fix);
            bytes.AddRange(Icons);

            return bytes.ToArray();
        }
        public static byte[] CreateMessage(GDM.PositionMemory P1, GDM.PositionMemory P2,  byte isDead, int levelid, byte col1, byte col2, byte glow, short room, byte[] IconIDs)
        {
            List<byte> bytes = new List<byte>();

            bytes.Add((byte)Prefixes.Message);
            bytes.AddRange(GDM.Globals.Global_Data.ClientID);
            bytes.AddRange(GDM.Globals.Global_Data.Main.UserPref.ClientKey_fix);

            bytes.AddRange(BitConverter.GetBytes(P1.x_position));
            bytes.AddRange(BitConverter.GetBytes(P1.y_position));
            bytes.AddRange(BitConverter.GetBytes(P1.x_rotation));
            bytes.AddRange(BitConverter.GetBytes(P1.y_rotation));
            bytes.Add(P1.gamemode);
            bytes.Add(P1.activeIconID);
            bytes.AddRange(BitConverter.GetBytes(P1.size));
            bytes.Add(P1.gravity);

            bytes.AddRange(BitConverter.GetBytes(P2.x_position));
            bytes.AddRange(BitConverter.GetBytes(P2.y_position));
            bytes.AddRange(BitConverter.GetBytes(P2.x_rotation));
            bytes.AddRange(BitConverter.GetBytes(P2.y_rotation));
            bytes.Add(P2.gamemode);
            bytes.Add(P2.activeIconID);
            bytes.AddRange(BitConverter.GetBytes(P2.size));
            bytes.Add(P2.gravity);

            bytes.Add(isDead);
            bytes.AddRange(BitConverter.GetBytes(levelid));
            bytes.AddRange(BitConverter.GetBytes(room));
            bytes.Add(col1);
            bytes.Add(col2);
            bytes.Add(glow);
            bytes.AddRange(IconIDs);
            return bytes.ToArray();
        }
    }
}
