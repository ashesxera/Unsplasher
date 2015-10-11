using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using HtmlAgilityPack;
using Unsplasher.Properties;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace Unsplasher
{
    internal class App : ApplicationContext
    {
        private ToolStripMenuItem _closeMenuItem;
        private ToolStripMenuItem _refreshMenuItem;
        private ToolStripMenuItem _jpegMenuItem;
        private ToolStripMenuItem _bmpMenuItem;

        private ToolStripMenuItem _30minIntervalMenuItem;
        private ToolStripMenuItem _60minIntervalMenuItem;
        private ToolStripMenuItem _4hourIntervalMenuItem;
        private ToolStripMenuItem _2hourIntervalMenuItem;
        private ToolStripMenuItem _startUpIntervalMenuItem;

        private NotifyIcon _trayIcon;
        private ContextMenuStrip _trayIconContextMenu;

        private ToolStripMenuItem[] _imageFormatItems;
        private ToolStripMenuItem[] _intervalItems;

        private System.Drawing.Imaging.ImageFormat _imageFormat;

        private Timer timer;
        

        public App()
        {
            Application.ApplicationExit += OnApplicationExit;
            InitializeComponent();
            _trayIcon.Visible = true;

            timer = new Timer { Interval = 1800000 };
            timer.Tick += _timer_Tick;
            timer.Enabled = true;
            timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            SetWallpaper(_imageFormat);
        }

        private void InitializeComponent()
        {
            _trayIcon = new NotifyIcon
            {
                BalloonTipIcon = ToolTipIcon.Info,
                BalloonTipText = "Background refreshed",
                BalloonTipTitle = "Unsplasher",
                Icon = Resources.TrayIconWhite
            };

            _trayIconContextMenu = new ContextMenuStrip();
            _closeMenuItem = new ToolStripMenuItem();
            _refreshMenuItem = new ToolStripMenuItem();
            _jpegMenuItem = new ToolStripMenuItem();
            _bmpMenuItem = new ToolStripMenuItem();
            _30minIntervalMenuItem = new ToolStripMenuItem();
            _60minIntervalMenuItem = new ToolStripMenuItem();
            _4hourIntervalMenuItem = new ToolStripMenuItem();
            _2hourIntervalMenuItem = new ToolStripMenuItem();
            _startUpIntervalMenuItem = new ToolStripMenuItem();
            _trayIconContextMenu.SuspendLayout();

            _imageFormatItems = new ToolStripMenuItem[]
            {
                _bmpMenuItem,
                _jpegMenuItem

            };

            _intervalItems = new ToolStripMenuItem[]
            {
                _30minIntervalMenuItem,
                _60minIntervalMenuItem,
                _2hourIntervalMenuItem,
                _4hourIntervalMenuItem,
                _startUpIntervalMenuItem
            };

            _imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;

            _trayIconContextMenu.Items.AddRange(_imageFormatItems);
            _trayIconContextMenu.Items.Add(new ToolStripSeparator());
            _trayIconContextMenu.Items.AddRange(_intervalItems);
            _trayIconContextMenu.Items.Add(new ToolStripSeparator());
            _trayIconContextMenu.Items.Add(_refreshMenuItem);
            _trayIconContextMenu.Items.Add(_closeMenuItem);


            _trayIconContextMenu.Name = "_trayIconContextMenu";
            //_trayIconContextMenu.Size = new Size(161, 200);

            _closeMenuItem.Name = "_closeMenuItem";
            _closeMenuItem.Size = new Size(161, 50);
            _closeMenuItem.Text = "Exit";
            _closeMenuItem.Click += CloseMenuItem_Click;

            _refreshMenuItem.Name = "Refresh";
            _refreshMenuItem.Size = new Size(161, 50);
            _refreshMenuItem.Text = "Refresh";
            _refreshMenuItem.Click += RefreshMenuItemClick;

            _bmpMenuItem.Name = "_bmpMenuItem";
            _bmpMenuItem.Size = new Size(161, 50);
            _bmpMenuItem.Text = "Bmp";
            _bmpMenuItem.Click += SelectFormatMenuItem_Click;

            _jpegMenuItem.Name = "_jpegMenuItem";
            _jpegMenuItem.Size = new Size(161, 50);
            _jpegMenuItem.Text = "Jpeg";
            _jpegMenuItem.Checked = true;
            _jpegMenuItem.Click += SelectFormatMenuItem_Click;

            _30minIntervalMenuItem.Name = "_30minIntervalMenuItem";
            _30minIntervalMenuItem.Size = new Size(161, 50);
            _30minIntervalMenuItem.Text = "30 Min";
            _30minIntervalMenuItem.Checked = true;
            _30minIntervalMenuItem.Click += SelectIntervalMenuItem_Click;

            _60minIntervalMenuItem.Name = "_60minIntervalMenuItem";
            _60minIntervalMenuItem.Size = new Size(161, 50);
            _60minIntervalMenuItem.Text = "60 Min";
            _60minIntervalMenuItem.Click += SelectIntervalMenuItem_Click;

            _2hourIntervalMenuItem.Name = "_2hourIntervalMenuItem";
            _2hourIntervalMenuItem.Size = new Size(161, 50);
            _2hourIntervalMenuItem.Text = "2 Hour";
            _2hourIntervalMenuItem.Click += SelectIntervalMenuItem_Click;

            _4hourIntervalMenuItem.Name = "_4hourIntervalMenuItem";
            _4hourIntervalMenuItem.Size = new Size(161, 50);
            _4hourIntervalMenuItem.Text = "4 Hour";
            _4hourIntervalMenuItem.Click += SelectIntervalMenuItem_Click;

            _startUpIntervalMenuItem.Name = "_startUpIntervalMenuItem";
            _startUpIntervalMenuItem.Size = new Size(161, 50);
            _startUpIntervalMenuItem.Text = "Login";
            _startUpIntervalMenuItem.Click += SelectIntervalMenuItem_Click;

            _trayIconContextMenu.ResumeLayout(false);
            _trayIcon.ContextMenuStrip = _trayIconContextMenu;
        }

        private void SelectIntervalMenuItem_Click(object sender, EventArgs e)
        {
            _intervalItems.All(_intervalItem => _intervalItem.Checked = false);
            ((ToolStripMenuItem)sender).Checked = true;
            switch (sender.ToString())
            {
                case "30 Min":
                    timer.Stop();
                    timer.Interval = 30 * 60 * 1000;
                    timer.Start();
                    break;
                case "60 Min":
                    timer.Stop();
                    timer.Interval = 60 * 60 * 1000;
                    timer.Start();
                    break;
                case "4 Hour":
                    timer.Stop();
                    timer.Interval = 4 * 60 * 60 * 1000;
                    timer.Start();
                    break;
                case "2 Hour":
                    timer.Stop();
                    timer.Interval = 2 * 60 * 60 * 1000;
                    timer.Start();
                    break;
                case "Login":
                default:
                    timer.Stop();
                    break;
            }
            //throw new NotImplementedException();
        }

        private void RefreshMenuItemClick(object sender, EventArgs e)
        {
            SetWallpaper(_imageFormat);
            _trayIcon.ShowBalloonTip(3000);
        }

        private static void SetWallpaper(System.Drawing.Imaging.ImageFormat imageformat)
        {
            List<HtmlNode> imageNodes = GetImageNodes("https://unsplash.com/rss");

            int rand = new Random().Next(imageNodes.Count);
            Wallpaper.Set(new Uri(imageNodes[rand].Attributes["src"].Value), Wallpaper.Style.Stretched, imageformat);
        }

        private static void SetWallpaper()
        {
            SetWallpaper(System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        private static List<HtmlNode> GetImageNodes(string url)
        {
            var client = new WebClient();
            string html = client.DownloadString(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            List<HtmlNode> imageNodes = (from HtmlNode node in doc.DocumentNode.SelectNodes("//img")
                select node).ToList();

            return imageNodes;
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            _trayIcon.Dispose();
        }

        private static void CloseMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void SelectFormatMenuItem_Click(object sender, EventArgs e)
        {
            _imageFormatItems.All(_imageFormatItem => _imageFormatItem.Checked = false);
            ((ToolStripMenuItem)sender).Checked = true;
            System.Reflection.PropertyInfo propertyinfo = typeof(System.Drawing.Imaging.ImageFormat).GetProperty(sender.ToString());
            _imageFormat = (System.Drawing.Imaging.ImageFormat)propertyinfo.GetValue(null, null);
            //switch (sender.ToString())
            //{
            //    case "BMP":
            //        _bmpMenuItem.Checked = true;
            //        _jpegMenuItem.Checked = false;
            //        _imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
            //        break;
            //    case "JPEG":
            //    default:
            //        _jpegMenuItem.Checked = true;
            //        _bmpMenuItem.Checked = false;
            //        _imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
            //        break;

            //}
        }
    }
}