using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Utilities
{
    public static class Utils
    {
        public static string FindTextBetween(string text, string left, string right)
        {
            // TODO: Validate input arguments

            int beginIndex = text.IndexOf(left); // find occurence of left delimiter
            if (beginIndex == -1)
                return string.Empty; // or throw exception?

            beginIndex += left.Length;

            int endIndex = text.IndexOf(right, beginIndex); // find occurence of right delimiter
            if (endIndex == -1)
                return string.Empty; // or throw exception?

            return text.Substring(beginIndex, endIndex - beginIndex).Trim();
        }
        
        public static void DeleteFilesOfExtension(string folder, string ext)
        {

            DirectoryInfo di = new DirectoryInfo(folder);
            FileInfo[] files = di.GetFiles("*." + ext)
                                 .Where(p => p.Extension == "." + ext).ToArray();
            foreach (FileInfo file in files)
                try
                {
                    file.Attributes = FileAttributes.Normal;
                    File.Delete(file.FullName);
                }
                catch { }
        }
        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                try
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                catch (Exception ex){
                    Utils.HandleException(ex);
                }
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }
        public static void HandleException(Exception ex, string handled = "Handled", bool IsNoise = false)
        {
            // send to server
            try
            {
                if (!File.Exists("Error Logs.txt"))
                {
                    File.Create("Error Logs.txt").Close();
                    File.AppendAllText("Error Logs.txt", "WPF throws exception everytime everything happens." +
                        Environment.NewLine +
                        Environment.NewLine);
                }

                System.IO.File.AppendAllText("Error Logs.txt", handled + " ----------" + Environment.NewLine + DateTime.UtcNow.ToString() +
                    ex.ToString() +
                    Environment.NewLine +
                    Environment.NewLine);
            }
            catch { }

            if (!IsNoise)
                Debug.WriteLine(ex.ToString());
        }

    }
}
