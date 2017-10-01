using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Omawari.Models;

namespace Omawari.ViewModels
{
    public class ScrapingRuleDetailViewModel : BindableBase
    {
        private RelayCommand<Window> okCommand = null;
        private RelayCommand<Window> cancelCommand = null;
        private ScrapingRule rule = null;
        private string title = null;

        private void Update()
        {
            Title = $"Rule: {Rule.Name}";
        }

        public RelayCommand<System.Windows.Window> OkCommand
        {
            get
            {
                if (okCommand != null) return okCommand;

                return okCommand = new RelayCommand<Window>((window) =>
                {
                    window.Close();
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
                    window.Close();
                });
            }
        }

        public ScrapingRule Rule
        {
            get { return rule; }
            set
            {
                SetProperty(ref rule, value);
                if (rule != null) Update();
            }
        }

        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }
    }
}
