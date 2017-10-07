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
    /// ScrapingResultWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ScrapingResultWindow : Window
    {
        public ViewModels.ScrapingResultViewModel ViewModel { get; } = new ViewModels.ScrapingResultViewModel();

        private ScrapingResultWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public ScrapingResultWindow(Models.ScrapingResult result) : this()
        {
            ViewModel.Result = result;
        }
    }
}
