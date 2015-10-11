using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Unsplasher
{
    public static class Wallpaper
    {
        public enum Style
        {
            Tiled,
            Centered,
            Stretched
        }

        private const int SpiSetdeskwallpaper = 20;
        private const int SpifUpdateinifile = 0x01;
        private const int SpifSendwininichange = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public static void Set(Uri uri, Style style, ImageFormat format)
        {
            Stream stream = new WebClient().OpenRead(uri.ToString());

            if (stream == null) return;

            Image img = Image.FromStream(stream);
            string tempPath = Path.Combine(Path.GetTempPath(), "img." + format.ToString());
            img.Save(tempPath, format);

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            if (style == Style.Stretched)
            {
                key.SetValue(@"WallpaperStyle", 2.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Centered)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Tiled)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());
            }

            SystemParametersInfo(SpiSetdeskwallpaper,
                0,
                tempPath,
                SpifUpdateinifile | SpifSendwininichange);
        }
    }
}