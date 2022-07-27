using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Networking
{
    public class PositionPacket
    {
        public byte PlayerCount { get; set; }
        public int XPosition { get; set; }
        public int YPosition { get; set; }
        public int XRotation { get; set; }
        public int YRotation { get; set; }
        public int Size { get; set; }
    }
}
