using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Point = System.Drawing.Point;

namespace Multiplayer.Utilities
{
    public static class GIFHelper
    {

        public static void SplitGIFs(string gifLocation)
        {
            Bitmap bm = new Bitmap(gifLocation);
            var dir = Path.GetDirectoryName(gifLocation) + "/";
            var frames = ParseFrames(bm);

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);


            int u = 0;
            foreach (var j in frames)
            {
                try
                {
                    j.Save(dir + u.ToString() + ".png", ImageFormat.Png);
                }
                catch { }
                u++;
            }

            int index = 0;

            for (int m = -1; m < (frames.Length - 1); m++)
            {
                int delay = BitConverter.ToInt32(bm.GetPropertyItem(20736).Value, index) * 10;

                int fpsFromDelay = delay / GDM.Globals.Global_Data.GIFDelay;

                string saveFile = dir + (m + 1).ToString() + ".png.delay";
                if (!File.Exists(saveFile)) File.Create(saveFile).Close();

                File.WriteAllText(saveFile, fpsFromDelay.ToString());

                index += 4;
            }

        }

        public static Bitmap[] ParseFrames(Bitmap Animation)
        {
            // Get the number of animation frames to copy into a Bitmap array

            int Length = Animation.GetFrameCount(FrameDimension.Time);
            // Allocate a Bitmap array to hold individual frames from the animation

            Bitmap[] Frames = new Bitmap[Length];

            // Copy the animation Bitmap frames into the Bitmap array

            for (int Index = 0; Index < Length; Index++)
            {
                // Set the current frame within the animation to be copied into the Bitmap array element

                Animation.SelectActiveFrame(FrameDimension.Time, Index);

                // Create a new Bitmap element within the Bitmap array in which to copy the next frame

                Frames[Index] = new Bitmap(Animation.Size.Width, Animation.Size.Height);

                // Copy the current animation frame into the new Bitmap array element

                Graphics.FromImage(Frames[Index]).DrawImage(Animation, new Point(0, 0));
            }

            // Return the array of Bitmap frames

            return Frames;
        }
    }
}
