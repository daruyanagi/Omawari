using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omawari.ViewModels
{
    using Omawari.Models;
    using Omawari.Views;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Reflection;
    using System.Windows;

    public class MainViewModel : BindableBase
    {
        public MainViewModel()
        {
            var app = App.Current as App;

            app.TimerEnableChanged += (s, a) =>
            {
                TimerEnabled = a.TimerEnabled;
                StartCommand.RaiseCanExecuteChanged();
                StopCommand.RaiseCanExecuteChanged();
                TimerStatusMessage = "Web Chacker: " + (TimerEnabled ? "Enabled" : "Disabled");
            };

            app.Pulsed += (s, a) =>
            {
                WorkingTimeMessage = a.WorkingMinutes != 1
                    ? $"Working: {a.WorkingMinutes} times (Last pulse: {DateTime.Now})"
                    : $"Working: {a.WorkingMinutes} time (Last pulse: {DateTime.Now})";
            };
        }

        private RelayCommand addScraperCommand = null;
        private RelayCommand editScraperCommand = null;
        private RelayCommand removeScraperCommand = null;
        private RelayCommand logSelectedScraperCommand = null;
        private RelayCommand checkSelectedScraperCommand;
        private RelayCommand startCommand = null;
        private RelayCommand stopCommand = null;
        private RelayCommand exitCommand = null;
        private RelayCommand checkAllScraperCommand = null;
        private RelayCommand settingsCommand = null;
        private RelayCommand helpCommand = null;

        private ScrapingRuleCollection items = App.Instance.ScraperCollection;
        private ObservableCollection<Models.ScrapingResult> updateLog = App.Instance.UpdateLog;
        private ScrapingRule selectedItem = null;
        private string workingTimeMessage = null;
        private string timerStatusMessage = null;
        private bool timerEnabled = false;

        public RelayCommand StartCommand
        {
            get
            {
                if (startCommand != null) return startCommand;

                return startCommand = new RelayCommand(
                    () => App.Instance.Start(),
                    () => !TimerEnabled
                );
            }
        }

        public RelayCommand StopCommand
        {
            get
            {
                if (stopCommand != null) return stopCommand;

                return stopCommand = new RelayCommand(
                    () => App.Instance.Stop(),
                    () => TimerEnabled
                );
            }
        }

        public RelayCommand ExitCommand
        {
            get
            {
                if (exitCommand != null) return exitCommand;

                return exitCommand = new RelayCommand(
                    () => App.Current.Shutdown()
                );
            }
        }

        public RelayCommand HelpCommand
        {
            get
            {
                if (helpCommand != null) return helpCommand;

                return helpCommand = new RelayCommand(
                    () => System.Diagnostics.Process.Start("http://blog.daruyanagi.jp/archive/category/Omawari")
                );
            }
        }

        public RelayCommand AddScraperCommand
        {
            get
            {
                if (addScraperCommand != null) return addScraperCommand;

                return addScraperCommand = new RelayCommand(
                    () => App.Instance.CreateRule()
                );
            }
        }

        public RelayCommand EditScraperCommand
        {
            get
            {
                if (editScraperCommand != null) return editScraperCommand;

                return editScraperCommand = new RelayCommand(
                    () => App.Instance.ShowRule(SelectedItem),
                    () => SelectedItem != null
                );
            }
        }

        public RelayCommand RemoveScraperCommand
        {
            get
            {
                if (removeScraperCommand != null) return removeScraperCommand;

                return removeScraperCommand = new RelayCommand(
                    () => App.Instance.DeleteRule(SelectedItem),
                    () => SelectedItem != null
                );
            }
        }

        public RelayCommand CheckSelectedScraperCommand
        {
            get
            {
                if (checkSelectedScraperCommand != null) return checkSelectedScraperCommand;

                return checkSelectedScraperCommand = new RelayCommand(
                    async () =>
                    {
                        if (SelectedItem == null) return;
                        await SelectedItem.CheckAsync();
                    }, 
                    () => SelectedItem != null
                );
            }
        }

        public RelayCommand CheckAllScraperCommand
        {
            get
            {
                if (checkAllScraperCommand != null) return checkAllScraperCommand;

                return checkAllScraperCommand = new RelayCommand(
                    () => Parallel.ForEach(Items, async (item) => await item.CheckAsync())
                );
            }
        }

        public RelayCommand LogSelectedScraperCommand
        {
            get
            {
                if (logSelectedScraperCommand != null) return logSelectedScraperCommand;

                return logSelectedScraperCommand = new RelayCommand(
                    () => new LogWindow(SelectedItem).ShowDialog(),
                    () => SelectedItem != null
                );
            }
        }

        public RelayCommand SettingsCommand
        {
            get
            {
                if (settingsCommand != null) return settingsCommand;

                return settingsCommand = new RelayCommand(
                    () => new SettingsWindow().ShowDialog()
                );
            }
        }

        public string Title
        {
            get { return $"{App.Instance.Name} v{App.Instance.Version}"; }
        }

        public ScrapingRuleCollection Items
        {
            get { return items; }
            set { SetProperty(ref items, value); }
        }

        public ObservableCollection<Models.ScrapingResult> UpdateLog
        {
            get { return updateLog; }
            set { SetProperty(ref updateLog, value); }
        }

        public ScrapingRule SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (!SetProperty(ref selectedItem, value)) return;

                EditScraperCommand.RaiseCanExecuteChanged();
                RemoveScraperCommand.RaiseCanExecuteChanged();
                CheckSelectedScraperCommand.RaiseCanExecuteChanged();
                LogSelectedScraperCommand.RaiseCanExecuteChanged();
            }
        }

        public string TimerStatusMessage
        {
            get { return timerStatusMessage; }
            set { SetProperty(ref timerStatusMessage, value); }
        }

        public string WorkingTimeMessage
        {
            get { return workingTimeMessage; }
            set { SetProperty(ref workingTimeMessage, value); }
        }

        public bool TimerEnabled
        {
            get { return timerEnabled; }
            set { SetProperty(ref timerEnabled, value); }
        }
    }
}
