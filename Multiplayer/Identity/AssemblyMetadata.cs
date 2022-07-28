using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Identity
{
    public class AssemblyMetadata : IData
    {
        public double Version => 5.5;

        public string Name => "Geometry Dash Multiplayer";
    }
}
