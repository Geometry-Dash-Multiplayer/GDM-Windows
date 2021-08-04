using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Utilities.JSON_Models
{
    public class Player
    {// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 

        public string username { get; set; }
        public string playerID { get; set; }
        public string accountID { get; set; }
        public int rank { get; set; }
        public int stars { get; set; }
        public int diamonds { get; set; }
        public int coins { get; set; }
        public int userCoins { get; set; }
        public int demons { get; set; }
        public int cp { get; set; }
        public bool friendRequests { get; set; }
        public string messages { get; set; }
        public string commentHistory { get; set; }
        public int moderator { get; set; }
        public string youtube { get; set; }
        public string twitter { get; set; }
        public string twitch { get; set; }
        public int icon { get; set; }
        public int ship { get; set; }
        public int ball { get; set; }
        public int ufo { get; set; }
        public int wave { get; set; }
        public int robot { get; set; }
        public int spider { get; set; }
        public int col1 { get; set; }
        public int col2 { get; set; }
        public int deathEffect { get; set; }
        public bool glow { get; set; }



    }
}
