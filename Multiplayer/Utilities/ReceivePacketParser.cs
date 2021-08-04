using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Utilities
{
    public class ReceivePacketParser
    {
        public Prefixes prefix;

        public byte[] buffer;
        private int position = 0;
        private int bytesleft = 0;

        public string ErrorMessage = "Unkown error, server just disconnected you.";
        public GDM.PositionMemory player1;
        public GDM.PositionMemory player2;
        public byte col1, col2, activeiconid, isglow, isded, isvip;
        public int levelID, clientID;
        public int PlayersCount = 0;
        public byte[] IconIDs = new byte[] {
                 0x0 /* cube */,
                 0x0 /* ship */,
                 0x0 /* ball */,
                 0x0 /* ufo */,
                 0x0 /* wave */,
                 0x0 /* robot */,
                 0x0 /* spider */,
        };

        public ReceivePacketParser(byte[] _buffer, bool Header = false)
        {
            buffer = _buffer;
            bytesleft = buffer.Length;
            prefix = (Prefixes)readByte();
            switch (prefix)
            {
                case Prefixes.Disconnext:
                    ErrorMessage = readStringToEnd();
                    break;
                case Prefixes.Ping:
                    // do fucking nothing
                    PlayersCount = readInt32();
                    break;
                case Prefixes.Message:
                    clientID = readInt32();

                    player1 = new GDM.PositionMemory();
                    player2 = new GDM.PositionMemory();

                    player1.x_position = readInt32();
                    player1.y_position = readInt32();
                    player1.x_rotation = readInt32();
                    player1.y_rotation = readInt32();
                    player1.gamemode = readByte();
                    player1.activeIconID = readByte();
                    player1.size = readInt32();
                    player1.gravity = readByte();

                    player2.x_position = readInt32();
                    player2.y_position = readInt32();
                    player2.x_rotation = readInt32();
                    player2.y_rotation = readInt32();
                    player2.gamemode = readByte();
                    player2.activeIconID = readByte();
                    player2.size = readInt32();
                    player2.gravity = readByte();

                    isded = readByte();
                    activeiconid = readByte();
                    col1 = readByte();
                    col2 = readByte();
                    isglow = readByte();
                    IconIDs = readByteLenth(7).ToArray();
                    isvip = readByte();
                    break;
                case Prefixes.PlayerDisconnect:
                    clientID = readInt32();
                    break;
            }
        }
        private float readFloat()
        {
            var result = BitConverter.ToSingle(readByteLenth(4).ToArray(), 0);
            return result;
        }
        private int readInt32()
        {
            var result = BitConverter.ToInt32(readByteLenth(4).ToArray(), 0);
            return result;
        }
        private long readLong()
        {
            var result = BitConverter.ToInt64(readByteLenth(8).ToArray(), 0);
            return result;
        }
        private string readStringToEnd() {
            var buffer = readByteLenth(bytesleft).ToArray();
            return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        }
        private byte readByte()
        {
            return readByteLenth(1).FirstOrDefault();
        }
        private IEnumerable<byte> readByteLenth(int len)
        {
            try
            {
                var segment = new ArraySegment<byte>(buffer, position, len);
                position += len;
                bytesleft -= len;
                return segment;
            }
            catch
            {
                return null;
            }
        }
    }
}
