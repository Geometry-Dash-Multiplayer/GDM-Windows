using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace Multiplayer.GDM
{
    public class Initialize
    {
        public Server Connection;
        public MainWindow Main;

        public Utilities.JSONModels.LevelID CurrentLevelData;

        public Initialize(MainWindow _main)
        {
            this.Main = _main;
            // check from old versions
            LoadUserPrefs();
            CheckOldData();
            InitializeClient();

            // load language
            GDM.LoadLanguage.Load();
            WebRequest.DefaultWebProxy = null;
            Globals.Global_Data.Initializer = this;
        }
        public void CheckOldData()
        {
            try
            {
                string OldUserDataFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Alizer/gdm.dat";
                string ParentDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Alizer";
                if (!Directory.Exists(ParentDir))
                    Directory.CreateDirectory(ParentDir);
                // MessageBox.Show(Globals.Paths.UserDataFile);
                if (File.Exists(OldUserDataFile))
                {
                    if (!File.Exists(Globals.Paths.UserDataFile))
                    {
                        File.Create(Globals.Paths.UserDataFile).Close();
                    }

                    File.WriteAllText(Globals.Paths.UserDataFile,
                        File.ReadAllText(OldUserDataFile)
                        );
                    File.Delete(OldUserDataFile);
                }
            }
            catch (Exception ex)
            {

                Utilities.Utils.HandleException(ex);

            }
        }
        public void LoadCaches()
        {
            // because robtop servers are hosted on a laptop from 2003
            if (!File.Exists(Globals.Paths.LevelsCache)) File.Create(Globals.Paths.LevelsCache).Close();
            Utilities.JSONModels.LevelCache.LevelIDandData = JsonConvert.DeserializeObject<Dictionary<int,string>>(
                File.ReadAllText(Globals.Paths.LevelsCache)
                   );

            if (Utilities.JSONModels.LevelCache.LevelIDandData == null) Utilities.JSONModels.LevelCache.LevelIDandData = new Dictionary<int, string>();


            if (!File.Exists(Globals.Paths.UsernamesCache)) File.Create(Globals.Paths.UsernamesCache).Close();
            Utilities.JSONModels.UsernameCache.PlayerIDAndUsername = JsonConvert.DeserializeObject<Dictionary<int, string>>(
                File.ReadAllText(Globals.Paths.UsernamesCache)
                   );
            if (Utilities.JSONModels.UsernameCache.PlayerIDAndUsername == null) Utilities.JSONModels.UsernameCache.PlayerIDAndUsername = new Dictionary<int, string>();
        }
        public void SaveCaches()
        {
            string output = JsonConvert.SerializeObject(Utilities.JSONModels.LevelCache.LevelIDandData);
            System.IO.File.WriteAllText(Globals.Paths.LevelsCache, output);
            output = JsonConvert.SerializeObject(Utilities.JSONModels.UsernameCache.PlayerIDAndUsername);
            System.IO.File.WriteAllText(Globals.Paths.UsernamesCache, output);
        }

        public void ClearCaches()
        {
            try
            {
                Globals.Global_Data.ReceiveNewClients = false;
                if (Globals.Global_Data.Connection != null)
                {
                    foreach (var g in Globals.Global_Data.Connection.model.players) {
                        try {
                            g.Disconnected();
                        }
                        catch (Exception ex)
                        {
                            Utilities.Utils.HandleException(ex);
                        }
                    }
                }

                if (Utilities.JSONModels.LevelCache.LevelIDandData != null)
                    Utilities.JSONModels.LevelCache.LevelIDandData.Clear();
                if (Utilities.JSONModels.UsernameCache.PlayerIDAndUsername != null)
                    Utilities.JSONModels.UsernameCache.PlayerIDAndUsername.Clear();

                Main.pstacks.Children.Clear();

                ImageBehavior.SetAnimatedSource(Main.image6, null);
                Main.image6.Source = null;

                try
                {
                    if (Directory.Exists(Globals.Paths.IconsFolder))
                        Utilities.Utils.DeleteDirectory(Globals.Paths.IconsFolder);
                }
                catch (Exception ex)
                {
                    Utilities.Utils.HandleException(ex);
                }
                Client.IconsAlreadyBeingLoaded.Clear();
                Client.IconsAlreadyLoaded.Clear();

                if (Globals.Global_Data.Connection != null) Globals.Global_Data.Connection.model.players.Clear();

                Globals.Global_Data.JSONCommunication.Clear();

                if (File.Exists(Globals.Paths.UsernamesCache)) File.WriteAllText(Globals.Paths.UsernamesCache, "");
                if (File.Exists(Globals.Paths.LevelsCache)) File.WriteAllText(Globals.Paths.LevelsCache, "");


                pfpset = false;
                Announce("Cache cleared! It make take a while to load new content now.");

                new Thread(() => { DownloadSelfIcons(); }).Start();

            }
            catch (Exception ex)
            {
                Utilities.Utils.HandleException(ex);
            }
            Globals.Global_Data.ReceiveNewClients = true;
        }
        public void InitializeClient()
        {
            InitializeDirectories();
            ShowFireWall();
            CheckVersion();
            new Thread(() =>
            {
                try
                {
                    LoadCaches();
                    LoadPlayerIDFromSaveFile();
                    if (Globals.Global_Data.PlayerIDLoaded)
                    DownloadSelfIcons();
                    Globals.Global_Data.Initializer.SetPlayerName(Utilities.TCP.GetUsernameFromPlayerID(Globals.Global_Data.PlayerID));
                    Globals.Global_Data.Initializer.SetAccountID(Globals.Global_Data.PlayerID);

                    // check server statuses

                }
                catch (Exception ex)
                {
                    Utilities.Utils.HandleException(ex);
                }
            }).Start();
        }
        public void LoadPlayerIDFromSaveFile()
        {
            int q = Utilities.SaveFile.SaveFileDescryptor.GetPlayerID();
            if (q > 0)
            {
                Globals.Global_Data.PlayerID = q;
                Globals.Global_Data.PlayerIDLoaded = true;
                // try to add the user to server db if he doesnt exist
                var temp = Utilities.TCP.ReadURL("http://95.111.251.138/gdm/getInfo.php?id=" + q.ToString()).Result;
                PlayerWatcher.InitClient();
                Debug.WriteLine("User check: " + temp);

            }
            else
                Globals.Global_Data.PlayerIDLoaded = false;
        }
        public void ShowFireWall()
        {
            new Thread(() =>
            {
                try
                {
                    IPAddress ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
                    IPEndPoint ipLocalEndPoint = new IPEndPoint(ipAddress, 32591);
                    TcpListener t = new TcpListener(ipLocalEndPoint);
                    t.Start();
                    t.Stop();
                }
                catch (Exception ex)
                {
                    Utilities.Utils.HandleException(ex);
                }
            }).Start();
        }
        public void CheckVersion()
        {
            new Thread(() =>
            {
                try
                {
                    // check if show self rainbow
                    Utilities.JSONModels.VersionUpdate deserializedProduct = JsonConvert.DeserializeObject<Utilities.JSONModels.VersionUpdate>(
                        Utilities.TCP.ReadURL(Globals.Global_Data.VersionLink).Result
                        );

                    if (deserializedProduct != null)
                    {
                        Globals.Global_Data.Main.UserPref.CachedLevels = deserializedProduct.caching != ":)";
                        Globals.Global_Data.Main.UserPref.CachedUsernames = deserializedProduct.caching != ":)";

                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            Main.arelevelscached.IsChecked = Main.UserPref.CachedLevels;
                            Main.areusernamescached.IsChecked = Main.UserPref.CachedUsernames;
                            if (deserializedProduct.version > Globals.Global_Data.Version)
                            {
                                // Main.arelevelscached.IsChecked = Main.UserPref.CachedLevels;
                                // Main.areusernamescached.IsChecked = Main.UserPref.CachedUsernames;
                                Main.update.Visibility = Visibility.Visible;
                                Main.updatetext.Text = deserializedProduct.desc;
                                Main.update.IsOpen = true;
                                Main.updateButt.Click += (k, t) =>
                                {
                                    Main.update.IsOpen = false;
                                    Process.Start("Updater.exe");
                                    Environment.Exit(0);
                                };
                            }
                        }));
                        // Globals.Paths.Main.UserPref.CachedLevels = deserializedProduct.caching != ":)";

                    }
                }
                catch (Exception ex)
                {
                    Utilities.Utils.HandleException(ex);
                }
            }).Start();
        }
        public void Relogin()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Main.login.IsOpen = true;
            }));
        }
        public void LoadUserPrefs()
        {
            try
            {
                var dirname = System.IO.Path.GetDirectoryName(Globals.Paths.UserDataFile);
                System.IO.Directory.CreateDirectory(dirname);
                if (!System.IO.File.Exists(Globals.Paths.UserDataFile))
                {
                    System.IO.File.Create(Globals.Paths.UserDataFile).Close();
                    SaveUserPref();
                }
                Main.UserPref = JsonConvert.DeserializeObject<UserPref>(System.IO.File.ReadAllText(Globals.Paths.UserDataFile));
                // generate key
                if (Main.UserPref.ClientKey_fix == null || Main.UserPref.ClientKey_fix.Length != Globals.Global_Data.KeySize)
                {
                    Main.UserPref.ClientKey_fix = Utilities.Randomness.RandomBytes(Globals.Global_Data.KeySize);
                    SaveUserPref();
                }
                Globals.Global_Data.VipKey = BitConverter.ToInt32(Main.UserPref.ClientKey_fix, 0);
                if (Main.UserPref.IsVIP)
                    Main.border5.Visibility = Visibility.Collapsed;
                Main.areiconscached.IsChecked = Main.UserPref.CachedIcons;
                Main.arelevelscached.IsChecked = Main.UserPref.CachedLevels;
                Main.areusernamescached.IsChecked = Main.UserPref.CachedUsernames;

                if (!Main.UserPref.CachedLevels) if (File.Exists(Globals.Paths.LevelsCache)) File.Delete(Globals.Paths.LevelsCache);
                    else if (!File.Exists(Globals.Paths.LevelsCache)) File.Create(Globals.Paths.LevelsCache).Close();
                if (!Main.UserPref.CachedUsernames) if (File.Exists(Globals.Paths.UsernamesCache)) File.Delete(Globals.Paths.UsernamesCache);
                    else if (!File.Exists(Globals.Paths.UsernamesCache)) File.Create(Globals.Paths.UsernamesCache).Close();

                if (string.IsNullOrEmpty(Main.UserPref.WindowName)) Main.UserPref.WindowName = "Geometry Dash";
                if (string.IsNullOrEmpty(Main.UserPref.MainModule)) Main.UserPref.WindowName = "GeometryDash.exe";

                Globals.Global_Data.HideUsernames = !Main.UserPref.ShowSelfUsername;
                // Main.UserPref.RenderCustomIcons = false;
                if (Main.UserPref.Version != Globals.Global_Data.Version) {
                    Directory.Delete(Globals.Paths.IconsFolder,true);
                }
                Main.UserPref.Version = Globals.Global_Data.Version;

            }
            catch (Exception ex)
            {     
            
                Utilities.Utils.HandleException(ex);
            
            File.Delete(Globals.Paths.UserDataFile);
            }
        }
        public void ResetPrefs()
        {
            try
            {
                // Utilities.JSONModels.LevelCache.LevelIDandData.Clear();
                // SaveCaches();
                // Main.UserPref = new UserPref();
                // Main.UserPref.PlayerID = Globals.Paths.PlayerID;
                // SaveUserPref();
                if (File.Exists(Globals.Paths.LevelsCache)) File.Delete(Globals.Paths.LevelsCache);
                if (File.Exists(Globals.Paths.UsernamesCache)) File.Delete(Globals.Paths.UsernamesCache);
                if (Directory.Exists(Globals.Paths.IconsFolder)) Directory.Delete(Globals.Paths.IconsFolder, true);
            }
            catch (Exception ex) {

                Utilities.Utils.HandleException(ex);
            }
            Announce("Settings reset!");
            Process.Start(Process.GetCurrentProcess().MainModule.FileName);
            Environment.Exit(0);
        }
        public void SaveUserPref()
        {
            // Announce("stats saved");
            System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
            // MessageBox.Show(t.ToString());
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Main.UserPref.CachedLevels = Main.arelevelscached.IsChecked;
                Main.UserPref.CachedIcons = Main.areiconscached.IsChecked;
                Main.UserPref.CachedUsernames = Main.areusernamescached.IsChecked;
            }));

            string output = JsonConvert.SerializeObject(Main.UserPref);
            System.IO.File.WriteAllText(Globals.Paths.UserDataFile, output);

            if (!File.Exists(Globals.Paths.GDMTempDataFile)) File.Create(Globals.Paths.GDMTempDataFile).Close();
            File.WriteAllText(Globals.Paths.GDMTempDataFile, output);
        }
        public void ConnectEU()
        {
            ServerConnected(0);
        }
        public void ConnectEast()
        {
            ServerConnected(2);
        }
        public void ConnectAS1()
        {
            ServerConnected(1);
        }
        int ServerIndex = 0;
        public void ServerConnected(int index)
        {
            try
            {
                ServerIndex = index;
                Connection = new Server(Globals.Global_Data.ServerIPs[index] /* 
                                                                          * Europe Connection = 0 */, this);
                Globals.Global_Data.Connection = Connection;
                Globals.Global_Data.ActiveServer = Globals.Global_Data.ServerIPs[index];
                Main.SG_Key.Text = "Secret Key: " + Utilities.Converter.BytesToString(Main.UserPref.ClientKey_fix);
                Main.StartAnimation("ServerSelected");

                Main.border5.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Globals.Global_Data.HandleException(ex);
            }
            // Main.StartAnimation("SingaporeActive");
        }

        public void SetPlayerCount(int players)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Main.SG_Key.Text = "Online : ";
                    // Main.sg_badge.Badge = players.ToString();
                }));
            });
        }
        public void ClearLevels()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Main.levels.Children.Clear();
            }));
        }
        public async void LoadLevelID(int levelid)
        {
            Debug.WriteLine("New level id: " + levelid);
            int Tries = 3;
            while (Tries >= 0)
            {
                try
                {
                    string output = Utilities.TCP.GetLevelDataResponse(levelid.ToString());
                    if (output != "-1")
                    {

                        Utilities.RubParser i = new Utilities.RubParser(output);
                        if (i.Parse())
                        {
                            var CurrentLevelData = new Utilities.JSONModels.LevelID();
                            CurrentLevelData.name = i.KeysAndCrates[2];
                            CurrentLevelData.difficultyFace = i.GetDifficultyFace();

                            if (CurrentLevelData != null)
                            {
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    try
                                    {
                                        Main.leveln.Text = CurrentLevelData.name;
                                        Main.levelid.Text = "";
                                        Main.author.Text = GetRoom(Globals.Global_Data.Room);
                                    }
                                    catch (Exception ex)
                                    {
                                        Globals.Global_Data.HandleException(ex);
                                    }
                                }));
                                // CurrentLevelData.featured == true? difficulty += "-featured" :difficulty = difficulty;
                                SetLevelDiff(CurrentLevelData.difficultyFace);
                                if (Globals.Global_Data.Connection != null)
                                    if (Globals.Global_Data.Connection.isHelloAcked)
                                    {
                                        Main.StartAnimation("ShowLevelsAndStats");
                                    }
                            }
                        }
                    }
                    Tries = -1;
                }
                catch (Exception ex)
                {
                    Tries--;
                    Globals.Global_Data.HandleException(ex);
                }
            }
        }
        bool isAttemptsReset = false;
        public void SetPlayerName(string name)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Main.playerW.Text = Globals.Global_Data.Lang.Welcome.Replace("%username%", name);//"Welcome back, " + name + "!";
                    Globals.Global_Data.Username = name;
                }));
            });
        }
        public void AddPlayer(Border border)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (!Main.pstacks.Children.Contains(border))
                        Main.pstacks.Children.Add(border);
                }));
            });
        }
        public async void SetLevelDiff(string diff)
        {
            Debug.WriteLine("Set diff: " + diff);
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    try
                    {
                        BitmapImage image = new BitmapImage(new Uri("UI/Images/Difficulties/" + diff + ".png", UriKind.Relative));

                        if (image != null)
                            Main.leveldif.Source = image;
                    }
                    catch (Exception ex)
                    {
                        Globals.Global_Data.HandleException(ex);
                    }
                }));
            }
            catch { }

        }
        public void WaitForGeometryDash()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Main.waitinggd.IsOpen = true;
            }));


            while (!Globals.Global_Data.IsInjected) Task.Delay(500);


            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Main.waitinggd.IsOpen = false;
            }));
        }
        public void Announce(string text)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    Main.announcer.Text = text;
                    Main.StartAnimation("Announce");
                }
                catch (Exception ex)
                {
                    Globals.Global_Data.HandleException(ex);
                }
            }));
        }
        public void ClearPlayers()
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (Main.pstacks.Children.Count > 0)
                        Main.pstacks.Children.Clear();
                }));
            });
        }
        public bool pfpset = false;
        public void DisableCustomIcons() {
            UserPref j = new UserPref {
                RenderCustomIcons = false
            };
            if (!File.Exists(Globals.Paths.GDMTempDataFile)) File.Create(Globals.Paths.GDMTempDataFile).Close();

            string output = JsonConvert.SerializeObject(j);
            File.WriteAllText(Globals.Paths.GDMTempDataFile, output);
            // File.WriteAllText(Globals.Paths.GDMTempDataFile, output);
        }
        public void EnableCustomIcons() {
            if (!File.Exists(Globals.Paths.GDMTempDataFile)) File.Create(Globals.Paths.GDMTempDataFile).Close();

            string output = JsonConvert.SerializeObject(Main.UserPref);
            File.WriteAllText(Globals.Paths.GDMTempDataFile, output);
        }
        public void ClearSelfIcons() {
            try
            {
                string IconsDirectory = Path.GetFullPath(Globals.Paths.IconsFolder + "/0");
                if (Directory.Exists(IconsDirectory)) Directory.Delete(IconsDirectory, true);

                pfpset = false; iconsDownloaded = false; DownloadSelfIcons();
            }
            catch (Exception ex)
            {
                Globals.Global_Data.HandleException(ex);
            }
        }
        public bool iconsDownloaded = false;
        public void DownloadSelfIcons()
        {
            try
            {
                string IconsDirectory = Path.GetFullPath(Globals.Paths.IconsFolder + "/0");
                if (!pfpset)
                {
                    Announce(Globals.Global_Data.Lang.DownloadingIcons);
                    if (!Main.UserPref.IsVIP)
                    {
                        Main.UserPref.RenderCustomIcons = false;
                        SaveUserPref();
                    }
                    if (Main.UserPref.RenderCustomIcons)
                        DisableCustomIcons();
                    ShowMainProgressBar();
                    SetMainProgressBarValue(10);
                    pfpset = true;
                    try
                    {
                        if (!Main.UserPref.CachedIcons)
                        {
                            if (!Directory.Exists(Globals.Paths.SelfIconsFolder)) Directory.CreateDirectory(Globals.Paths.SelfIconsFolder);
                            Utilities.Utils.DeleteFilesOfExtension(Globals.Paths.SelfIconsFolder, "png");
                            Utilities.Utils.DeleteFilesOfExtension(Globals.Paths.SelfIconsFolder, "gif");
                        }
                    }
                    catch (Exception ex)
                    {
                        Globals.Global_Data.HandleException(ex);
                    }
                    for (int i = 0; i < 7; i++)
                    {
                        string icon_type = Client.IconsAndIDs[i];
                        int iconID = PlayerWatcher.MyIcons[i];
                        string apiurl = Utilities.TCP.GetAPIUrl(
                            icon_type,
                            ((int)PlayerWatcher.Col1).ToString(),
                            ((int)PlayerWatcher.Col2).ToString(),
                            iconID.ToString(),
                            Globals.Global_Data.PlayerID.ToString(),
                            ((int)PlayerWatcher.IsGlow).ToString(),
                            PlayerWatcher.MyIcons[0].ToString()
                            );
                        string path = null;
                        if (Main.UserPref.CachedIcons)
                        {
                            path = CheckIfIconExists(IconsDirectory, i.ToString());
                        }
                        while (path is null)
                        {
                            path = Utilities.TCP.DownloadImageToDir(apiurl, Globals.Paths.SelfIconsFolder, i.ToString());
                            // Debug.WriteLine("Downloaded at " + path);
                        }
                        SetMainProgressBarValue(((i + 1) / 5d) * 100);

                    }
                    SaveUserPref();
                    HideMainProgressBar();
                    Announce(Globals.Global_Data.Lang.IconsLoaded);
                    SetMyPFP();
                    iconsDownloaded = true;
                }
            }
            catch (Exception ex)
            {
                Utilities.Utils.HandleException(ex);
            }
        }
        public string CheckIfIconExists(string u, string index)
        {
            if (File.Exists(u + "/" + index + ".png"))
                return u + "/" + index + ".png";
            if (File.Exists(u + "/" + index + "/0.png"))
                return u + "/" + index + "/image.gif";
            return null;
        }
        public void ShowMainProgressBar() {
            Main.StartAnimation("ShowMainProg");
        }
        public void SetMainProgressBarValue(double value)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    var anim = new DoubleAnimation
                    {
                        To = value,
                        EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseInOut },
                        Duration = TimeSpan.FromMilliseconds(500)
                    };
                    anim.Completed += (s, e) =>
                    {
                        if (Main.mainprog != null)
                            Main.mainprog.Value = value;
                    };
                    if (Main.mainprog != null)
                        Main.mainprog.BeginAnimation(ProgressBar.ValueProperty, anim);
                }
                catch (Exception ex)
                {
                    Utilities.Utils.HandleException(ex);
                }
            }));

        }
        public void HideMainProgressBar()
        {
            Main.StartAnimation("ShowMainProgR");
        }
        public void SetMyPFP()
        {
            // string y = Utilities.TCP.GetAPIUrl("cube","","","",Globals.Paths.PlayerID.ToString(),"");
            // var g = await Utilities.TCP.GetNewImageAsync(new Uri(y,UriKind.Absolute));
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    string CubeIconPath = Globals.Paths.SelfIconsFolder + "/0.png";

                    if (!File.Exists(CubeIconPath)) CubeIconPath = Globals.Paths.SelfIconsFolder + "/0/image.gif";

                    var bitmap = new BitmapImage();
                    var stream = File.OpenRead(CubeIconPath);

                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    if (bitmap != null)
                        ImageBehavior.SetAnimatedSource(Main.image6, bitmap);

                    stream.Close();
                    stream.Dispose();


                    Main.StartAnimation("ShowPFP");
                }
                catch (Exception ex)
                {
                    Utilities.Utils.HandleException(ex);
                }
            }));

        }
        public void SetRoom(short room)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Main.author.Text = Main.Master.GetRoom(Globals.Global_Data.Room);
                Main.elapsed.Text = Main.Master.GetRoom(Globals.Global_Data.Room);
            }));
        }
        public void SetPing(int milliseconds)
        {
            if (milliseconds > 0)
            {
                Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        try
                        {
                            Main.accountID.Text = Globals.Global_Data.Lang.ping.Replace("%ping%", milliseconds.ToString()); // "Ping : " + milliseconds.ToString() + "ms";
                            if (milliseconds > 200)
                                Main.accountID.Foreground = Main.redc.Background;
                            else 
                                Main.accountID.Foreground = Main.tutrqiosecolor.Background;
                        }
                        catch (Exception ex)
                        {
                            Globals.Global_Data.HandleException(ex);
                        }
                    }));
                });
            }
        }
        public void SetAccountID(int id)
        {
            SetAccountID(id.ToString());
        }
        public void SetAccountID(string id)
        {
            new Thread(() =>
            {

                try
                {
                    // Globals.Paths.Initializer.Announce("Downloading your icons.");
                    // Debug.WriteLine("Downloading self icons.");
                    // check vip
                    Globals.Global_Data.Main.UserPref.IsVIP = Utilities.TCP.isVip(Globals.Global_Data.PlayerID.ToString());
                    Debug.WriteLine("IsVip: " + Globals.Global_Data.Main.UserPref.IsVIP.ToString());
                    string j = Utilities.TCP.ReadURL("http://95.111.251.138/gdm/isRainbow.php?id=" + Globals.Global_Data.PlayerID.ToString()).Result;
                    var deserializedProduct = JsonConvert.DeserializeObject<Utilities.JSONModels.ClientData>(j);
                    Globals.Global_Data.Main.UserPref.ShowSelfRainbow = Convert.ToBoolean(deserializedProduct.israinbow);
                    Globals.Global_Data.Main.UserPref.ShowSelfRainbowPastel = Convert.ToBoolean(deserializedProduct.israinbowpastel);

                    Color color = (Color)ColorConverter.ConvertFromString(deserializedProduct.hexcolor);

                    Globals.Global_Data.Main.UserPref.R = color.R;
                    Globals.Global_Data.Main.UserPref.G = color.G;
                    Globals.Global_Data.Main.UserPref.B = color.B;

                    if (Globals.Global_Data.Initializer.iconsDownloaded)
                        Globals.Global_Data.Main.Master.SaveUserPref();

                    Globals.Global_Data.Initializer.DownloadSelfIcons();

                    if (!Globals.Global_Data.Main.UserPref.IsVIP)
                    {
                        Globals.Global_Data.Main.UserPref.RenderCustomIcons = false;
                        DisableCustomIcons();
                    }
                    // Globals.Paths.Initializer.SetMyPFP();
                }
                catch (Exception ex)
                {
                    Utilities.Utils.HandleException(ex);
                }

                Debug.WriteLine("Account ID: " + id);
            }).Start();

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Main.accountID.Text = Globals.Global_Data.Lang.Accountid.Replace("%accountid%", id);
            }));
        }
        public void SetPing(string milliseconds)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (ServerIndex == 0)
                    Main.sg_ping.Text = milliseconds;
                else if (ServerIndex == 1)
                    Main.sg_ping2.Text = milliseconds;
            }));
        }
        public string GetRoom(short room)
        {
            switch (room)
            {
                case 0:
                    return "Lobby : Public";
                default:
                    return "Lobby : " + Utilities.Converter.ToString(room);

            }
        }
        public void SetLocalPort(int milliseconds)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Main.server_online.Text = "Local Port : " + milliseconds.ToString();
            }));
        }
        public void InitializeDirectories()
        {
            if (!Directory.Exists(Globals.Paths.DataFolder))
                Directory.CreateDirectory(Globals.Paths.DataFolder);

            if (!Main.UserPref.CachedIcons)
                if (Directory.Exists(Globals.Paths.IconsFolder))
                    Utilities.Utils.DeleteDirectory(Globals.Paths.IconsFolder);

            if (!Directory.Exists(Globals.Paths.IconsFolder))
                Directory.CreateDirectory(Globals.Paths.IconsFolder);

            if (Directory.Exists(Globals.Paths.TempIcons))
                Utilities.Utils.DeleteDirectory(Globals.Paths.TempIcons);
        }
    }
}
