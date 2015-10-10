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
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _trayIconContextMenu;

        public App()
        {
            Application.ApplicationExit += OnApplicationExit;
            InitializeComponent();
            _trayIcon.Visible = true;

            var timer = new Timer {Interval = 900000};
            timer.Tick += _timer_Tick;
            timer.Enabled = true;
            timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            SetWallpaper();
        }

        private void InitializeComponent()
        {
            _trayIcon = new NotifyIcon
            {
                BalloonTipIcon = ToolTipIcon.Info,
                BalloonTipText = "Is there a new background?",
                BalloonTipTitle = "Unsplasher",
                Icon = Resources.TrayIcon
            };

            _trayIconContextMenu = new ContextMenuStrip();
            _closeMenuItem = new ToolStripMenuItem();
            _refreshMenuItem = new ToolStripMenuItem();
            _trayIconContextMenu.SuspendLayout();

            _trayIconContextMenu.Items.AddRange(new ToolStripItem[]
            {
                _refreshMenuItem
            });
            _trayIconContextMenu.Items.AddRange(new ToolStripItem[]
            {
                _closeMenuItem
            });
            _trayIconContextMenu.Name = "_trayIconContextMenu";
            _trayIconContextMenu.Size = new Size(161, 100);

            _closeMenuItem.Name = "_closeMenuItem";
            _closeMenuItem.Size = new Size(161, 50);
            _closeMenuItem.Text = "Exit";
            _closeMenuItem.Click += CloseMenuItem_Click;

            _refreshMenuItem.Name = "Refresh";
            _refreshMenuItem.Size = new Size(161, 50);
            _refreshMenuItem.Text = "Refresh";
            _refreshMenuItem.Click += RefreshMenuItemClick;

            _trayIconContextMenu.ResumeLayout(false);
            _trayIcon.ContextMenuStrip = _trayIconContextMenu;
        }

        private void RefreshMenuItemClick(object sender, EventArgs e)
        {
            SetWallpaper();
            _trayIcon.ShowBalloonTip(3000);
        }

        private static void SetWallpaper()
        {
            List<HtmlNode> imageNodes = GetImageNodes("https://unsplash.com/rss");

            int rand = new Random().Next(imageNodes.Count);
            Wallpaper.Set(new Uri(imageNodes[rand].Attributes["src"].Value), Wallpaper.Style.Stretched);
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
    }
}