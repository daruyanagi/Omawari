using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Omawari.Models;

namespace Omawari.ViewModels
{
    public class ScrapingRuleViewModel : BindableBase
    {
        private ScrapingRule rule = null;
        private RelayCommand manualCheckCommand = null;
        private RelayCommand openUrlCommand = null;
        private RelayCommand openDataFolderCommand = null;
        private RelayCommand testCommand = null;
        private RelayCommand<Window> okCommand = null;

        public ScrapingRule Rule
        {
            get { return rule; }
            set { SetProperty(ref rule, value); OnPropertyChanged(nameof(Title)); }
        }

        public string Title
        {
            get { return $"Rule: {rule?.Name}"; }
        }

        public RelayCommand ManualCheckCommand
        {
            get
            {
                if (manualCheckCommand != null) return manualCheckCommand;

                return manualCheckCommand = new RelayCommand(async () =>
                {
                    await Rule.CheckAsync();
                });
            }
        }

        public RelayCommand OpenUrlCommand
        {
            get
            {
                if (openUrlCommand != null) return openUrlCommand;

                return openUrlCommand = new RelayCommand(() =>
                {
                    System.Diagnostics.Process.Start(Rule.Target.ToString());
                });
            }
        }

        public RelayCommand OpenDataFolderCommand
        {
            get
            {
                if (openDataFolderCommand != null) return openDataFolderCommand;

                return openDataFolderCommand = new RelayCommand(() =>
                {
                    System.Diagnostics.Process.Start(Rule.Location.ToString());
                });
            }
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

        public RelayCommand<System.Windows.Window> OkCommand
        {
            get
            {
                if (okCommand != null) return okCommand;

                return okCommand = new RelayCommand<System.Windows.Window>((window) =>
                {
                    window.Close();
                });
            }
        }
    }
}
