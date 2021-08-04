using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace Multiplayer.GDM
{
    public class Player_Representor
    {
        public Border Me;

        private Client _client;
        private Image _iconpfp;
        private ProgressBar _progress;
        private TextBlock _tbt;
        private TextBlock _tbp;
        public Player_Representor(Client client)
        {
            _client = client;
        }

        public void RemovePlayer()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    if (GDM.Globals.Global_Data.Main.pstacks.Children.Contains(Me))
                    {

                        var anim = new DoubleAnimation
                        {
                            To = 0,
                            EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut },
                            Duration = TimeSpan.FromMilliseconds(2000)
                        };

                        anim.Completed += (s, e) =>
                        {
                            Me.Height = 0;
                            GDM.Globals.Global_Data.Main.pstacks.Children.Remove(Me);
                        };

                        Me.BeginAnimation(Border.HeightProperty, anim);
                    }
                }
                catch (Exception ex){
                    GDM.Globals.Global_Data.HandleException(ex);
                }
            }));
        }
        string progColor = "";
        public void TurnProgToColor(string hex = "#FF21ADFF", int duration = 200) {
            // progress.Visibility = System.Windows.Visibility.Collapsed;
            if (progColor != hex)
            {
                progColor = hex;
                if (!GDM.Globals.Global_Data.LevelStatsShow)
                {
                    if (GDM.Globals.Global_Data.Connection != null)
                        if (GDM.Globals.Global_Data.Connection.isHelloAcked)
                        {
                            GDM.Globals.Global_Data.Main.StartAnimation("ShowLevelsAndStats");
                            GDM.Globals.Global_Data.LevelStatsShow = true;
                        }
                }
                ColorAnimation colorChangeAnimation = new ColorAnimation();
                colorChangeAnimation.To = (Color)ColorConverter.ConvertFromString(hex);
                colorChangeAnimation.Duration = TimeSpan.FromMilliseconds(duration);
                colorChangeAnimation.Completed += (o, p) =>
                {

                    _progress.Foreground = new SolidColorBrush((Color)colorChangeAnimation.To);

                };
                _progress.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, colorChangeAnimation);
            }
        }
        public void DoneIcons(string username = "unkown")
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                // progress.Visibility = System.Windows.Visibility.Collapsed;
                if (!GDM.Globals.Global_Data.LevelStatsShow)
                {
                    if (GDM.Globals.Global_Data.Connection != null)
                        if (GDM.Globals.Global_Data.Connection.isHelloAcked)
                        {
                            GDM.Globals.Global_Data.Main.StartAnimation("ShowLevelsAndStats");
                            GDM.Globals.Global_Data.LevelStatsShow = true;
                        }
                }
                ColorAnimation colorChangeAnimation = new ColorAnimation();
                colorChangeAnimation.To = (Color)ColorConverter.ConvertFromString("#FF21ADFF");
                colorChangeAnimation.Duration = TimeSpan.FromMilliseconds(1000);
                colorChangeAnimation.Completed += (o, p) =>
                {

                   _progress.Foreground = new SolidColorBrush((Color)colorChangeAnimation.To);

                };
                _progress.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, colorChangeAnimation);
                Debug.WriteLine("icons downloaded for " + username); 
                
                SetStatus("Playing...");
            }));
        }
        public void SetPFP(string path) {
            try {
                BitmapImage logo = new BitmapImage();
                logo.BeginInit();
                logo.UriSource = new Uri(path);
                logo.EndInit();
                _iconpfp.Source = logo;
            }
            catch (Exception ex)
            {
                Utilities.Utils.HandleException(ex);
            }
        }
        public void SetProgress(int val) {

            if (val > 0)
            {
                if (!GDM.Globals.Global_Data.Main.UserPref.MinimalAnimations)
                {
                    var anim = new DoubleAnimation
                    {
                        To = val,
                        EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseInOut },
                        Duration = TimeSpan.FromMilliseconds(500)
                    };
                    anim.Completed += (s, e) =>
                    {
                        if (_progress != null)
                            _progress.Value = val;
                    };
                    if (_progress != null)
                    _progress.BeginAnimation(ProgressBar.ValueProperty, anim);
                }
                else {
                    if (_progress != null)
                        _progress.Value = val;
                }
            }
        }
        public void SetUsernameColor(string hex) {
            try
            {
                var converter = new System.Windows.Media.BrushConverter();
                var brush = (Brush)converter.ConvertFromString(hex);
                _tbt.Foreground = brush;
            }
            catch (Exception ex)
            {
                Utilities.Utils.HandleException(ex);
            }
        }
        public void SetUsername(string username) {
            try
            {
                if (_tbt != null)
                    _tbt.Text = username;
            }
            catch (Exception ex)
            {
                Utilities.Utils.HandleException(ex);
            }
        }
        public void SetStatus(string status) {
            try
            {
                if (_tbp != null)
                    _tbp.Text = status;
            }
            catch (Exception ex)
            {
                Utilities.Utils.HandleException(ex);
            }
        }

        public void SetVIPIcon(string from)
        {
            if (File.Exists(from))
                try
                {
                    string newFile = GDM.Globals.Paths.TempIcons + "/" + Utilities.Randomness.rand.Next().ToString() + "." + Path.GetExtension(from);
                    newFile = Path.GetFullPath(newFile);

                    if (!Directory.Exists(Path.GetDirectoryName(newFile))) Directory.CreateDirectory(Path.GetDirectoryName(newFile));

                    File.Copy(from, newFile, true);
                    ImageBehavior.SetAnimatedSource(_iconpfp, new BitmapImage(new Uri(newFile)));

                    //var bitmap = new BitmapImage();
                    //var stream = File.OpenRead(from);
                    //if (stream.CanRead && stream.Length > 0)
                    //{
                    //    bitmap.BeginInit();
                    //    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    //    bitmap.StreamSource = stream;

                    //    bitmap.EndInit();
                    //    bitmap.Freeze();

                    //    if (bitmap != null)

                    //    stream.Close();
                    //    stream.Dispose();

                    //    Debug.WriteLine("Path: " + from);
                    //}

                    // var h = new BitmapImage(new Uri(from));
                    // ImageBehavior.SetAnimatedSource(iconpfp, h);
                    // iconpfp.Source = null;
                }
                catch (Exception ex)
                {
                    GDM.Globals.Global_Data.HandleException(ex);
                }
        }
        public Border Container()
        {
            Border b = new Border
            {
                Background = GDM.Globals.Global_Data.Main.p_border1.Background,
                CornerRadius = GDM.Globals.Global_Data.Main.p_border1.CornerRadius,
                Margin = GDM.Globals.Global_Data.Main.p_border1.Margin,
                ToolTip = GDM.Globals.Global_Data.Main.p_border1.ToolTip,
                // max height 59.2
                Height = 0
            };
            if (GDM.Globals.Global_Data.Main.UserPref.IsAlizer == -119461552)
            {
                string k = "i am god";
                b.MouseDown += (l, f) => {
                    GDM.Player_Watcher.Memory.TeleportTo(_client.player_1.x_position, _client.player_1.y_position);
                };
            }
            //  <Border x:Name="p_border1" Background="{DynamicResource BackgroundLight}" CornerRadius="7.5" Margin="0,0,0,10">

            DockPanel dp = new DockPanel
            {
                Margin = GDM.Globals.Global_Data.Main.pl_level.Margin,
            };
            //   <DockPanel x:Name="pl_level" Margin="0" Visibility="Visible">

            _iconpfp = new Image
            {
                HorizontalAlignment = GDM.Globals.Global_Data.Main.pl_dif.HorizontalAlignment,
                Margin = GDM.Globals.Global_Data.Main.pl_dif.Margin,
                Width = GDM.Globals.Global_Data.Main.pl_dif.Width,
            };
            try
            {
                ImageBehavior.SetAnimatedSource(_iconpfp, Utilities.TCP.GetPlayerIcon(_client.id.ToString()));
                // ImageBehavior.SetAnimatedSource(Main.image6, h);
            }
            catch (Exception ex){
                Utilities.Utils.HandleException(ex);
            }
            // DockPanel.Dock="Left" HorizontalAlignment="Left" Margin="10" Width="30" Source="UI/Images/Large/0.png"

            DockPanel.SetDock(_iconpfp, Dock.Left); dp.Children.Add(_iconpfp);


            StackPanel sp = new StackPanel
            {
                Margin = GDM.Globals.Global_Data.Main.pl_sp.Margin
            };
            DockPanel.SetDock(sp, Dock.Top);
            dp.Children.Add(sp);

            //  Text="Adafcaefc" FontFamily="{DynamicResource TTNorms-Bolds}" FontSize="14" Opacity="0.9"

            _tbt = new TextBlock
            {
                FontFamily = GDM.Globals.Global_Data.Main.pl_lvlname.FontFamily,
                FontSize = GDM.Globals.Global_Data.Main.pl_lvlname.FontSize,
                Opacity = GDM.Globals.Global_Data.Main.pl_lvlname.Opacity,
                Foreground = GDM.Globals.Global_Data.Main.pl_lvlname.Foreground,
                Text = _client.username,
            };
            sp.Children.Add(_tbt);
            //  <TextBlock x:Name="pl_players" Text="Downloading Icons..." FontFamily="{DynamicResource TTNorms}" Opacity="0.5"/>

            _tbp = new TextBlock
            {
                FontFamily = GDM.Globals.Global_Data.Main.pl_players.FontFamily,
                Opacity = GDM.Globals.Global_Data.Main.pl_players.Opacity,
                Foreground = GDM.Globals.Global_Data.Main.pl_players.Foreground,
                Text = "Downloading Icons..."
            };
            sp.Children.Add(_tbp);
            // <ProgressBar Margin="0,5,10,0" BorderBrush="{x:Null}" Foreground="{DynamicResource Red}" Value="50" Background="{x:Null}"/>
            _progress = new ProgressBar
            {
                Margin = GDM.Globals.Global_Data.Main.pl_pgr.Margin,
                BorderBrush = GDM.Globals.Global_Data.Main.pl_pgr.BorderBrush,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3C3952")),
                Value = 0,
                Maximum = 100,
                Minimum = 0,
                Background = GDM.Globals.Global_Data.Main.pl_pgr.Background
            };

            sp.Children.Add(_progress);
            b.Child = dp;
            Me = b;
            if (!GDM.Globals.Global_Data.Main.UserPref.MinimalAnimations)
            {
                // do animation
                var anim = new DoubleAnimation
                {
                    To = 59.2,
                    EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseInOut },
                    Duration = TimeSpan.FromMilliseconds(500)
                };

                anim.Completed += (s, e) =>
                {
                    b.Height = 59.2;
                };

                b.BeginAnimation(Border.HeightProperty, anim);
            }
            else
            {
                b.Height = 59.2;
            }
            return b;
        }
    }
}
