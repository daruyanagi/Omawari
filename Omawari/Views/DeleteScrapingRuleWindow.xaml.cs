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
    /// DeleteScrapingRuleWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DeleteScrapingRuleWindow : Window
    {
        public ViewModels.DeleteScrapingRuleViewModel ViewModel { get; } = new ViewModels.DeleteScrapingRuleViewModel();

        private DeleteScrapingRuleWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public DeleteScrapingRuleWindow(Models.ScrapingRule rule) : this()
        {
            ViewModel.Rule = rule;
        }
    }
}
