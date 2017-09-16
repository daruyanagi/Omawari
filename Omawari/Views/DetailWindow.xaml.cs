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
    /// DetailWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DetailWindow : Window
    {
        public ViewModels.DetailViewModel ViewModel { get; } = new ViewModels.DetailViewModel();

        private DetailWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public DetailWindow(Models.Scraper scraper) : this()
        {
            ViewModel.Scraper = scraper;
        }

        public Models.Scraper Scraper { get { return ViewModel.Scraper; } }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
