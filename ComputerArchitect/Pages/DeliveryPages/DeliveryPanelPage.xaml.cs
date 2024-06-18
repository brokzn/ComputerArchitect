using ComputerArchitect.Database;
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
using ComputerArchitect.Pages.DeliveryPages;

namespace ComputerArchitect.Pages
{
    /// <summary>
    /// Логика взаимодействия для DeliveryPanelPage.xaml
    /// </summary>
    public partial class DeliveryPanelPage : Page
    {
        public Users CurrentUser { get; set; }
        public DeliveryPanelPage(Users currentUser)
        {
            CurrentUser = currentUser;
            InitializeComponent();
            DeliveryPanelFrame.NavigationService.Navigate(new WaitingOrdersPage(CurrentUser));
        }

        private void WaitingOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            DeliveryPanelFrame.NavigationService.Navigate(new WaitingOrdersPage(CurrentUser));
            CurrentSelectedTableLabel.Content = "Доступные заказы";
        }

        private void OrderInProcess_Click(object sender, RoutedEventArgs e)
        {
            DeliveryPanelFrame.NavigationService.Navigate(new OrderInProcessPage(CurrentUser));
            CurrentSelectedTableLabel.Content = "Текущий заказ";
        }
    }
}
