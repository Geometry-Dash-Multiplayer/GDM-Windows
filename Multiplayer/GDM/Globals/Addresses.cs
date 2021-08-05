using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.GDM.Globals
{
    public static class Addresses
    {
        public static int BaseAddr = 0x3222D0; 

        public static int PlayerLayer = 0x164;

        public static int Player1Pointer = 0x224;

        public static int Player2Pointer = 0x228;

        public static int[] Player1Layer = new int[] { PlayerLayer, Player1Pointer };

        public static int[] Player2Layer = new int[] { PlayerLayer, Player2Pointer };

        public static int XPositionOffset = 0x67C;
        public static int YPositionOffset = 0x680;
        public static int XRotationOffset = 0x020;
        public static int YRotationOffset = 0x024;
        public static int PlayerSizeOffset = 0x644;
        public static int PlayerFormOffset = 0x638;
        public static int IsGlow = 0x27C;
        public static int AddrPlayerID = 0x1BC;
        public static int IconFormOffset = 0x638;
        public static int GravityOffset = 0x63E;
        public static int[] IsDead = new int[] { 0x164, 0x39C };
        public static int[] LevelLengthOffets1 = new int[] { 0x164, 0x3B4 };
        public static float LevelLength = 0;
        public static int[] AttemptOffsets1 = new int[] { 0x164, 0x39C };
        public static int[] AttemptOffsets2 = new int[] { 0x164, 0x39C };
        public static int LevelIDOffset = 0x2A0;
        public static int AttemptAddr = 0;
    }
}
