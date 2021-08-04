using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Utilities.JSON_Models
{
    public class lc
    {
        public Dictionary<int, level> levels = new Dictionary<int, level>();
    }
    public class level
    {
        public int Players;
    }
}
