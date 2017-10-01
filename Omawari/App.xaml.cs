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
    using Omawari.Models;
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

        public event EventHandler<PulsedEventArgs> Pulsed = null;

        public event EventHandler<TimerEnableChangedArgs> TimerEnableChanged = null;
        public event EventHandler<UpdateDetectedChangedArgs> UpdateDetected = null;

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
            Instance.UpdateLog.Insert(0, result);
            Instance.ShowInformation($"{scraper.Name} is updated.");

            UpdateDetected?.Invoke(this, new UpdateDetectedChangedArgs(scraper, result));
        }

        public static App Instance { get { return App.Current as App; } }

        public Models.ScrapingRuleCollection ScraperCollection { get; private set; }
        public AsyncObservableCollection<Models.ScrapingResult> UpdateLog { get; private set; }
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
            UpdateLog = new AsyncObservableCollection<Models.ScrapingResult>();
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
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            NotifyIcon.Visible = false;
            ScraperCollection.Save();
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
            var window = new Views.ScrapingResultWindow();
            window.ViewModel.Result = result;
            window.ShowDialog();
        }

        public void CreateRule()
        {
            var window = new Views.CreateScrapingRuleWindow();

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

        internal void ShowRule(ScrapingRule rule)
        {
            var window = new Views.ScrapingRuleWindow();
            window.ViewModel.Rule = rule;
            window.ShowDialog();
            App.Instance.ScraperCollection.Save();
        }

        internal void DeleteRule(ScrapingRule rule)
        {
            var window = new Views.DeleteScrapingRuleWindow();
            window.ViewModel.Rule = rule;
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
            var window = new Views.ScrapingTestWindow();
            window.ViewModel.Rule = rule;
            window.Loaded += async (s, a) => await window.ViewModel.TestAsync();
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

        public class UpdateDetectedChangedArgs
        {
            public UpdateDetectedChangedArgs(Models.ScrapingRule scraper, Models.ScrapingResult result)
            {
                Scraper = scraper;
                Result = result;
            }

            Models.ScrapingRule Scraper { get; set; }
            Models.ScrapingResult Result { get; set; }
        }
    }
}
