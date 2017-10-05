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
        private ScrapingResult result = null;
        private ScrapingResult oldResult = null;
        private RelayCommand showParentRuleCommand = null;
        private RelayCommand openUrlCommand = null;
        private RelayCommand openDataFileCommand = null;
        private RelayCommand<System.Windows.Window> okCommand = null;
        private RelayCommand showPreviousLogCommand = null;
        private RelayCommand showNextLogCommand = null;

        public ScrapingResult Result
        {
            get { return result; }
            set
            {
                SetProperty(ref result, value);
                OnPropertyChanged(nameof(Title));
                ShowPreviousLogCommand.RaiseCanExecuteChanged();
                ShowNextLogCommand.RaiseCanExecuteChanged();
            }
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
            get { return Result?.Diff(OldResult); }
        }

        public RelayCommand ShowPreviousLogCommand
        {
            get
            {
                if (showPreviousLogCommand != null) return showPreviousLogCommand;

                return showPreviousLogCommand = new RelayCommand(
                    () =>
                    {
                        var rule = App.Instance.ScraperCollection.FirstOrDefault(_ => _.Id == Result.ScraperId);
                        if (rule == null) return;
                        var index = rule.AllResults.IndexOf(Result);
                        var prev = rule.AllResults.ElementAtOrDefault(index - 1);
                        if (prev != null) Result = prev;
                    },
                    () =>
                    {
                        var rule = App.Instance.ScraperCollection.FirstOrDefault(_ => _.Id == Result.ScraperId);
                        if (rule == null) return false;
                        var index = rule.AllResults.IndexOf(Result);
                        var prev = rule.AllResults.ElementAtOrDefault(index - 1);
                        if (prev == null) return false;
                        return true;
                    }
                );
            }
        }

        public RelayCommand ShowNextLogCommand
        {
            get
            {
                if (showNextLogCommand != null) return showNextLogCommand;

                return showNextLogCommand = new RelayCommand(
                    () =>
                    {
                        var rule = App.Instance.ScraperCollection.FirstOrDefault(_ => _.Id == Result.ScraperId);
                        if (rule == null) return;
                        var index = rule.AllResults.IndexOf(Result);
                        var next = rule.AllResults.ElementAtOrDefault(index + 1);
                        if (next != null) Result = next;
                    },
                    () =>
                    {
                        var rule = App.Instance.ScraperCollection.FirstOrDefault(_ => _.Id == Result.ScraperId);
                        if (rule == null) return false;
                        var index = rule.AllResults.IndexOf(Result);
                        var next = rule.AllResults.ElementAtOrDefault(index + 1);
                        if (next == null) return false;
                        return true;
                    }
                );
            }
        }

        public RelayCommand ShowParentRuleCommand
        {
            get
            {
                if (showParentRuleCommand != null) return showParentRuleCommand;

                return showParentRuleCommand = new RelayCommand(
                    () => App.Instance.ShowRule(Result.Scraper)
                );
            }
        }

        public RelayCommand OpenUrlCommand
        {
            get
            {
                if (openUrlCommand != null) return openUrlCommand;

                return openUrlCommand = new RelayCommand(
                    () => System.Diagnostics.Process.Start(Result.Scraper.Target.ToString())
                );
            }
        }

        public RelayCommand OpenDataFileCommand
        {
            get
            {
                if (openDataFileCommand != null) return openDataFileCommand;

                return openDataFileCommand = new RelayCommand(
                    () => System.Diagnostics.Process.Start("explorer", $"/select,{Result.Location}")
                );
            }
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
