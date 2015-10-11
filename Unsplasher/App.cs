using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using HtmlAgilityPack;
using Unsplasher.Properties;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using System.Text.RegularExpressions;

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

        private System.ComponentModel.BackgroundWorker backgroundWorker;

        static private System.Drawing.Imaging.ImageFormat _imageFormat;

        static private Timer timer;
        

        public App()
        {
            Application.ApplicationExit += OnApplicationExit;
            timer = new Timer { Interval = 1800000 };

            LoadConfigure();
            InitializeComponent();
            _trayIcon.Visible = true;

            
            timer.Tick += _timer_Tick;
            timer.Enabled = true;
            timer.Start();

            _refreshMenuItem.Text = "refreshing";
            _refreshMenuItem.Enabled = false;
            backgroundWorker.RunWorkerAsync();
        }

        private void LoadConfigure()
        {
            if (System.IO.File.Exists("config.json"))
            {
                string json = System.IO.File.ReadAllText("config.json");
                try
                {
                    string[] keyValueArray = json.Replace("{", string.Empty).Replace("}", string.Empty).Replace("\"", string.Empty).Split(',');
                    Dictionary<string, string> conf = keyValueArray.ToDictionary(item => item.Split(':')[0], item => item.Split(':')[1]);
                    timer.Interval = Convert.ToInt32(conf["interval"]);
                    System.Reflection.PropertyInfo propertyinfo = typeof(System.Drawing.Imaging.ImageFormat).GetProperty(conf["type"]);
                    _imageFormat = (System.Drawing.Imaging.ImageFormat)propertyinfo.GetValue(null, null);
                }
                catch
                {
                    ;
                }
            }
            //throw new NotImplementedException();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            _refreshMenuItem.Text = "refreshing";
            _refreshMenuItem.Enabled = false;
            backgroundWorker.RunWorkerAsync();
        }

        private void InitializeComponent()
        {
            backgroundWorker = new System.ComponentModel.BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;

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

        private void backgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            _trayIcon.ShowBalloonTip(3000);
            _refreshMenuItem.Text = "refresh";
            _refreshMenuItem.Enabled = true;
            //throw new NotImplementedException();
        }

        private void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            System.ComponentModel.BackgroundWorker worker = sender as System.ComponentModel.BackgroundWorker;
            SetWallpaper(_imageFormat);
        }

        private void SelectIntervalMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var imageFormatItem in _imageFormatItems)
                imageFormatItem.Checked = false;
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
            _refreshMenuItem.Text = "refreshing";
            _refreshMenuItem.Enabled = false;
            backgroundWorker.RunWorkerAsync();
        }

        private static void SetWallpaper(System.Drawing.Imaging.ImageFormat imageformat)
        {
            List<HtmlNode> imageNodes = GetImageNodes("https://www.bing.com/HPImageArchive.aspx?format=rss&idx=0&n=16&mkt=en-US");
            imageNodes.AddRange(GetImageNodes("https://unsplash.com/rss"));

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
            client.Encoding = System.Text.Encoding.UTF8;
            string html = client.DownloadString(url);

            if (url.Contains("bing"))
            {
                html = html.Replace("1366x768.jpg", "1920x1080.jpg");

                foreach (Match match in Regex.Matches(html, @"<img.+?/?>"))
                {
                    html = html.Replace(match.Value, "<div>" + match.Value + "</div>");
                }

                foreach (Match match in Regex.Matches(html, "src=\"/"))
                {
                    html = html.Replace(match.Value, "src=\"http://www.bing.com/");
                }
            }
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
            SaveConfigure();
            Application.Exit();
        }

        private static void SaveConfigure()
        {
            Dictionary<string, string> configure = new Dictionary<string, string>();
            configure.Add("source", "https://unsplash.com/rss");
            configure.Add("interval", timer.Interval.ToString());
            configure.Add("type", _imageFormat.ToString());
            var kvs = configure.Select(kvp => string.Format("\"{0}\":\"{1}\"", kvp.Key, string.Join(",", kvp.Value)));
            if (System.IO.File.Exists("config.json"))
                System.IO.File.Delete("config.json");
            System.IO.File.AppendAllText("config.json",string.Concat("{", string.Join(",", kvs), "}"));

        }

        private void SelectFormatMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var imageFormatItem in _imageFormatItems)
                imageFormatItem.Checked = false;
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