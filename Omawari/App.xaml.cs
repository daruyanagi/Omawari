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
            using (var Semaphore = new Semaphore(1, 1, APP_ID, out bool created))
            {
                if (!created) return;

                App app = new App();
                app.InitializeComponent();
                app.Run();
            }
        }

#if DEBUG
        private const string APP_ID = "{9ABF2DEC-1CA9-4A76-A2BE-C86AE811DA64}";
        public string Name { get; } = $"{Assembly.GetExecutingAssembly().GetName().Name} (DEBUG)";
#else
        private const string APP_ID = "{B70D60F7-96A6-47EC-B2DA-D47D899A7861}";
        public string Name { get; } = $"{Assembly.GetExecutingAssembly().GetName().Name}";
#endif
        public Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
        public Version Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;

        private Forms.Timer Timer = new Forms.Timer();
        private Forms.NotifyIcon NotifyIcon = new Forms.NotifyIcon();
        private FileSystemWatcher Watcher;

        public event EventHandler<PulsedEventArgs> Pulsed = null;
        public event EventHandler<TimerEnableChangedArgs> TimerEnableChanged = null;
        public event EventHandler<DetectedEventArgs> UpdateDetected = null;
        public event EventHandler<DetectedEventArgs> ErrorDetected = null;

        protected void OnPulsed()
        {
            CheckAll();

            Pulsed?.Invoke(this, new PulsedEventArgs(++WorkingMinutes));
        }

        protected void OnTimerEnableChanged()
        {
            TimerEnableChanged?.Invoke(this, new TimerEnableChangedArgs(Timer.Enabled));
        }

        public void NotifyUpdateDetected(Models.ScrapingRule scraper, Models.ScrapingResult result)
        {
            Instance.ShowInformation($"{scraper.Name} is updated.");

            UpdateDetected?.Invoke(this, new DetectedEventArgs(scraper, result));
        }

        public void NotifyErrorDetected(Models.ScrapingRule scraper, Models.ScrapingResult result)
        {
            Instance.ShowInformation($"{scraper.Name} is failed.");

            ErrorDetected?.Invoke(this, new DetectedEventArgs(scraper, result));
        }

        public static App Instance { get { return App.Current as App; } }

        public Models.ScrapingRuleCollection ScraperCollection { get; private set; }
        public Models.GlobalSettings GlobalSettings { get; set; }
        public int WorkingMinutes { get; set; } = 0;
        public string DataFolder { get; set; }
        public string SettingsPath { get { return Path.Combine(DataFolder, "settings.json"); } }
        public string ScrapersPath { get { return Path.Combine(DataFolder, "scrapers.json"); } }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            // データフォルダーのチェック：Properties.Settings.Default で管理するのはこのデータだけ
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

            // プロパティの初期化
            GlobalSettings = Models.GlobalSettings.Load(SettingsPath);
            ScraperCollection = Models.ScrapingRuleCollection.Load(ScrapersPath);

            // 通知アイコンの初期化
            NotifyIcon.Text = App.Instance.Name;
            NotifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            NotifyIcon.Visible = true;
            NotifyIcon.Click += (s, a) => { if (MainWindow.IsVisible) MainWindow.Hide(); else MainWindow.Show(); };
            NotifyIcon.ContextMenu = new Forms.ContextMenu(new Forms.MenuItem[]
            {
                new Forms.MenuItem("Open", (s, a) => { MainWindow.Show(); }),
                new Forms.MenuItem("Exit", (s, a) => { App.Current.Shutdown(); }),
            });

            // コレクションとタイマーの初期化
            foreach (var scraper in ScraperCollection)
            {
                await scraper.UpdateResultsAsync();
            }

            Timer.Interval = 60 * 1000;
            Timer.Tick += (s, a) => { OnPulsed(); };
            if (GlobalSettings.AutoStart) Start();

            // クラウドストレージなどを介した複数環境での利用を考慮したファイルシステム監視
            Watcher = new FileSystemWatcher();
            Watcher.Path = DataFolder;
            Watcher.NotifyFilter = NotifyFilters.FileName;
            Watcher.IncludeSubdirectories = true;
            Watcher.Filter = "*.json";
            Watcher.Created += async (s, a) =>
            {
                var id = Path.GetDirectoryName(a.FullPath).Split('\\').Last();
                var rule = ScraperCollection.FirstOrDefault(_ => _.Id.ToString() == id);
                if (rule == null) return;
                var index = ScraperCollection.IndexOf(rule);
                await ScraperCollection[index].UpdateResultsAsync();
            };
            Watcher.Deleted += async (s, a) =>
            {
                var id = Path.GetDirectoryName(a.FullPath).Split('\\').Last();
                var rule = ScraperCollection.FirstOrDefault(_ => _.Id.ToString() == id);
                if (rule == null) return;
                var index = ScraperCollection.IndexOf(rule);
                await ScraperCollection[index].UpdateResultsAsync();
            };
            Watcher.EnableRaisingEvents = true;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            NotifyIcon.Visible = false;
            // ScraperCollection.Save(); <-- 読み込みミスって保存したら全部消えてまうやん！
            GlobalSettings.Save();
        }

        public void CheckAll()
        {
            var now = DateTime.UtcNow;

            Parallel.ForEach(Instance.ScraperCollection, async (item) =>
            {
                if (item.Status == Models.ScrapingStatus.Running) return;
                
                if (item.LastResult == null || 
                    item.LastResult.CompletedAt + TimeSpan.FromMinutes(item.Interval) < now)
                {
                    await item.CheckAsync();
                }
                // ToDo: WaitTimeForChangingToPending の設定ユーザーインターフェイスを追加する
                else if (item.LastResult.CompletedAt + TimeSpan.FromMinutes(Instance.GlobalSettings.WaitTimeForChangingToPending) < now)
                {
                    item.Status = Models.ScrapingStatus.Pending;
                }
            });
        }
        
        public void Start()
        {
            Timer.Start();
            OnTimerEnableChanged();
        }

        public void Stop()
        {
            Timer.Stop();
            OnTimerEnableChanged();
        }

        // ToDo：トースト通知にする
        public void ShowInformation(string message, int duration = 5 * 1000)
        {
            NotifyIcon.ShowBalloonTip(duration, Instance.Name, message, Forms.ToolTipIcon.Info);
        }

        public void ShowCaution(string message, int duration = 5 * 1000)
        {
            NotifyIcon.ShowBalloonTip(duration, Instance.Name, message, Forms.ToolTipIcon.Error);
        }

        public void ShowResult(Models.ScrapingResult result)
        {
            var window = App.Current.Windows
                .OfType<Views.ScrapingResultWindow>()
                .FirstOrDefault(_ => _.ViewModel.Result.Id == result.Id);

            if (window != null) { window.Activate(); return; }

            window = new Views.ScrapingResultWindow(result)
            {
                Owner = MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            window.Show();
        }

        public void CreateRule()
        {
            var window = new Views.CreateScrapingRuleWindow()
            {
                Owner = MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            if (window.ShowDialog() == true)
            {
                // ToDo: バリデーション

                var rule = window.ViewModel.Rule;

                if (string.IsNullOrEmpty(rule.Name))
                {
                    rule.Name = $"{rule.Target} @ {rule.Selectors}";
                }

                App.Instance.ScraperCollection.Add(window.ViewModel.Rule);
                App.Instance.ScraperCollection.Save();
            }
        }

        internal void ShowRule(Models.ScrapingRule rule)
        {
            var window = App.Current.Windows
                .OfType<Views.ScrapingRuleWindow>()
                .FirstOrDefault(_ => _.ViewModel.Rule.Id == rule.Id);

            if (window != null) { window.Activate(); return; }

            window = new Views.ScrapingRuleWindow(rule)
            {
                Owner = MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            window.Show();

            try
            {
                App.Instance.ScraperCollection.Save();
            }
            catch
            {
                // アプリ終了時にクラッシュするケースを握りつぶす（v1.4 追加
            }
        }

        internal void DeleteRule(Models.ScrapingRule rule)
        {
            var window = new Views.DeleteScrapingRuleWindow(rule)
            {
                Owner = MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            if (window.ShowDialog() == true)
            {
                if (window.ViewModel.IsDeleteData)
                {
                    Directory.Delete(rule.Location, true);
                }

                App.Instance.ScraperCollection.Remove(rule);
                App.Instance.ScraperCollection.Save();
            }
        }

        public void TestRule(Models.ScrapingRule rule)
        {
            var window = new Views.ScrapingTestWindow(rule)
            {
                Owner = MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            window.Loaded += async (s, a) => await window.ViewModel.TestAsync();
            window.ShowDialog();
        }

        internal void ShowSettings()
        {
            var window = new Views.SettingsWindow()
            {
                Owner = MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            window.ShowDialog();
        }

        public class PulsedEventArgs
        {
            public PulsedEventArgs(int workingMinutes)
            {
                WorkingMinutes = workingMinutes;
            }

            public int WorkingMinutes { get; set; }
        }

        public class TimerEnableChangedArgs
        {
            public TimerEnableChangedArgs(bool timerEnabled)
            {
                TimerEnabled = timerEnabled;
            }

            public bool TimerEnabled { get; set; }
        }

        public class DetectedEventArgs
        {
            public DetectedEventArgs(Models.ScrapingRule scraper, Models.ScrapingResult result)
            {
                Scraper = scraper;
                Result = result;
            }

            Models.ScrapingRule Scraper { get; set; }
            Models.ScrapingResult Result { get; set; }
        }
    }
}
