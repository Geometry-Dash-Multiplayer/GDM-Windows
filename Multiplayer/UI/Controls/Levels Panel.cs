using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Multiplayer.UI.Controls
{
    public class Levels_Panel
    {
        public Dictionary<int, LevelPanel>
            Lpanels = new Dictionary<int, LevelPanel>();
        public void Refresh(IEnumerable<int> y)
        {
            foreach (var LevelInJson in Lpanels.Keys.ToList())
            {
                if (!y.Contains(LevelInJson))
                {
                    RemovePanel(LevelInJson);
                }
            }
        }
        public void Clear()
        {
            Lpanels.Clear();
            GDM.Globals.Global_Data.Main.levels.Children.Clear();
        }
        public static void RemoveChild(DependencyObject parent, UIElement child)
        {
            var panel = parent as Panel;
            if (panel != null)
            {
                panel.Children.Remove(child);
                return;
            }

            var decorator = parent as Decorator;
            if (decorator != null)
            {
                if (decorator.Child == child)
                {
                    decorator.Child = null;
                }
                return;
            }

            var contentPresenter = parent as ContentPresenter;
            if (contentPresenter != null)
            {
                if (contentPresenter.Content == child)
                {
                    contentPresenter.Content = null;
                }
                return;
            }

            var contentControl = parent as ContentControl;
            if (contentControl != null)
            {
                if (contentControl.Content == child)
                {
                    contentControl.Content = null;
                }
                return;
            }

            // maybe more
        }
        public void MakeTop(int id)
        {
            try
            {
                if (GDM.Globals.Global_Data.Main.levels.Children[0] != null)
                {
                    if ((int)((Border)GDM.Globals.Global_Data.Main.levels.Children[0]).Tag == id)
                    {
                        return;
                    }
                }
                var toBeRemoved = GDM.Globals.Global_Data.Main.levels.Children[0];
                var toBeAdded = GDM.Globals.Global_Data.Main.levels.Children[GDM.Globals.Global_Data.Main.levels.Children.IndexOf(Lpanels[id].MainB)];
                GDM.Globals.Global_Data.Main.levels.Children.Remove(toBeRemoved);
                GDM.Globals.Global_Data.Main.levels.Children.Remove(toBeAdded);
                RemoveChild(GDM.Globals.Global_Data.Main.levels, toBeRemoved);
                RemoveChild(GDM.Globals.Global_Data.Main.levels, toBeAdded);
                GDM.Globals.Global_Data.Main.levels.Children.Insert(0, toBeAdded);
                GDM.Globals.Global_Data.Main.levels.Children.Insert(1, toBeRemoved);
                // var h = GDM.Globals.Global_Data.Main.levels.Children[d]; 
                // = GDM.Globals.Global_Data.Main.levels.Children[0];

                // GDM.Globals.Global_Data.Main.levels.Children[0] = h;
            }
            catch (Exception ex)
            {
                Utilities.Utils.HandleException(ex);
            }
        }
        public bool DoesExist(int id)
        {
            return Lpanels.ContainsKey(id);
        }
        public void RemovePanel(int id)
        {
            try
            {
                if (!GDM.Globals.Global_Data.Main.UserPref.MinimalAnimations)
                {
                    var anim = new DoubleAnimation
                    {
                        To = 0,
                        EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut },
                        Duration = TimeSpan.FromMilliseconds(2000)
                    };

                    anim.Completed += (s, e) =>
                    {
                        Lpanels[id].MainB.Height = 0;

                        GDM.Globals.Global_Data.Main.levels.Children.Remove(
                            Lpanels[id].MainB
                            );
                        Lpanels.Remove(id);

                    };

                    Lpanels[id].MainB.BeginAnimation(Border.HeightProperty, anim);
                }
                else
                {

                    Lpanels[id].MainB.Height = 0;

                    GDM.Globals.Global_Data.Main.levels.Children.Remove(
                        Lpanels[id].MainB
                        );
                    Lpanels.Remove(id);
                }

            }
            catch (Exception ex)
            {
                Utilities.Utils.HandleException(ex);
            }
        }
        public void AddPanel(int id, int players, string level, BitmapImage icon)
        {
            try
            {
                if (!Lpanels.ContainsKey(id))
                {
                    Lpanels[id] = new LevelPanel();
                    Lpanels[id].CreateLevelPanel(level, icon, players);
                    GDM.Globals.Global_Data.Main.levels.Children.Add(Lpanels[id].MainB);
                    Lpanels[id].SetLevelID(id);
                }
            }
            catch (Exception ex)
            {
                Utilities.Utils.HandleException(ex);
            }
        }
        static public void BringToFront(StackPanel pParent, Border pToMove)
        {
            try
            {
                int currentIndex = Canvas.GetZIndex(pToMove);
                int zIndex = 0;
                int maxZ = 0;
                UserControl child;
                for (int i = 0; i < pParent.Children.Count; i++)
                {
                    if (pParent.Children[i] is UserControl &&
                        pParent.Children[i] != pToMove)
                    {
                        child = pParent.Children[i] as UserControl;
                        zIndex = Canvas.GetZIndex(child);
                        maxZ = Math.Max(maxZ, zIndex);
                        if (zIndex > currentIndex)
                        {
                            Canvas.SetZIndex(child, zIndex - 1);
                        }
                    }
                }
                Canvas.SetZIndex(pToMove, maxZ);
            }
            catch (Exception ex)
            {
            }
        }
        public void PutTo(int id, int index)
        {
            try
            {
                if (index >= 1)
                {
                    int j = GDM.Globals.Global_Data.Main.levels.Children.IndexOf(Lpanels[id].MainB);
                    Debug.WriteLine("J index:" + j);
                    GDM.Globals.Global_Data.Main.levels.Children.RemoveAt(j);
                    GDM.Globals.Global_Data.Main.levels.Children.Insert(j - 1, Lpanels[id].MainB);
                    // BringToFront(GDM.Globals.Global_Data.Main.levels, Lpanels[id].MainB);
                }
            }
            catch (Exception ex)
            {
                Utilities.Utils.HandleException(ex);
            }
        }
        public void SetPlayers(int id, int players)
        {
            try
            {
                if (Lpanels.ContainsKey(id))
                {
                    Lpanels[id].SetPlayerCount(players);
                }
                // reorder

            }
            catch (Exception ex)
            {
                Utilities.Utils.HandleException(ex);
            }
        }
    }
    public class LevelPanel
    {
        public Border MainB;
        public TextBlock playerCount;
        public TextBlock levelID;
        public int Players = 0;
        public int LevelID = 0;
        public void SetLevelID(int id)
        {
            LevelID = id;
            levelID.Text = id.ToString();
            MainB.Tag = id;
        }
        public void SetPlayerCount(int h)
        {
            if (playerCount != null)
            {
                playerCount.Text = h.ToString() + " playing";
                Players = h;
            }
        }
        public Border CreateLevelPanel(string lname, BitmapImage im, int players)
        {
            MainB = new Border
            {
                CornerRadius = GDM.Globals.Global_Data.Main.pl_pop_lvl_container.CornerRadius,
                Background = GDM.Globals.Global_Data.Main.pl_pop_lvl_container.Background,
                Height = 0,
                Margin = GDM.Globals.Global_Data.Main.pl_pop_lvl_container.Margin,
                ToolTip = "Copy Level ID!",
                Cursor = GDM.Globals.Global_Data.Main.pl_pop_lvl_container.Cursor
            };

            ToolTipService.SetInitialShowDelay(MainB, 0);
            ToolTipService.SetBetweenShowDelay(MainB, 0);

            var dp = new DockPanel
            {
                Margin = GDM.Globals.Global_Data.Main.pl_pop_lvl_dock1.Margin
            };
            MainB.Child = dp;
            var ims = new Image
            {
                Source = im,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Margin = GDM.Globals.Global_Data.Main.pl_pop_lvl_image.Margin
            };
            dp.Children.Add(ims);
            DockPanel.SetDock(ims, Dock.Left);

            var st = new StackPanel
            {
                Orientation = GDM.Globals.Global_Data.Main.pl_pop_st.Orientation,
            };

            var tb = new TextBlock
            {
                Margin = GDM.Globals.Global_Data.Main.pl_pop_lvl_levelname.Margin,
                Text = lname,
                FontSize = GDM.Globals.Global_Data.Main.pl_pop_lvl_levelname.FontSize,
                VerticalAlignment = GDM.Globals.Global_Data.Main.pl_pop_lvl_levelname.VerticalAlignment
            };

            st.Children.Add(tb);

            levelID = new TextBlock
            {
                Margin = GDM.Globals.Global_Data.Main.pl_pop_lvl_levelname.Margin,
                Text = lname,
                FontSize = GDM.Globals.Global_Data.Main.pl_pop_lvl_levelid.FontSize,
                VerticalAlignment = GDM.Globals.Global_Data.Main.pl_pop_lvl_levelid.VerticalAlignment,
                Opacity = 0.75
            };
            st.Children.Add(levelID);

            dp.Children.Add(st);
            DockPanel.SetDock(st, Dock.Top);

            playerCount = new TextBlock
            {
                Margin = GDM.Globals.Global_Data.Main.pl_pop_lvl_playercount.Margin,
                Text = players.ToString() + " playing",
                FontSize = GDM.Globals.Global_Data.Main.pl_pop_lvl_playercount.FontSize,
                VerticalAlignment = GDM.Globals.Global_Data.Main.pl_pop_lvl_playercount.VerticalAlignment,
                Opacity = 0.75
            };
            dp.Children.Add(playerCount);

            MainB.MouseDown += (q, w) =>
            {
                try
                {
                    Clipboard.SetText(levelID.Text);
                    GDM.Globals.Global_Data.Initializer.Announce("Level ID (" + levelID.Text + ") copied to clipboard!");
                }
                catch { }
            };

            DockPanel.SetDock(playerCount, Dock.Top);


            if (!GDM.Globals.Global_Data.Main.UserPref.MinimalAnimations)
            {
                // do animation
                var anim = new DoubleAnimation
                {
                    To = GDM.Globals.Global_Data.Main.pl_pop_lvl_container.Height,
                    EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut },
                    Duration = TimeSpan.FromMilliseconds(1000)
                };

                anim.Completed += (s, e) =>
                {
                    MainB.Height = GDM.Globals.Global_Data.Main.pl_pop_lvl_container.Height;
                };

                MainB.BeginAnimation(Border.HeightProperty, anim);
            }
            else
            {
                MainB.Height = GDM.Globals.Global_Data.Main.pl_pop_lvl_container.Height;
            }
            return MainB;
        }
    }
}
