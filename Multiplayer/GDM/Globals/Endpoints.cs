using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.GDM.Globals
{
    /// <summary>
    /// Contains endpoints to GDM api.
    /// </summary>
    public static class Endpoints
    {
        public const string ProxiedGDBrowser = "http://gdbrowser.com";

        public const string APIEndpoint = "http://95.111.251.138/gdm/";

        public const string GetInfo = APIEndpoint + "getInfo.php";
    }
}
