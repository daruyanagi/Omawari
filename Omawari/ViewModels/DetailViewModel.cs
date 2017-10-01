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
        private ScrapingRule scraper = default(ScrapingRule);
        private ScrapingResult result;
        private RelayCommand runCommand;
        private RelayCommand<Window> okCommand;
        private RelayCommand<Window> cancelCommand;

        private void AddOrSave()
        {
            // 既存のものは上書き
            // 新規は追加して保存する

            var item = App.Instance.ScraperCollection.FirstOrDefault(_ => _.Id == Scraper.Id);
            if (item == null)
            {
                App.Instance.ScraperCollection.Add(Scraper);
            }
            else
            {
                App.Instance.ScraperCollection.Exchange(item, Scraper);
            }
            App.Instance.ScraperCollection.Save();
        }

        private void FillNameIfEmpty()
        {
            // 名前が空ならば補完しておく
            // ToDo: グローバル設定に入れる
            if (string.IsNullOrEmpty(Scraper.Name))
            {
                Scraper.Name = $"{Scraper.Target} @ {Scraper.Selectors}";
            }
        }

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

                return okCommand = new RelayCommand<Window>((window) =>
                {
                    if (Result?.Status != "success")
                    {
                        App.Instance.ShowCaution("Please test and pass it.");
                        return;
                    }

                    FillNameIfEmpty();
                    AddOrSave();

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

        public ScrapingRule Scraper
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
