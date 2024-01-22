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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComputerArchitect.Pages
{
    /// <summary>
    /// Логика взаимодействия для BasicPCComponentsPage.xaml
    /// </summary>
    public partial class BasicPCComponentsPage : Page
    {
        public BasicPCComponentsPage()
        {
            InitializeComponent();
        }

        private void CatalogPageOpenLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            CatalogPageOpenLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6DB2E3"));
        }
        private void CatalogPageOpenLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            CatalogPageOpenLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6D6D6"));
        }

        private void CatalogPageOpenLabel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           NavigationService.Navigate(new CatalogPage());
        }

        private void OpenCPUPageCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new CPUPage());
        }

        private void OpenMotherBoardPageCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new MotherBoardPage());
        }

        private void OpenGPUPageCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new GPUPage());
        }
    }
}
