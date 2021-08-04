using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Utilities.JSON_Models
{
    public class VersionUpdate
    {
        public double version { get; set; }
        public string link { get; set; }
        public string desc { get; set; }
        public string caching { get; set; }
    }
}
