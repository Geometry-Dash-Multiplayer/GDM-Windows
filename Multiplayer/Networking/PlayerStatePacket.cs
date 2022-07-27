using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Networking
{
    public class PlayerStatePacket
    {
        public byte GameMode { get; set; }
        public bool IsVip { get; set; }
        public byte ActiveIconId { get; set; }
        public byte Gravity { get; set; }
        public byte IsDead { get; set; }
    }
}
