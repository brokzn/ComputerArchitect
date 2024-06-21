using ComputerArchitect.Database;
using ComputerArchitect.Pages.AdminPages;
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
    /// Логика взаимодействия для AdminPanelPage.xaml
    /// </summary>
    public partial class AdminPanelPage : Page
    {
        public Users CurrentUser { get; set; }
        public AdminPanelPage(Users currentUser)
        {
            CurrentUser = currentUser;
            InitializeComponent();
            AdminPanelFrame.NavigationService.Navigate(new UsersTabelPage());
        }

        private void UsersTabelButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentSelectedTableLabel.Content = "Список пользователей";
            AdminPanelFrame.NavigationService.Navigate(new UsersTabelPage());
        }

        private void SelfTakeOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentSelectedTableLabel.Content = "Заказы на самовывоз";
            AdminPanelFrame.NavigationService.Navigate(new SelfTakeOrdersPage());
        }

        private void EndingComponentsButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentSelectedTableLabel.Content = "Заканчивающиеся компоненты";
            AdminPanelFrame.NavigationService.Navigate(new EndingComponentsPage());
        }

        private void SellStatsPageButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentSelectedTableLabel.Content = "Статистика продаж";
            AdminPanelFrame.NavigationService.Navigate(new SellGraphPage());
        }
    }
}
