using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Utilities.JSON_Models
{
    public class Level_Data
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 

        public string name { get; set; }
        public string id { get; set; }
        public string description { get; set; }
        public string author { get; set; }
        public string playerID { get; set; }
        public string accountID { get; set; }
        public string difficulty { get; set; }
        public int downloads { get; set; }
        public int likes { get; set; }
        public bool disliked { get; set; }
        public string length { get; set; }
        public int stars { get; set; }
        public int orbs { get; set; }
        public int diamonds { get; set; }
        public string difficultyFace { get; set; }

    }
}
