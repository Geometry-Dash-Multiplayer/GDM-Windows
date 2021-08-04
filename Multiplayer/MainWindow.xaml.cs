using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Multiplayer.GDM;

namespace Multiplayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public GDM.Initialize Master;
        public GDM.Preferences UserPref = new GDM.Preferences();
        public MainWindow()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            {
                Utilities.Utils.HandleException(eventArgs.Exception, "Noise", true);
            };
            this.WindowStyle = WindowStyle.SingleBorderWindow;

            GDM.Globals.Global_Data.Main = this;

            Master = new GDM.Initialize(this);

            Placeholders.Visibility = Visibility.Collapsed;
            pl_pop_lvl_container.Visibility = Visibility.Collapsed;
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
            e.Handled = true;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

            UI.TransparencyFix transparencyFix = new UI.TransparencyFix(this);
            transparencyFix.MakeTransparent();

            if (!UserPref.IsVIP)
                Task.Run(() =>
                {
                    Task.Delay(3000);
                    StartAnimation("ShowCost");
                });

            settings.Visibility = Visibility.Visible;
            this.StateChanged += MainWindow_StateChanged;

            GDM.Player_Watcher.Memory.Start();
            foreach (var foo in Enum.GetValues(typeof(Utilities.FormLongs)))
            {
                GDM.Client.Client.IconsAndIDs.Add((int)foo, foo.ToString());
            }
            vinfo.Text = "public release " + GDM.Globals.Global_Data.Version.ToString("0.00") + "-main";
            border6.Opacity = 0;

            // MessageBox.Show((RenderCapability.Tier >> 16).ToString());

        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            GDM.Globals.Global_Data.MainState = this.WindowState;
        }

        public void ShowError(string title, string desc, string help = "https://discord.gg/mNvPDCgB5M", string helpS = "Need Help?")
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                StartAnimation("ErrorApp");
                e_title.Text = title;
                e_desc.Text = desc;
                n_h.Tag = help;
                n_h.Content = helpS;
            }));
        }
        public void StartAnimation(string animation)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Storyboard board = (Storyboard)this.FindResource(animation);
                board.Begin();
                if (UserPref.MinimalAnimations)
                {
                    board.SkipToFill();
                }
            }));
        }

        private void Exit(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Master.SaveUserPref();
                Master.SaveCaches();
                GDM.Load_Language.SaveJSON();
                if (GDM.Globals.Global_Data.Connection != null)
                    GDM.Globals.Global_Data.Connection.SendDiconnect();
                if (e != null)
                    e.Handled = true;
            }
            catch (Exception ex)
            {
                Utilities.Utils.HandleException(ex);
            }
            Environment.Exit(0);
        }

        private void Minimize(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            e.Handled = true;
        }

        private void Closing_(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Exit(null, null);
        }

        private void OpenTag(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement).Tag != null)
            {
                string tag = (sender as FrameworkElement).Tag.ToString();
                if (tag.Contains("http"))
                    Process.Start(tag);
                else
                {
                    StartAnimation(tag);
                }
                StartAnimation("ErrorDis");
                if (e != null)
                    e.Handled = true;
            }
        }

        private void CheckVIP(object sender, RoutedEventArgs e)
        {
            // check if actual VIP from server
            if (!UserPref.IsVIP) ShowError(GDM.Globals.Global_Data.Lang.YouNeedVIP, GDM.Globals.Global_Data.Lang.NeedToCustomize, "ShowCost", GDM.Globals.Global_Data.Lang.BuyVIP);
            else Process.Start("https://adaf.xyz/gdm/vip/index.php?u=" + GDM.Globals.Global_Data.Username);
            e.Handled = true;
        }

        private void ShowSettings(object sender, RoutedEventArgs e)
        {
            settings.IsOpen = true;
            windowname.Text = UserPref.WindowName;
            modulename.Text = UserPref.MainModule;

            minimalanims.IsChecked = UserPref.MinimalAnimations;

            renderselficons.IsChecked = UserPref.RenderCustomIcons;
            showplayerusernames.IsChecked = GDM.Globals.Global_Data.ShowUsernames;

            playersopactiry.Value = UserPref.PlayersOpacity;
        }

        private void ApplySettings(object sender, RoutedEventArgs e)
        {
            try
            {
                settings.IsOpen = false;

                if (!string.IsNullOrEmpty(windowname.Text))
                    UserPref.WindowName = windowname.Text;
                if (!string.IsNullOrEmpty(modulename.Text))
                    UserPref.MainModule = modulename.Text;

                bool restart = false;
                if (UserPref.MinimalAnimations != (bool)minimalanims.IsChecked)
                {
                    restart = true;
                }

                UserPref.MinimalAnimations = (bool)minimalanims.IsChecked;
                UserPref.RenderCustomIcons = (bool)renderselficons.IsChecked;
                GDM.Globals.Global_Data.ShowUsernames = (bool)showplayerusernames.IsChecked;
                UserPref.PlayersOpacity = (float)playersopactiry.Value;
                UserPref.ShowSelfUsername = GDM.Globals.Global_Data.ShowUsernames;
                Debug.WriteLine("Player Opacity: " + UserPref.PlayersOpacity);

                if (!string.IsNullOrEmpty(vipkey_.Text))
                {

                    var bytes = Utilities.Converter.StringToByteArray(vipkey_.Text.Replace(" ", ""));
                    UserPref.Key = bytes;
                    GDM.Globals.Global_Data.VipKey = BitConverter.ToInt32(bytes, 0);
                    Debug.WriteLine("Key: " + GDM.Globals.Global_Data.VipKey);
                    GDM.Globals.Global_Data.VIPKeyOk = true;
                    UserPref.IsVIP = Utilities.TCP.isVip(GDM.Globals.Global_Data.PlayerID.ToString(), GDM.Globals.Global_Data.VipKey);

                }

                Master.SaveUserPref();
                if (restart)
                {
                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                }

            }
            catch (Exception ex)
            {
                Utilities.Utils.HandleException(ex);
            }

        }

        private void ResetSettings(object sender, RoutedEventArgs e)
        {
            Master.ResetPrefs();
            settings.IsOpen = false;
        }

        private void CloseCost(object sender, MouseButtonEventArgs e)
        {
            StartAnimation("UnShowCost");
            Task.Delay(1000);
        }

        private void CloseMb(object sender, MouseButtonEventArgs e)
        {
            StartAnimation("ErrorDis");
        }

        private void AboutHover(object sender, MouseEventArgs e)
        {
            StartAnimation("AboveHover");
        }

        private void UnAboutHover(object sender, MouseEventArgs e)
        {
            StartAnimation("UnAboveHover");
        }

        private void ConsiderVIP(object sender, MouseButtonEventArgs e)
        {
            OpenTag(sender, e);
            StartAnimation("UnShowCost");
            Task.Delay(1000);
        }

        private void border4_MouseLeave(object sender, MouseEventArgs e)
        {
            StartAnimation("UnHoverPay");
        }

        private void border4_MouseEnter(object sender, MouseEventArgs e)
        {
            StartAnimation("HoverPay");
        }

        private void ShowLobbies(object sender, RoutedEventArgs e)
        {
            lobbies.IsOpen = true;
            try
            {
                OpenTag(sender, null);
            }
            catch (Exception ex)
            {
                GDM.Globals.Global_Data.HandleException(ex);
            }
            e.Handled = true;
        }
        public async void SetLobby(short room)
        {
            GDM.Globals.Global_Data.IsPlayingLevel = false;
            GDM.Globals.Global_Data.Room = room;
            Master.SetRoom(GDM.Globals.Global_Data.Room);
            Debug.WriteLine("lobby int16: " + GDM.Globals.Global_Data.Room);
            await Task.Delay(3000);
            if (GDM.Globals.Global_Data.Connection != null)
            {
                GDM.Globals.Global_Data.Connection.model.players.Clear();
                try
                {
                    foreach (var h in GDM.Globals.Global_Data.Connection.model.players) h.Disconnected();
                    if (Master != null)
                        Master.ClearPlayers();
                    GDM.Globals.Global_Data.Connection.model.players.Clear();
                }
                catch (Exception ex)
                {
                    GDM.Globals.Global_Data.HandleException(ex);
                }
            }
            GDM.Globals.Global_Data.IsPlayingLevel = true;

        }
        private void ChangeLobby(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tb_lc.Text))
                {
                    SetLobby(0000);
                    Master.Announce("You've joined the Public lobby!");
                }
                else
                {
                    var bytes = Utilities.Converter.StringToByteArray(tb_lc.Text.Replace(" ", ""));
                    SetLobby(BitConverter.ToInt16(bytes, 0));
                }
            }
            catch (Exception ex)
            {
                GDM.Globals.Global_Data.HandleException(ex);
            }

            lobbies.IsOpen = false;
        }

        private void HideLobbies(object sender, MouseButtonEventArgs e)
        {
            lobbies.IsOpen = false;
        }

        private void CopyRoom(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(Utilities.Converter.ToString(GDM.Globals.Global_Data.Room));
        }

        private void Reconnect(object sender, RoutedEventArgs e)
        {
            if (Master.Connection != null)
                Master.Connection.Reconnect();
        }

        private void NewLobby(object sender, MouseButtonEventArgs e)
        {
            lobbies.IsOpen = false;
            if (GDM.Globals.Global_Data.Connection == null)
            {
                Master.Announce("Please connect to a server first.");
                return;
            }
            try
            {
                e.Handled = true;
                if (!UserPref.IsVIP)
                {
                    ShowError(GDM.Globals.Global_Data.Lang.YouNeedVIP, GDM.Globals.Global_Data.Lang.NeedToCreateLobby, "ShowCost", GDM.Globals.Global_Data.Lang.BuyVIP);
                    return;
                }
                else
                {
                    // var bytes = Utilities.Converter.StringToByteArray(tb_lc.Text); SetLobby(BitConverter.ToInt16(bytes, 0));
                    var h = (short)Utilities.Randomness.rand.Next(short.MinValue, short.MaxValue);
                    SetLobby(h);
                    string Copyable = Utilities.Converter.BytesToString(BitConverter.GetBytes(h)).Replace(" ", "").ToLower();
                    Clipboard.SetText(Copyable);
                    Master.Announce("Lobby code copied to clipboard.");
                    SetLobby(h);
                    lobbyC.Text = Copyable;
                    lobbyCopied.Visibility = Visibility.Visible;
                    lobbyCopied.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                GDM.Globals.Global_Data.HandleException(ex);
            }
        }

        private async void ChangeKey(object sender, RoutedEventArgs e)
        {
            try
            {
                var bytes = Utilities.Converter.StringToByteArray(password.Password.Replace(" ", ""));
                UserPref.Key = bytes;
                GDM.Globals.Global_Data.VipKey = BitConverter.ToInt32(bytes, 0);
                Debug.WriteLine("Key: " + GDM.Globals.Global_Data.VipKey);
                GDM.Globals.Global_Data.VIPKeyOk = true;
                vipstatus.Text = "Checking...";
                await Task.Run(() =>
                {
                    try
                    {
                        UserPref.IsVIP = Utilities.TCP.isVip(GDM.Globals.Global_Data.PlayerID.ToString(), GDM.Globals.Global_Data.VipKey);
                        if (UserPref.IsVIP)
                        {

                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                vipstatus.Text = "VIP Status confirmed!";
                            }));

                            UserPref.RenderCustomIcons = true;
                            Master.ClearSelfIcons();
                            Master.DownloadSelfIcons();
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                vipstatus.Text = "Not VIP...";
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        GDM.Globals.Global_Data.HandleException(ex);
                    }
                    Task.Delay(2000);
                });
                Master.SaveUserPref();

            }
            catch (Exception ex)
            {
                GDM.Globals.Global_Data.HandleException(ex);
            }
            login.IsOpen = false;
            new Thread(() => { Master.DownloadSelfIcons(); }).Start();

        }

        private void CloseLobbyDialogue(object sender, RoutedEventArgs e)
        {
            lobbyCopied.IsOpen = false;
        }

        private async void EnterKey(object sender, MouseButtonEventArgs e)
        {
            StartAnimation("UnShowCost");
            if (!GDM.Globals.Global_Data.PlayerIDLoaded)
                await Task.Run(() =>
                {
                    Master.WaitForGeometryDash();
                });
            Master.Relogin();
        }

        private void ShowPlayerUsernames(object sender, RoutedEventArgs e)
        {
            try
            {
                GDM.Globals.Global_Data.ShowUsernames = true;
                UserPref.ShowSelfUsername = GDM.Globals.Global_Data.ShowUsernames;
                if (GDM.Globals.Global_Data.Connection != null)
                    foreach (var j in GDM.Globals.Global_Data.Connection.model.players)
                    {
                        j.ShowUsername();
                    }
            }
            catch (Exception ex)
            {
                GDM.Globals.Global_Data.HandleException(ex);
            }
        }

        private void HidePlayerUsernames(object sender, RoutedEventArgs e)
        {
            try
            {
                GDM.Globals.Global_Data.ShowUsernames = false;
                UserPref.ShowSelfUsername = GDM.Globals.Global_Data.ShowUsernames;
                if (GDM.Globals.Global_Data.Connection != null)
                    foreach (var j in GDM.Globals.Global_Data.Connection.model.players)
                    {
                        j.HideUsername();
                    }
            }
            catch (Exception ex)
            {
                GDM.Globals.Global_Data.HandleException(ex);
            }
        }

        private void ClearCache(object sender, RoutedEventArgs e)
        {
            Master.ClearCaches();
        }

        private void rienjectdll(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {

                if (GDM.Player_Watcher.Memory.aMemory != null)
                {
                    GDM.Player_Watcher.Memory.aMemory.isAlreadyInjected = false;
                    if (!GDM.Player_Watcher.Memory.aMemory.dllInject(GDM.Globals.Global_Data.DLLPath)) ;
                }
            }).Start();
            settings.IsOpen = false;
        }

        private void SetLangEn(object sender, MouseButtonEventArgs e)
        {
            UserPref.Lang = "en";
            GDM.Load_Language.Load();
            langs.IsOpen = false;
        }

        private void SetLangRu(object sender, MouseButtonEventArgs e)
        {
            UserPref.Lang = "ru";
            GDM.Load_Language.Load();
            langs.IsOpen = false;
        }

        private void changelang(object sender, RoutedEventArgs e)
        {
            langs.IsOpen = true;
            settings.IsOpen = false;
        }

        private void SizeChangedf(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 340)
            {
                brand.Visibility = Visibility.Collapsed;
                menuItem.Visibility = Visibility.Collapsed;
                settings1.Visibility = Visibility.Collapsed;
                about.Visibility = Visibility.Collapsed;
                customizer.Visibility = Visibility.Collapsed;
                stackPanel2.Visibility = Visibility.Collapsed;

                leftgrid.Width = double.NaN;
            }
            else
            {
                brand.Visibility = Visibility.Visible;
                menuItem.Visibility = Visibility.Visible;
                settings1.Visibility = Visibility.Visible;
                about.Visibility = Visibility.Visible;
                customizer.Visibility = Visibility.Visible;
                stackPanel2.Visibility = Visibility.Visible;

                leftgrid.Width = 210d;
            }
            e.Handled = true;
        }

        private void SAbout(object sender, RoutedEventArgs e)
        {
            StartAnimation("ShowAbout");
        }

        private void HideAbout(object sender, MouseButtonEventArgs e)
        {
            StartAnimation("ShowAbout_R");
        }

        private void SetLangSp(object sender, MouseButtonEventArgs e)
        {

            UserPref.Lang = "es";
            GDM.Load_Language.Load();
            langs.IsOpen = false;
        }

        private void OpenServer(object sender, MouseButtonEventArgs e)
        {
            var ui_sender = sender as FrameworkElement;
            int server_index = int.Parse(ui_sender.Tag.ToString());
            if (GDM.Globals.Global_Data.PlayerID != 0 && GDM.Globals.Global_Data.IsInjected) Master.ServerConnected(server_index);
            else if (!GDM.Globals.Global_Data.IsInjected)
            {
                if (!GDM.Globals.Global_Data.IsGDThere)
                    Master.Announce(GDM.Globals.Global_Data.Lang.PleaseRunGD);
                else Master.Announce(GDM.Globals.Global_Data.Lang.DLLInjecting);
            }
            else
            {
                ShowError("You're not registered on Geometry Dash", "Please login an account on Geometry Dash to play Multiplayer, need help?", "https://www.youtube.com/watch?v=2KcWgc3xYhc");
            }
            e.Handled = true;
        }
    }
}
