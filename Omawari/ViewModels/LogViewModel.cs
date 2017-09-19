using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Omawari.Models;
using System.IO;

namespace Omawari.ViewModels
{
    public class LogViewModel : BindableBase
    {
        private List<ScrapingResult> results = null;
        private ScrapingResult result = null;
        private Scraper scraper;

        private RelayCommand reloadCommand;
        private RelayCommand manualCheckCommand;
        private RelayCommand settingsCommand;

        public RelayCommand ManualCheckCommand
        {
            get
            {
                if (manualCheckCommand != null) return manualCheckCommand;

                return manualCheckCommand = new RelayCommand(async () =>
                {
                    await Scraper.CheckAsync();
                    ReloadCommand.Execute(null);
                });
            }
        }

        public RelayCommand ReloadCommand
        {
            get
            {
                if (reloadCommand != null) return reloadCommand;

                return reloadCommand = new RelayCommand(() =>
                {
                    OnPropertyChanged(nameof(Results));
                    OnPropertyChanged(nameof(GroupedResults));
                });
            }
        }

        public RelayCommand SettingsCommand
        {
            get
            {
                if (settingsCommand != null) return settingsCommand;

                return settingsCommand = new RelayCommand(() =>
                {
                    new Views.DetailWindow(Scraper).ShowDialog();
                });
            }
        }

        public string Title
        {
            get { return $"Log: {Scraper.Name}"; }
        }

        public Scraper Scraper
        {
            get { return scraper; }
            set
            {
                SetProperty(ref scraper, value);
                OnPropertyChanged(nameof(Title));
                ReloadCommand.Execute(null);
            }
        }

        public List<ScrapingResult> Results
        {
            get { return scraper.Results; }
            set
            {
                SetProperty(ref results, value);
            }
        }

        public List<ScrapingResult> GroupedResults
        {
            get { return Results.GroupBy(_ => _.Text).Select(_ => _.First()).OrderByDescending(_ => _.CompletedAt).ToList(); }
        }

        public ScrapingResult SelectedResult
        {
            get { return result; }
            set
            {
                SetProperty(ref result, value);
                OnPropertyChanged(nameof(Diff));
            }
        }

        public string Diff
        {
            get
            {
                try
                {
                    var index = GroupedResults.IndexOf(SelectedResult) + 1;
                    return SelectedResult.Diff(GroupedResults[index]);
                }
                catch
                {
                    return "Diff is not available.";
                }
            }
        }
    }
}
