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
    /// ScrapingRuleWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ScrapingRuleWindow : Window
    {
        public ViewModels.ScrapingRuleViewModel ViewModel { get; } = new ViewModels.ScrapingRuleViewModel();

        private ScrapingRuleWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public ScrapingRuleWindow(Models.ScrapingRule scraper) : this()
        {
            ViewModel.Rule = scraper;
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listView = sender as ListView;
            var item = listView.SelectedItem as Models.ScrapingResult;

            App.Instance.ShowResult(item);
        }
    }
}
