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

        private ScraperCollection items = App.ScraperCollection;
        private ObservableCollection<Models.ScrapingResult> updateLog = App.UpdateLog;
        private Scraper selectedItem = null;

        private string timerCommandLabel = "Start";
        private RelayCommand helpCommand;

        public RelayCommand StartCommand
        {
            get
            {
                if (startCommand != null) return startCommand;

                return startCommand = new RelayCommand(() =>
                {
                    App.Start();
                    StartCommand.RaiseCanExecuteChanged();
                    StopCommand.RaiseCanExecuteChanged();
                    OnPropertyChanged(nameof(StatusBarMessage));
                }, () => !App.GetTimerIsEnabled());
            }
        }

        public RelayCommand StopCommand
        {
            get
            {
                if (stopCommand != null) return stopCommand;

                return stopCommand = new RelayCommand(() =>
                {
                    App.Stop();
                    StartCommand.RaiseCanExecuteChanged();
                    StopCommand.RaiseCanExecuteChanged();
                    OnPropertyChanged(nameof(StatusBarMessage));
                }, () => App.GetTimerIsEnabled());
            }
        }

        public RelayCommand ExitCommand
        {
            get
            {
                if (exitCommand != null) return exitCommand;

                return exitCommand = new RelayCommand(() =>
                {
                    App.Current.Shutdown();
                });
            }
        }

        public RelayCommand HelpCommand
        {
            get
            {
                if (helpCommand != null) return helpCommand;

                return helpCommand = new RelayCommand(() =>
                {
                    System.Diagnostics.Process.Start("http://daruyanagi.jp/");
                });
            }
        }

        public RelayCommand AddScraperCommand
        {
            get
            {
                if (addScraperCommand != null) return addScraperCommand;

                return addScraperCommand = new RelayCommand(() =>
                {
                    new DetailWindow(new Models.Scraper()).ShowDialog();
                });
            }
        }

        public RelayCommand EditScraperCommand
        {
            get
            {
                if (editScraperCommand != null) return editScraperCommand;

                return editScraperCommand = new RelayCommand(() =>
                {
                    if (SelectedItem == null) return;

                    new DetailWindow(SelectedItem.Clone()).ShowDialog();

                    SelectedItem = null;
                }, () => SelectedItem != null);
            }
        }

        public RelayCommand RemoveScraperCommand
        {
            get
            {
                if (removeScraperCommand != null) return removeScraperCommand;

                return removeScraperCommand = new RelayCommand(() =>
                {
                    if (SelectedItem == null) return;

                    Items.Remove(SelectedItem);
                    Items.Save();

                    SelectedItem = null;
                }, () => SelectedItem != null);
            }
        }

        public RelayCommand CheckSelectedScraperCommand
        {
            get
            {
                if (checkSelectedScraperCommand != null) return checkSelectedScraperCommand;

                return checkSelectedScraperCommand = new RelayCommand(async () =>
                {
                    if (SelectedItem == null) return;
                    await SelectedItem.CheckAsync();
                }, () => SelectedItem != null);
            }
        }

        public RelayCommand CheckAllScraperCommand
        {
            get
            {
                if (checkAllScraperCommand != null) return checkAllScraperCommand;

                return checkAllScraperCommand = new RelayCommand(() =>
                {
                    Parallel.ForEach(Items, async (item) => { await item.CheckAsync(); });
                });
            }
        }

        public RelayCommand LogSelectedScraperCommand
        {
            get
            {
                if (logSelectedScraperCommand != null) return logSelectedScraperCommand;

                return logSelectedScraperCommand = new RelayCommand(() =>
                {
                    if (SelectedItem == null) return;

                    new LogWindow(SelectedItem).ShowDialog();
                }, () => SelectedItem != null);
            }
        }

        public RelayCommand SettingsCommand
        {
            get
            {
                if (settingsCommand != null) return settingsCommand;

                return settingsCommand = new RelayCommand(() =>
                {
                    new SettingsWindow().ShowDialog();
                });
            }
        }

        public string Title
        {
            get
            {
#if DEBUG
                return $"{App.Name} v{App.Version} (DEBUG)";
#else
                return $"{App.Name} v{App.Version}";
#endif
            }
        }

        public ScraperCollection Items
        {
            get { return items; }
            set { SetProperty(ref items, value); }
        }

        public ObservableCollection<Models.ScrapingResult> UpdateLog
        {
            get { return updateLog; }
            set { SetProperty(ref updateLog, value); }
        }

        public Scraper SelectedItem
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

        public string TimerCommandLabel
        {
            get { return timerCommandLabel; }
            set { SetProperty(ref timerCommandLabel, value); }
        }

        public string StatusBarMessage
        {
            get { return $"Timer: {App.GetTimerIsEnabled()}"; }
        }
    }
}
