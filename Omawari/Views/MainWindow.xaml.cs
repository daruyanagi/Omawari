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

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
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
            ViewModel.LogSelectedScraperCommand.Execute(null);
        }
        
        private void listViewLog_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var result = listViewLog.SelectedItem as Models.ScrapingResult;
            var window = new Views.LogWindow(result.Scraper, result);
            window.ShowDialog();
        }
    }
}
