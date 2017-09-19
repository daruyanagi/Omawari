using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omawari.ViewModels
{
    using Omawari.Models;
    using System.ComponentModel;
    using System.Windows.Media;
    using System.Windows;

    public class DetailViewModel : BindableBase
    {
        private Scraper scraper = default(Scraper);
        private ScrapingResult result;
        private RelayCommand runCommand;
        private RelayCommand<Window> okCommand;
        private RelayCommand<Window> cancelCommand;

        public RelayCommand RunCommand
        {
            get
            {
                if (runCommand != null) return runCommand;

                return runCommand = new RelayCommand(async () => 
                {
                    Result = await Scraper.RunAsync();
                });
            }
        }

        public RelayCommand<System.Windows.Window> OKCommand
        {
            get
            {
                if (okCommand != null) return okCommand;

                return okCommand = new RelayCommand<System.Windows.Window>((window) =>
                {
                    if (Result?.Status != "success")
                    {
                        App.ShowCaution("Please test and pass it.");
                        return;
                    }

                    if (string.IsNullOrEmpty(Scraper.Name)) Scraper.Name = $"{Scraper.Target} @ {Scraper.Selectors}";

                    var item = App.ScraperCollection.FirstOrDefault(_ => _.Id == Scraper.Id);

                    if (item == null)
                        App.ScraperCollection.Add(Scraper);
                    else
                        App.ScraperCollection.Exchange(item, Scraper);

                    App.ScraperCollection.Save();

                    window.Close();
                });
            }
        }
        
        public RelayCommand<System.Windows.Window> CancelCommand
        {
            get
            {
                if (cancelCommand != null) return cancelCommand;

                return cancelCommand = new RelayCommand<System.Windows.Window>((window) =>
                {
                    window.Close();
                });
            }
        }

        public string Title
        {
            get
            {
                return string.IsNullOrEmpty(Scraper?.Name)
                    ? "New Scraper"
                    : Scraper.Name;
            }
        }

        public Scraper Scraper
        {
            get { return scraper; }
            set { SetProperty(ref scraper, value); }
        }

        public ScrapingResult Result
        {
            get { return result; }
            private set { SetProperty(ref result, value); }
        }
    }
}
