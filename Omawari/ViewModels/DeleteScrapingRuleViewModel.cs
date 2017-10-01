using Omawari.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Omawari.ViewModels
{
    public class DeleteScrapingRuleViewModel : BindableBase
    {
        private ScrapingRule rule = null;
        private bool isDeleteData = true;
        private RelayCommand<Window> noCommand = null;
        private RelayCommand<Window> yesCommand = null;

        public ScrapingRule Rule
        {
            get { return rule; }
            set { SetProperty(ref rule, value); }
        }

        public bool IsDeleteData
        {
            get { return isDeleteData; }
            set { SetProperty(ref isDeleteData, value); }
        }

        public RelayCommand<System.Windows.Window> NoCommand
        {
            get
            {
                if (noCommand != null) return noCommand;

                return noCommand = new RelayCommand<Window>((window) =>
                {
                    window.DialogResult = false;
                    window.Close();
                });
            }
        }

        public RelayCommand<System.Windows.Window> YesCommand
        {
            get
            {
                if (yesCommand != null) return yesCommand;

                return yesCommand = new RelayCommand<Window>((window) =>
                {
                    window.DialogResult = true;
                    window.Close();
                });
            }
        }
    }
}
