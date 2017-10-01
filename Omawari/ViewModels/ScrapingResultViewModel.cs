using Omawari.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omawari.ViewModels
{
    public class ScrapingResultViewModel : BindableBase
    {
        private RelayCommand<System.Windows.Window> okCommand = null;
        private ScrapingResult result = null;
        private ScrapingResult oldResult = null;

        public ScrapingResult Result
        {
            get { return result; }
            set { SetProperty(ref result, value); OnPropertyChanged(nameof(Title)); }
        }

        public ScrapingResult OldResult
        {
            get { return oldResult; }
            set { SetProperty(ref oldResult, value); OnPropertyChanged(nameof(Diff)); }
        }

        public string Title
        {
            get { return $"Result: {Result?.Id}"; }
        }

        public string Diff
        {
            get { return Result.Diff(OldResult); }
        }

        public RelayCommand<System.Windows.Window> OkCommand
        {
            get
            {
                if (okCommand != null) return okCommand;

                return okCommand = new RelayCommand<System.Windows.Window>((window) => window.Close());
            }
        }
    }
}
