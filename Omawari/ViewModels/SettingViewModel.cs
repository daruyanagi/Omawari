using Omawari.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omawari.ViewModels
{
    public class SettingViewModel : BindableBase
    {
        private RelayCommand<System.Windows.Window> okCommand = null;
        private GlobalSettings settings = Models.GlobalSettings.Load(App.SettingsPath);

        public RelayCommand<System.Windows.Window> OKCommand
        {
            get
            {
                okCommand = okCommand ?? new RelayCommand<System.Windows.Window>((window) => {
                    App.GlobalSettings = settings;
                    settings.Save();
                    window.Close();
                });

                return okCommand;
            }
        }

        public GlobalSettings Settings
        {
            get { return settings; }
            set { SetProperty(ref settings, value); }
        }
    }
}
