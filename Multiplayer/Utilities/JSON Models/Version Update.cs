using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Utilities.JSON_Models
{
    public class Version_Update
    {
        [JsonProperty("version")]
        public double Version { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("desc")]
        public string PatchNotes { get; set; }

        [JsonProperty("caching")]
        public bool CachingEnabled { get; set; }
    }
}
