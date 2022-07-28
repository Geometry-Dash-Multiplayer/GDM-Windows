using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.GDM
{

    public class PositionMemory
    {
        public int x_position, y_position, x_rotation, y_rotation, size;
        public byte gamemode, activeIconID, gravity;
        public PositionMemory(int x, int y, int xr, int yr, int size, byte form, byte iconID, byte gravity)
        {
            this.x_position = x; this.y_position = y; this.x_rotation = xr; this.y_rotation = yr; this.size = size; this.gamemode = form; this.activeIconID = iconID; this.gravity = gravity;
        }
        public PositionMemory()
        {

        }
    }
}
