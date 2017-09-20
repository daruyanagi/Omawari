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
        private ScrapingResult result = null;
        private Scraper scraper;
        
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
            }
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
                    var index = Scraper.UpdateResults.IndexOf(SelectedResult) + 1;
                    return SelectedResult.Diff(Scraper.UpdateResults[index]);
                }
                catch
                {
                    return "Diff is not available.";
                }
            }
        }
    }
}
