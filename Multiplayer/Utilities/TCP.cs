using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Multiplayer.Utilities
{
    // todo
    public static class TCP
    {
        public static string GetLevelDataResponse(string levelid)
        {
            string result = null;
            while (string.IsNullOrWhiteSpace(result))
                result = Post("http://www.boomlings.com/database/getGJLevels21.php", "gameVersion=21&binaryVersion=35&gdw=0&type=0&str=" + levelid + "&diff=-&len=-&page=0&total=0&uncompleted=0&onlyCompleted=0&featured=0&original=0&twoPlayer=0&coins=0&epic=0&secret=Wmfd2893gb7");
            return result;
        }
        public static string GetUsernameFromPlayerID(int playerID)
        {
            string result = null;
            while (string.IsNullOrWhiteSpace(result))
                result = Post("http://www.boomlings.com/database/getGJUsers20.php", "gameVersion=21&binaryVersion=35&gdw=0&str=" + playerID.ToString() + "&total=0&page=0&secret=Wmfd2893gb7");
            return Utils.FindTextBetween(result, "1:", ":2:");
        }
        public static string Post(string url, string param)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);

                var postData = param;
                var data = Encoding.UTF8.GetBytes(postData);
                request.Proxy = null;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                request.Timeout = 5000;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return responseString;
            }
            catch
            {

            }
            return null;
        }
        public static bool isVip(string id, int key = 0)
        {
            string url = "http://95.111.251.138/gdm/isVip.php?id=" + id.Trim() + "&cv=true&pass=" + key.ToString();
            if (key == 0)
            {
                url = "http://95.111.251.138/gdm/isVip.php?id=" + id.Trim();
            }
            return bool.Parse(ReadURL(url).Result);
        }
        public static async Task<string> ReadURL(string url)
        {
            try
            {
                bool suc = false;
                while (!suc)
                {
                    using (WebClient client = new WebClient())
                    {
                        client.Proxy = null;
                        return client.DownloadString(new Uri(url, UriKind.Absolute));
                    }
                }
            }
            catch { }
            return string.Empty;
        }
        public static string GetCustomImageURL(string id, string form)
        {
            string h = $"{GDM.Globals.Endpoints.GetInfo}?id={id}&iid={form}";
            return ReadURL(h).Result;
        }

        public static BitmapImage GetPlayerIcon(string playerIDorUsername)
        {
            return FromURL("http://" + GDM.Globals.Endpoints.ProxiedGDBrowser + "/icon/" + playerIDorUsername.Trim());
        }
        public static string GetNewImageAsync(string url)
        {
            try
            {
                string filename = Utilities.Randomness.rand.Next().ToString();

                filename += ".png";
                string dataFolderName = GDM.Globals.Paths.DataFolder + "/" + filename;

                if (!File.Exists(filename))
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(new Uri(url), dataFolderName);
                    }
                return dataFolderName;
            }
            catch (Exception ex)
            {
                Utilities.Utils.HandleException(ex);
            }
            return null;
        }

        public static BitmapImage FromURL(string url)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(url);
            bitmapImage.EndInit();
            return bitmapImage;
        }
        public static string FromLevelDiff(string id)
        {
            return "http://" + GDM.Globals.Endpoints.ProxiedGDBrowser + "/assets/difficulties/" + id.Trim().ToLower() + ".png";
        }
        // unused
        public static string PlayerDataFromID(string id)
        {
            return "http://" + GDM.Globals.Endpoints.ProxiedGDBrowser + "/api/profile/" + id.Trim() + "?player=true";
        }


        public static string GetAPIUrl(string form, string col1, string col2, string iconID, string accid, string isGlow, string cubeID)
        {
            return "http://95.111.251.138/gdm/getIcon.php?form=" + form.Trim() + "&col1=" + col1.Trim() + "&col2=" + col2.Trim() + "&icon=" + iconID.Trim() + "&id=" + accid + "&glow=" + isGlow + "&cubeID=" + cubeID;
        }
        public static bool IsFileLocked(string y)
        {
            try
            {
                using (FileStream stream = new FileInfo(y).Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }
        public static string DownloadImageToDir(string url, string dir, string index)
        {
            WebClient client = new WebClient();
            client.Proxy = null;
            try
            {
                // MessageBox.Show("dir: " + dir +" index: " + index);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                dir += "/";
                var temp = dir + "index.tmp";

                bool isGif = false;

                client.DownloadFile(new Uri(url), temp);

                if (!string.IsNullOrEmpty(client.ResponseHeaders["Extension"]))
                {
                    var ext = client.ResponseHeaders["Extension"].Replace(".", "").ToLower();
                    isGif = ext.Contains("gif");

                    string supposedToBeTheFileNameButFFS = dir + index + "." + ext;
                    if (isGif)
                    {
                        supposedToBeTheFileNameButFFS = dir + index + "/" + "image.gif";
                    }
                    if (File.Exists(supposedToBeTheFileNameButFFS) && IsFileLocked(supposedToBeTheFileNameButFFS))
                    {
                        return supposedToBeTheFileNameButFFS;
                    }
                    string directory = Path.GetDirectoryName(supposedToBeTheFileNameButFFS);
                    // make sure dir exists first
                    if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                    if (File.Exists(supposedToBeTheFileNameButFFS)) File.Delete(supposedToBeTheFileNameButFFS);
                    File.Move(temp, supposedToBeTheFileNameButFFS);

                    if (File.Exists(temp)) File.Delete(temp);

                    if (isGif)
                    {
                        // split gif
                        Utilities.GIFHelper.SplitGIFs(supposedToBeTheFileNameButFFS);
                    }
                    return supposedToBeTheFileNameButFFS;
                }
                else
                {
                    // its a png
                    var ext = "png";
                    string supposedToBeTheFileNameButFFS = dir + index + "." + ext;
                    if (File.Exists(supposedToBeTheFileNameButFFS) && IsFileLocked(supposedToBeTheFileNameButFFS))
                    {
                        return supposedToBeTheFileNameButFFS;
                    }
                    string directory = Path.GetDirectoryName(supposedToBeTheFileNameButFFS);
                    if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                    if (File.Exists(supposedToBeTheFileNameButFFS)) File.Delete(supposedToBeTheFileNameButFFS);
                    File.Move(temp, supposedToBeTheFileNameButFFS);
                    return supposedToBeTheFileNameButFFS;
                }
            }
            catch (Exception ex)
            {
                GDM.Globals.Global_Data.HandleException(ex);
            }
            return null;
        }

    }
}
