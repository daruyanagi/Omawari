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
#if DEBUG
            const string id = "{9ABF2DEC-1CA9-4A76-A2BE-C86AE811DA64}";
#else
            const string id = "{B70D60F7-96A6-47EC-B2DA-D47D899A7861}";
#endif
            using (var Semaphore = new Semaphore(1, 1, id, out bool created))
            {
                if (!created) return;

                App app = new App();
                app.InitializeComponent();
                app.Run();
            }
        }

        public static readonly Assembly Assembly = System.Reflection.Assembly.GetExecutingAssembly();
#if DEBUG
        public static readonly string Name = $"{Assembly.GetName().Name} (DEBUG)";
#else
        public static readonly string Name = Assembly.GetName().Name;
#endif
        public static readonly Version Version = Assembly.GetName().Version;

        private static Forms.Timer Timer = new Forms.Timer();
        private static Forms.NotifyIcon NotifyIcon = new Forms.NotifyIcon();

        public static Models.ScraperCollection ScraperCollection { get; private set; }
        public static AsyncObservableCollection<Models.ScrapingResult> UpdateLog { get; private set; }
        public static Models.GlobalSettings GlobalSettings { get; set; }
        public static int WorkingMinutes { get; set; } = 0;
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
            UpdateLog = new AsyncObservableCollection<Models.ScrapingResult>();

            // 通知アイコンの用意
            NotifyIcon.Text = App.Name;
            NotifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(App.Assembly.Location);
            NotifyIcon.Visible = true;
            NotifyIcon.Click += (s, a) => { if (MainWindow.IsVisible) MainWindow.Hide(); else MainWindow.Show(); };
            NotifyIcon.ContextMenu = new Forms.ContextMenu(new Forms.MenuItem[]
            {
                new Forms.MenuItem("Open", (s, a) => { MainWindow.Show(); }),
                new Forms.MenuItem("Exit", (s, a) => { App.Current.Shutdown(); }),
            });

            Timer.Interval = 60 * 1000;
            Timer.Tick += (s, a) => { CheckAll(); WorkingMinutes++; };
            if (GlobalSettings.AutoStart) Start();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            NotifyIcon.Visible = false;
            ScraperCollection.Save();
            GlobalSettings.Save();
        }

        public static void Updated(Models.Scraper scraper, Models.ScrapingResult result)
        {
            ShowInformation($"{scraper.Name} is updated.");

            UpdateLog.Insert(0, result);
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
                if (item.LastResult.CompletedAt + TimeSpan.FromMinutes(GlobalSettings.WaitTimeForChangingToPending) < now) // 10分経ったらペンディングにしとくかな
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
