using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Utilities.Encryption
{

    public class Robtop_Parser
    {
        public string Response;
        public Dictionary<int, string> KeysAndCrates = new Dictionary<int, string>();
        public Robtop_Parser(string response)
        {
            this.Response = response;
        }
        public bool Parse()
        {
            if (Response == "-1") return false;
            try
            {
                var splitNewgrounds = Response.Split('#')[0];
                var splitLevelInfo = splitNewgrounds.Split(':');

                for (int q = 0; q < splitLevelInfo.Length; q++)
                {

                    if (q % 2 == 0)
                    { // if even then its a key
                        KeysAndCrates.Add(int.Parse(splitLevelInfo[q]), null);
                    }
                    else
                    { // if odd then its a value
                        KeysAndCrates[KeysAndCrates.Keys.Last()] = splitLevelInfo[q];
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public Dictionary<int, string> Difficulties = new Dictionary<int, string>() {
            { 0, "unrated"} , { 10,"easy"} , { 20,"normal"}, { 30,"hard"} , { 40,"harder"} , { 50,"insane"}
        };
        public Dictionary<int, string> Demons = new Dictionary<int, string>() {
            { 3, "easy"} , { 4,"medium"} , { 5,"insane"}, { 6 , "extreme" }
        };
        public string GetDifficultyFace()
        {
            try
            {
                string dif = "unrated";
                bool isEpic = int.Parse(KeysAndCrates[42]) > 0;
                bool isFeatured = int.Parse(KeysAndCrates[19]) > 0;
                Difficulties.TryGetValue(int.Parse(KeysAndCrates[9]), out dif);
                int demontype = 0;
                int isAuto = 0;
                try
                {
                    if (!string.IsNullOrEmpty(KeysAndCrates[17]))
                        demontype = int.Parse(KeysAndCrates[17].Trim());
                    if (!string.IsNullOrEmpty(KeysAndCrates[25]))
                        isAuto = int.Parse(KeysAndCrates[25].Trim());
                }
                catch (Exception ex)
                {
                }
                if (demontype > 0) dif = (Demons.ContainsKey(int.Parse(KeysAndCrates[43])) ? Demons[int.Parse(KeysAndCrates[43])] : "hard") + " demon";
                if (isAuto > 0) dif = "auto";
                string face = (demontype != 1 ? dif : "demon-" + dif.Split(' ')[0]) + (isEpic ? "-epic" : (isFeatured ? "-featured" : ""));
                return face;
            }
            catch { return null; }
        }
    }
}
