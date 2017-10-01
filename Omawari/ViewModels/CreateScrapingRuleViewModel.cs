using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Omawari.Models;

namespace Omawari.ViewModels
{
    public class CreateScrapingRuleViewModel : BindableBase
    {
        private ScrapingRule rule = new ScrapingRule();
        private RelayCommand testCommand = null;
        private RelayCommand<Window> cancelCommand = null;
        private RelayCommand<Window> okCommand = null;

        public ScrapingRule Rule
        {
            get { return rule; }
            set { SetProperty(ref rule, value); }
        }
        
        public RelayCommand TestCommand
        {
            get
            {
                if (testCommand != null) return testCommand;

                return testCommand = new RelayCommand(() =>
                {
                    App.Instance.TestRule(Rule);
                });
            }
        }

        public RelayCommand<System.Windows.Window> CancelCommand
        {
            get
            {
                if (cancelCommand != null) return cancelCommand;

                return cancelCommand = new RelayCommand<Window>((window) =>
                {
                    window.DialogResult = false;
                    window.Close();
                });
            }
        }

        public RelayCommand<System.Windows.Window> OkCommand
        {
            get
            {
                if (okCommand != null) return okCommand;

                return okCommand = new RelayCommand<Window>((window) =>
                {
                    window.DialogResult = true;
                    window.Close();
                });
            }
        }
    }
}
