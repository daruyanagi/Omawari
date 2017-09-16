using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Omawari
{
    using Forms = System.Windows.Forms;

    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        public static void Main()
        {
            const string id = "{B70D60F7-96A6-47EC-B2DA-D47D899A7861}";

            using (var Semaphore = new Semaphore(1, 1, id, out bool created))
            {
                if (!created) return;

                App app = new App();
                app.InitializeComponent();
                app.Run();
            }
        }

        public static readonly Assembly Assembly = System.Reflection.Assembly.GetExecutingAssembly();
        public static readonly string Name = Assembly.GetName().Name;
        public static readonly Version Version = Assembly.GetName().Version;

        private static Forms.Timer Timer = new Forms.Timer();
        private static Forms.NotifyIcon NotifyIcon = new Forms.NotifyIcon();
        
        public static Models.ScraperCollection ScraperCollection { get; private set; }
        public static Models.GlobalSettings GlobalSettings { get; set; }
        public static string DataFolder { get; set; }
        public static string SettingsPath { get { return Path.Combine(DataFolder, "settings.json"); } }
        public static string ScrapersPath { get { return Path.Combine(DataFolder, "scrapers.json"); } }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // データフォルダーのチェック
            DataFolder = Omawari.Properties.Settings.Default.DataFolder;
            if (!Directory.Exists(DataFolder))
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                switch (dialog.ShowDialog())
                {
                    case Forms.DialogResult.OK:
                        DataFolder = dialog.SelectedPath;
                        Omawari.Properties.Settings.Default.DataFolder = DataFolder;
                        Omawari.Properties.Settings.Default.Save();
                        break;
                    default:
                        Shutdown();
                        break;
                }
            }

            GlobalSettings = Models.GlobalSettings.Load(SettingsPath);
            ScraperCollection = Models.ScraperCollection.Load(ScrapersPath);

            // 通知アイコンの用意
            NotifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(App.Assembly.Location);
            NotifyIcon.Visible = true;
            NotifyIcon.Click += (s, a) => { if (MainWindow.IsVisible) MainWindow.Hide(); else MainWindow.Show(); };
            NotifyIcon.ContextMenu = new Forms.ContextMenu(new Forms.MenuItem[]
            {
                new Forms.MenuItem("Open", (s, a) => { MainWindow.Show(); }),
                new Forms.MenuItem("Exit", (s, a) => { App.Current.Shutdown(); }),
            });

            Timer.Interval = 60 * 1000;
            Timer.Tick += (s, a) => { CheckAll(); };
            if (GlobalSettings.AutoStart) Start();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            NotifyIcon.Visible = false;
            ScraperCollection.Save();
            GlobalSettings.Save();
        }

        public static void CheckAll()
        {
            var now = DateTime.UtcNow;

            Parallel.ForEach(ScraperCollection, async (item) =>
            {
                if (item.Status == Models.ScrapingStatus.Running) return;
                
                if (item.LastResult == null || 
                    item.LastResult.CompletedAt + TimeSpan.FromMinutes(item.Interval) < now)
                {
                    await item.CheckAsync();
                }
                if (item.LastResult.CompletedAt + TimeSpan.FromMinutes(10) < now) // 10分経ったらペンディングにしとくかな
                {
                    item.Status = Models.ScrapingStatus.Pending;
                }
            });
        }

        public static void Start()
        {
            Timer.Start();
        }

        public static void Stop()
        {
            Timer.Stop();
        }

        public static void StartOrStop()
        {
            if (Timer.Enabled) Timer.Stop(); else Timer.Start();
        }

        public static bool GetTimerIsEnabled()
        {
            return Timer.Enabled;
        }

        public static void ShowInformation(string message, int duration = 5 * 1000)
        {
            NotifyIcon.ShowBalloonTip(duration, App.Name, message, Forms.ToolTipIcon.Info);
        }

        public static void ShowCaution(string message, int duration = 5 * 1000)
        {
            NotifyIcon.ShowBalloonTip(duration, App.Name, message, Forms.ToolTipIcon.Error);
        }
    }
}
