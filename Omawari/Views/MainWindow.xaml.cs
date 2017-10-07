using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Omawari.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public ViewModels.MainViewModel ViewModel { get; } = new ViewModels.MainViewModel();
        
        private Utilities.ThrottoleAction filterAction = null;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            filterAction = new Utilities.ThrottoleAction(Dispatcher, () =>
            {
                if (ruleListView.ItemsSource == null) return;
                CollectionViewSource.GetDefaultView(ruleListView.ItemsSource).Refresh();
            });

            var refreshAction = new Utilities.ThrottoleAction(Dispatcher, () =>
            {
                if (ruleListView.ItemsSource == null) return;
                CollectionViewSource.GetDefaultView(listViewLogs.ItemsSource).Refresh();
            });

            Models.LogRepository.Instance.Items.CollectionChanged += (sender, args) =>
            {
                refreshAction.Invoke();
            };
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            App.Instance.ShowRule(ViewModel.SelectedItem);
        }
        
        private void listViewLog_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listView = sender as ListView;
            if (listView == null) return;

            var result = listView.SelectedItem as Models.ScrapingResult;
            if (result == null) return;

            App.Instance.ShowResult(result);
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (listViewLogs.ItemsSource == null) return;
            CollectionViewSource.GetDefaultView(listViewLogs.ItemsSource).Refresh();
        }

        private void LogsCollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = false;

            var result = e.Item as Models.ScrapingResult;
            if (result == null) return;

            if (updatesToggleButton.IsChecked == true)
            {
                e.Accepted |= result.Status == "Updated";
                e.Accepted |= result.Status == "New";
            }

            if (errorsToggleButton.IsChecked == true)
            {
                e.Accepted |= result.Status == "Failed";
                e.Accepted |= result.Status == "Empty";
                e.Accepted |= result.Status == "fail"; // 廃止
            }

            if (othersToggleButton.IsChecked == true)
            {
                e.Accepted |= result.Status == "No Changed";
                e.Accepted |= result.Status == "success"; // 廃止
            }
        }

        private void RulesCollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = true;

            if (string.IsNullOrEmpty(filterTextBox.Text)) return;
            
            var rule = e.Item as Models.ScrapingRule;
            if (rule == null) return;
            e.Accepted =
                rule.Name.Contains(filterTextBox.Text) ||
                rule.Target.ToString().Contains(filterTextBox.Text);
        }

        private void filterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            filterAction.Invoke();
        }

        private void ruleListView_Click(object sender, RoutedEventArgs e)
        {
            var header = e.OriginalSource as GridViewColumnHeader;
            if (header == null || header.Column == null) return;

            var binding = header.Column.DisplayMemberBinding as Binding;
            if (binding == null) return;
            
            var listView = sender as ListView;
            if (listView == null) return;

            var view = CollectionViewSource.GetDefaultView(ruleListView.ItemsSource);
            var property = binding.Path.Path;

            try
            {
                var description = view.SortDescriptions.First(_ => _.PropertyName == property);
                
                var direction = description.Direction == ListSortDirection.Descending
                    ? ListSortDirection.Ascending
                    : ListSortDirection.Descending;

                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(property, direction));
            }
            catch
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(property, ListSortDirection.Descending));
            }

            view.Refresh();
        }
    }
}
