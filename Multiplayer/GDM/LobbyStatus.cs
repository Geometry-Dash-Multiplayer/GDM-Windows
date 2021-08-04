using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Multiplayer.GDM
{
    public static class LobbyStatus
    {
        public static bool hasStarted = false;
        public static void Start()
        {
            if (hasStarted) return;
            hasStarted = true;

            var g = new UI.Controls.Levels_Panel();

            new Thread(() =>
            {
                while (true)
                {
                    Globals.Global_Data.Main.StartAnimation("ShowRefreshList");
                    string apiUrl = "http://" + Globals.Global_Data.ActiveServer + "/gdm/lobbies/" + Globals.Global_Data.Room.ToString() + ".json";
                    string result = Utilities.TCP.ReadURL(apiUrl).Result;
                    if (!string.IsNullOrEmpty(result))
                    {
                        int Index = 0;
                        Utilities.JSONModels.lc deserializedProduct =
                        JsonConvert.DeserializeObject<Utilities.JSONModels.lc>(result);
                        if (deserializedProduct != null)
                        {
                            List<int> availableLevels = new List<int>();
                            var sotedEntry = from entry in deserializedProduct.levels orderby entry.Value.Players descending select entry;
                            foreach (var h in sotedEntry)
                            {
                                if (h.Key <= 0) { continue; }
                                string output = "-1";
                                // MessageBox.Show("cached? " + GlobalData.Main.UserPref.CachedLevels.ToString());
                                if (Globals.Global_Data.Main.UserPref.CachedLevels)
                                {
                                    if (!Utilities.JSONModels.LevelCache.LevelIDandData.TryGetValue(h.Key, out output))
                                    {
                                        output = Utilities.TCP.GetLevelDataResponse(h.Key.ToString());

                                        Utilities.JSONModels.LevelCache.LevelIDandData.Add(h.Key, output);
                                    }
                                }
                                else output = Utilities.TCP.GetLevelDataResponse(h.Key.ToString());
                                availableLevels.Add(h.Key);

                                if (!g.DoesExist(h.Key))
                                {
                                    if (output != "-1")
                                    {
                                        Utilities.RubParser i = new Utilities.RubParser(output);
                                        if (i.Parse())
                                        {
                                            var CurrentLevelData = new Utilities.JSONModels.LevelID();
                                            CurrentLevelData.name = i.KeysAndCrates[2];
                                            CurrentLevelData.difficultyFace = i.GetDifficultyFace();
                                            if (CurrentLevelData.difficultyFace != null)
                                            {
                                                // r u = Utilities.TCP.FromLevelDiff(CurrentLevelData.difficultyFace);
                                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                                {
                                                    try
                                                    {
                                                        BitmapImage image = new BitmapImage(new Uri("UI/Images/Difficulties/" + CurrentLevelData.difficultyFace + ".png", UriKind.Relative));

                                                        g.AddPanel(h.Key, h.Value.Players, CurrentLevelData.name, image);

                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Utilities.Utils.HandleException(ex);
                                                    }
                                                }));
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Application.Current.Dispatcher.Invoke(new Action(() =>
                                    {
                                        g.SetPlayers(h.Key, h.Value.Players);
                                    }));
                                }
                                Index++;
                            }
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                // remove levels wid no plaers
                                g.Refresh(availableLevels);
                            }));
                            availableLevels.Clear();
                        }
                    }
                    else
                    {

                    }
                    Globals.Global_Data.Main.StartAnimation("UnShowRefreshList");
                    try
                    {
                        Globals.Global_Data.Initializer.SaveCaches();
                    }
                    catch (Exception ex)
                    {
                        Globals.Global_Data.HandleException(ex);
                    }
                    Thread.Sleep(5000);
                }
            }).Start();
        }
    }
}
