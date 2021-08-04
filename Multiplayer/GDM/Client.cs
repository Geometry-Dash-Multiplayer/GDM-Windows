using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
namespace Multiplayer.GDM
{
    public class Client
    {
        public string username;
        public int id;
        public PositionMemory player_1, player_2;

        public byte IsDead = 0x0;
        // public byte Opacity = 0x64; // 100
        public byte IsVIP = 0x0;

        public byte IsRainbow = 0x0;
        public byte IsPastelColor = 0x0;

        public byte colorR = 255;
        public byte colorG = 255;
        public byte colorB = 255;

        [JsonIgnore]
        public string h_username;
        [JsonIgnore]
        public short Lobby = 0;
        [JsonIgnore]
        public byte color_1, color_2, isGlow;
        [JsonIgnore]
        public byte[] IconIDs = new byte[] { 0, 0, 0, 0, 0, 0, 0 };
        [JsonIgnore]
        public static Dictionary<int, string> IconsAndIDs = new Dictionary<int, string>();
        [JsonIgnore]
        public static List<int> IconsAlreadyBeingLoaded = new List<int>();
        [JsonIgnore]
        public static List<int> IconsAlreadyLoaded = new List<int>();
        [JsonIgnore]
        public int levelid;
        [JsonIgnore]
        public bool isIconIDDownloaded = false;
        [JsonIgnore]
        public string IconsDirectory = Globals.Paths.IconsFolder + "/";
        [JsonIgnore]
        public PlayerRepresentor represent;
        [JsonIgnore]
        public Utilities.JSONModels.Player PlayerData;
        [JsonIgnore]
        System.Timers.Timer ProgressTimer = new System.Timers.Timer();
        public Client(int clientID, PositionMemory _p1, PositionMemory _p2, byte Color1, byte Color2, byte isGlow, byte[] iconIDs)
        {
            IconsDirectory = Path.GetFullPath(Globals.Paths.IconsFolder + "/" + clientID.ToString());
            if (!Directory.Exists(IconsDirectory)) Directory.CreateDirectory(IconsDirectory);

            this.id = clientID;
            player_1 = _p1; player_2 = _p2;
            this.IconIDs = iconIDs;
            this.color_1 = Color1;
            this.color_2 = Color2;
            SetUsername("Player " + id.ToString());
        }
        public void Initialize()
        {
            // Debug.WriteLine( " initializing");
            ThreadForInit = new Thread(() =>
            {
                try
                {
                    TLoadPlayerData();
                    InitializeRepresentor();
                    DownloadIcons();
                }
                catch (Exception ex)
                {
                    Globals.Global_Data.HandleException(ex);
                }

                ProgressTimer.Elapsed += ProgressTimer_Elapsed;
                ProgressTimer.Interval = 1000;
                ProgressTimer.Enabled = true;
                ProgressTimer.Start();
            }); ThreadForInit.Start();
        }
        public void SetUsername(string i)
        {

            if (!Globals.Global_Data.HideUsernames)
            { // if hide usernames false
              //ShowUsername();
                username = i;
                if (represent != null)
                    represent.SetUsername(username);
            }
            else
            {
                username = i;
                HideUsername();
            }
        }
        public void HideUsername()
        {
            h_username = username;
            username = "";
            if (represent != null)
                represent.SetUsername("");
        }
        public void ShowUsername()
        {
            username = h_username;
            if (represent != null)
                represent.SetUsername(username);
        }
        private void ProgressTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                var player = Globals.Global_Data.Connection.model.players.Where(x => x.id == this.id);
                if (player.Count() > 0)
                {
                    if (isIconIDDownloaded)
                    {
                        float y = BitConverter.ToSingle(BitConverter.GetBytes(player_1.x_position), 0);

                        float percentage = (y / Globals.Global_Data.LevelLength) * 100;
                        double j = Math.Round(percentage, 2, MidpointRounding.AwayFromZero);

                        if (represent != null)
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                try
                                {
                                    var g = GetCurrentStatic(this.id);
                                    if (IsDead == 0)
                                    {
                                        // put to top if percent high
                                        g.represent.SetStatus("Playing " + j.ToString() + "%");
                                        g.represent.TurnProgToColor();
                                    }
                                    else
                                    {
                                        g.represent.SetStatus("Died " + j.ToString() + "%");
                                        g.represent.TurnProgToColor("#f54242", 100);
                                    }

                                    g.represent.SetProgress(Convert.ToInt32(j));
                                }
                                catch (Exception ex){
                            
                                    Globals.Global_Data.HandleException(ex);
                                }
                            
                            }));
                    }
                    //  player.FirstOrDefault().isIconIDDownloaded = true;
                    // player.FirstOrDefault().represent.DoneIcons(this.username);
                }
                else
                {
                    ProgressTimer.Stop();
                }
            }
            catch (Exception ex)
            {
                Globals.Global_Data.HandleException(ex);
            }
        }

        public void Disconnected()
        {
            ProgressTimer.Stop();
            try
            {
                if (LoadedThread.IsAlive)
                    LoadedThread.Abort();
                if (represent != null)
                    represent.RemovePlayer();
            }
            catch (Exception ex)
            {
                Globals.Global_Data.HandleException(ex);
            }
        }
        [JsonIgnore]
        public Thread LoadedThread, ThreadForInit;
        public void TLoadPlayerData()
        {

            LoadedThread = new Thread(() =>
            {
                try
                {
                    LoadPlayerData();
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        try
                        {
                            if (!Globals.Global_Data.HideUsernames)
                                Globals.Global_Data.Initializer.Announce(Globals.Global_Data.Lang.Joined.Replace("%username%", this.username));

                            Debug.WriteLine(username + " joined");
                            var g = GetCurrentStatic(this.id);
                            if (g != null)
                                g.SetUsername(username);
                        }
                        catch (Exception ex)
                        {
                            Globals.Global_Data.HandleException(ex);
                        }
                    }));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }); LoadedThread.Start();
        }
        public void InitializeRepresentor()
        {

            represent = new PlayerRepresentor(this);
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var border = represent.Container();
                Globals.Global_Data.Initializer.AddPlayer(border);
            }));

        }
        public void LoadPlayerData()
        {
            bool usernameFound = false;
            while (!usernameFound)
            {
                try
                {
                    if (Globals.Global_Data.Main.UserPref.CachedUsernames)
                    {
                        if (Utilities.JSONModels.UsernameCache.PlayerIDAndUsername.ContainsKey(id))
                        {
                            username = Utilities.JSONModels.UsernameCache.PlayerIDAndUsername[id];
                            if (!string.IsNullOrEmpty(username))
                            {
                                usernameFound = true;
                                return;
                            }
                        }
                    }
                    var g = GetCurrentStatic(this.id);
                    if (g != null)
                    {
                        username = Utilities.TCP.GetUsernameFromPlayerID(id);
                        if (Globals.Global_Data.Main.UserPref.CachedUsernames)
                        {
                            if (!Utilities.JSONModels.UsernameCache.PlayerIDAndUsername.ContainsKey(id))
                            Utilities.JSONModels.UsernameCache.PlayerIDAndUsername.Add(id, username);
                        }
                    }
                    usernameFound = true;
                }
                catch (Exception ex)
                {
                    Globals.Global_Data.HandleException(ex);
                }
            }
        }
        public void DownloadIcons()
        {
            if (!IconsAlreadyBeingLoaded.Contains(this.id))
            {
                IconsAlreadyBeingLoaded.Add(this.id);
                new Thread(() =>
                {
                    int u = 0;
                    while (u < 7)
                    {
                        try
                        {
                            // add downloading icons progress bar
                            string icon_type = IconsAndIDs[u];
                            int iconID = (int)IconIDs[u];
                            string apiurl = Utilities.TCP.GetAPIUrl(
                                icon_type,
                                ((int)color_1).ToString(),
                                ((int)color_2).ToString(),
                                iconID.ToString(),
                                id.ToString(),
                                ((int)isGlow).ToString(),
                                IconIDs[0].ToString()
                                );

                            string path = null;
                            while (path == null)
                            {
                                if (Globals.Global_Data.Main.UserPref.CachedIcons)
                                {
                                    path = CheckIfIconExists(IconsDirectory, u.ToString());
                                }
                                if (path == null)
                                {
                                    path = Utilities.TCP.DownloadImageToDir(apiurl, IconsDirectory, u.ToString());
                                }
                            }

                            var _g = GetCurrentStatic(this.id);

                            if (_g != null)
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                try
                                {

                                    var prog = (u / 6f) * 100f;
                                    if (_g.represent != null)
                                    {
                                        _g.represent.SetProgress((int)prog);
                                        if (u == 0)
                                            _g.represent.SetVIPIcon(path);
                                    }

                                }
                                catch (Exception ex)
                                {
                                    Globals.Global_Data.HandleException(ex);
                                }
                            }));
                        }
                        catch (Exception ex)
                        {
                            Globals.Global_Data.HandleException(ex);
                        }
                        u++;
                    }

                    try
                    {
                        if (IsVIP == 0x1)
                        {
                            string j = Utilities.TCP.ReadURL("http://95.111.251.138/gdm/isRainbow.php?id=" + id.ToString()).Result;
                            var deserializedProduct = JsonConvert.DeserializeObject<Utilities.JSONModels.ClientData>(j);
                            IsRainbow = (byte)deserializedProduct.israinbow;
                            IsPastelColor = (byte)deserializedProduct.israinbowpastel;

                            Color color = (Color)ColorConverter.ConvertFromString(deserializedProduct.hexcolor);
                            colorR = color.R;
                            colorG = color.G;
                            colorB = color.B;
                        }
                    }
                    catch (Exception ex)
                    {
                        Utilities.Utils.HandleException(ex);
                    }

                    // done
                    IconsAlreadyLoaded.Add(this.id);
                    // needed because the client may have been disposedGetCurrentStatic
                    var g = GetCurrentStatic(this.id);
                    if (g != null)
                    {
                        g.isIconIDDownloaded = true;
                        if (g.represent != null)
                            g.represent.DoneIcons(this.username);

                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            try
                            {
                                var j = CheckIfIconExists(IconsDirectory, "0");
                                if (j != null)
                                    g.represent.SetVIPIcon(j);
                            }
                            catch (Exception ex)
                            {
                                Globals.Global_Data.HandleException(ex);
                            }
                        }));
                    }
                    // isIconIDDownloaded = true;
                    // represent.DoneIcons(this.username);
                }).Start();
            }
            try
            {
                if (IconsAlreadyLoaded.Contains(this.id))
                {

                    try
                    {
                        if (IsVIP == 0x1)
                        {
                            string lj = Utilities.TCP.ReadURL("http://95.111.251.138/gdm/isRainbow.php?id=" + id.ToString()).Result;
                            var deserializedProduct = JsonConvert.DeserializeObject<Utilities.JSONModels.ClientData>(lj);
                            IsRainbow = (byte)deserializedProduct.israinbow;
                            IsPastelColor = (byte)deserializedProduct.israinbowpastel;

                            Color color = (Color)ColorConverter.ConvertFromString(deserializedProduct.hexcolor);
                            colorR = color.R;
                            colorG = color.G;
                            colorB = color.B;
                        }
                    }
                    catch (Exception ex)
                    {
                        Utilities.Utils.HandleException(ex);
                    }

                    string j = CheckIfIconExists(IconsDirectory, "0");
                    if (File.Exists(j))
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            //try
                            //{
                            //    represent.SetVIPIcon(j);
                            //}
                            //catch (Exception ex)
                            //{
                            //    Utilities.Utils.HandleException(ex);
                            //}
                        }));

                    if (represent != null)
                        this.represent.DoneIcons(this.username);
                    this.isIconIDDownloaded = true;
                }
            }
            catch (Exception ex)
            {
                Utilities.Utils.HandleException(ex);
            }
        }
        public Client GetCurrentStatic(int id)
        {
            var _j = Globals.Global_Data.Connection.model.players.Where(x => x.id == id);
            if (_j.Count() > 0)
            {
                return _j.FirstOrDefault();
            }
            return null;
        }
        public string CheckIfIconExists(string u, string index)
        {
            if (File.Exists(u + "/" + index + ".png"))
                return u + "/" + index + ".png";
            if (File.Exists(u + "/" + index + "/0.png"))
                return u + "/" + index + "/image.gif";
            return null;
        }
        public string GetExtension(string fil)
        {
            return System.IO.Path.GetExtension(fil);
        }
        public void Set(PositionMemory _p1, PositionMemory _p2, byte isDead, byte Color1, byte Color2, byte[] iconIDs)
        {
            player_1 = _p1; player_2 = _p2;
            this.color_1 = Color1;
            this.color_2 = Color2;
            this.IconIDs = iconIDs;
            this.IsDead = isDead;
        }
    }

    public class JSONModel
    {
        public List<Client> players = new List<Client>();
    }
}
