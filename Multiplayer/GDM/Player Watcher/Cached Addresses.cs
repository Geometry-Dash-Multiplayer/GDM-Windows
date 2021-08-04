using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.GDM.Player_Watcher
{
    // my balls are throbbing
    public static class Cached_Addresses
    {
        public static int ModuleBaseAddr = 0;

        public static int X1Position_StaticAddress = 0;
        public static int Y1Position_StaticAddress = 0;
        public static int X1Rotation_StaticAddress = 0;
        public static int Y1Rotation_StaticAddress = 0;
        public static int X2Position_StaticAddress = 0;
        public static int Y2Position_StaticAddress = 0;
        public static int X2Rotation_StaticAddress = 0;
        public static int Y2Rotation_StaticAddress = 0;

        public static int Size1_StaticAddress = 0;
        public static int Icon1FormAddr = 0;
        public static int Size2_StaticAddress = 0;
        public static int Icon2FormAddr = 0;

        public static IntPtr LevelObjectsListAddr;
        public static IntPtr LevelAddr;
        public static IntPtr CubeIconAddr1;
        public static IntPtr ShipIconAddr1;
        public static IntPtr BallIconAddr1;
        public static IntPtr UFOIconAddr1;
        public static IntPtr WaveIconAddr1;
        public static IntPtr RobotIconAddr1;
        public static IntPtr SpiderIconAddr1;
        public static IntPtr LevelLengthAddr;
        public static IntPtr IsDeadAddr;

        public static IntPtr Color1Addr;
        public static IntPtr Color2Addr;
        public static IntPtr Player1GravityAddress;
        public static IntPtr Player2GravityAddress;
    }
}
