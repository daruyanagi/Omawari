using Omawari.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omawari.ViewModels
{
    public class ScrapingTestViewModel : BindableBase
    {
        private ScrapingRule rule = null;
        private ScrapingResult result = null;
        private RelayCommand<System.Windows.Window> okCommand = null;

        public ScrapingRule Rule
        {
            get { return rule; }
            set { SetProperty(ref rule, value); OnPropertyChanged(nameof(Title)); }
        }

        public ScrapingResult Result
        {
            get { return result; }
            set { SetProperty(ref result, value); }
        }

        public string Title
        {
            get { return $"Test: {Rule?.Name} (IsDynaic: {Rule?.IsDynamic})"; }
        }

        public RelayCommand<System.Windows.Window> OkCommand
        {
            get
            {
                if (okCommand != null) return okCommand;

                return okCommand = new RelayCommand<System.Windows.Window>((window) => window.Close());
            }
        }

        internal async Task TestAsync()
        {
            try
            {
                Result = await Rule.RunAsync();
            }
            catch
            {
                rule.Status = ScrapingStatus.Failed;
            }
        }
    }
}
