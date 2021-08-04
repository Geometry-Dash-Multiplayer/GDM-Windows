using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.GDM
{
    public static class Load_Language
    {
        public static void Load() {

            if (Globals.Global_Data.Main.UserPref.Lang == null)
            {
                Globals.Global_Data.Main.langs.IsOpen = true;
                return;
            }

            Globals.Global_Data.LanguageFile = "Language-" + Globals.Global_Data.Main.UserPref.Lang + ".json";

            if (!File.Exists(Globals.Global_Data.LanguageFile)) {
                File.Create(Globals.Global_Data.LanguageFile).Close();
                SaveJSON();
            }
            string y = File.ReadAllText(Globals.Global_Data.LanguageFile);

            Globals.Global_Data.Lang = JsonConvert.DeserializeObject<Utilities.JSONModels.Language>(y);
            if (Globals.Global_Data.Lang == null) {
                Globals.Global_Data.Lang = new Utilities.JSONModels.Language();
            }

            Globals.Global_Data.Main.textBlock1.Text = Globals.Global_Data.Lang.SelectServer;
            Globals.Global_Data.Main.serverdesc.Text = Globals.Global_Data.Lang.Preferably;
            Globals.Global_Data.Main.europe1.Text = Globals.Global_Data.Lang.Europe;

            Globals.Global_Data.Main.menuItem.Content = Globals.Global_Data.Lang.Lobbies;
            Globals.Global_Data.Main.customizer.Content = Globals.Global_Data.Lang.Customize;
            Globals.Global_Data.Main.settings1.Content = Globals.Global_Data.Lang.Settings;
            Globals.Global_Data.Main.sg_ping.Text = Globals.Global_Data.Lang.Online;
            Globals.Global_Data.Main.sg_ping2.Text = Globals.Global_Data.Lang.Online;

            Globals.Global_Data.Main.vipaccfeatures.Text = Globals.Global_Data.Lang.VIPAccountFeatures;
            Globals.Global_Data.Main.feature1.Text = Globals.Global_Data.Lang.Feature1;
            Globals.Global_Data.Main.feature2.Text = Globals.Global_Data.Lang.Feature2;
            Globals.Global_Data.Main.feature3.Text = Globals.Global_Data.Lang.Feature3;
            Globals.Global_Data.Main.desc1.Text = Globals.Global_Data.Lang.Feature1Description;
            Globals.Global_Data.Main.desc2.Text = Globals.Global_Data.Lang.Feature2Description;
            Globals.Global_Data.Main.desc3.Text = Globals.Global_Data.Lang.Feature3Description;

            Globals.Global_Data.Main.settingss.Text = Globals.Global_Data.Lang.Settings;
            Globals.Global_Data.Main.settings5.Header = Globals.Global_Data.Lang.Settings;
            Globals.Global_Data.Main.windowname.Text = Globals.Global_Data.Lang.WindowName;
            Globals.Global_Data.Main.vipkey.Text = Globals.Global_Data.Lang.VIPKey;
            Globals.Global_Data.Main.rendercustomicons.Text = Globals.Global_Data.Lang.RenderSelfIcons;
            Globals.Global_Data.Main.renderusernames.Text = Globals.Global_Data.Lang.RenderUsernames;
            Globals.Global_Data.Main.playersopactiry1.Text = Globals.Global_Data.Lang.PlayersOpacity;
            Globals.Global_Data.Main.performancemode.Text = Globals.Global_Data.Lang.PerformanceMode;
            Globals.Global_Data.Main.reinjectdll.Content = Globals.Global_Data.Lang.ReinjectDLL;
            Globals.Global_Data.Main.changelange.Content = Globals.Global_Data.Lang.ChangeLang;
            Globals.Global_Data.Main.applysettings.Content = Globals.Global_Data.Lang.Apply;
            Globals.Global_Data.Main.resetsettings.Content = Globals.Global_Data.Lang.ClearData;

            Globals.Global_Data.Main.createjoinalobbi.Text = Globals.Global_Data.Lang.CreateJoinALobby;
            Globals.Global_Data.Main.tb_lobbyCode.Text = Globals.Global_Data.Lang.EnterALobbyCode;
            Globals.Global_Data.Main.createnewlobby.Text = Globals.Global_Data.Lang.CreateNewLobby;
            Globals.Global_Data.Main.joinlobby.Content = Globals.Global_Data.Lang.JoinLobby;

            Globals.Global_Data.Main.cache2.Header = Globals.Global_Data.Lang.Cache;
            Globals.Global_Data.Main.cachehelpsprevent.Header = Globals.Global_Data.Lang.CacheHelpsPrevent;
            Globals.Global_Data.Main.areiconscached.Header = Globals.Global_Data.Lang.Icons;
            Globals.Global_Data.Main.arelevelscached.Header = Globals.Global_Data.Lang.Levels;
            Globals.Global_Data.Main.areusernamescached.Header = Globals.Global_Data.Lang.Usernames;
            Globals.Global_Data.Main.clearcache.Header = Globals.Global_Data.Lang.ClearCache;
            Globals.Global_Data.Main.about.Content = Globals.Global_Data.Lang.About;

        }
        public static void SaveJSON() {
            string output = JsonConvert.SerializeObject(Globals.Global_Data.Lang, Formatting.Indented);
            File.WriteAllText(Globals.Global_Data.LanguageFile, output);
        }
    }
}
