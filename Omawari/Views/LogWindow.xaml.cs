using Omawari.Models;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Omawari.Views
{
    /// <summary>
    /// LogWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class LogWindow : Window
    {
        public ViewModels.LogViewModel ViewModel { get; } = new ViewModels.LogViewModel();

        private LogWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public LogWindow(Scraper scraper) : this()
        {
            ViewModel.Scraper = scraper;
        }

        public LogWindow(Scraper scraper, ScrapingResult result) : this(scraper)
        {
            ViewModel.SelectedResult = result;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
