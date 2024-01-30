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

namespace ComputerArchitect.Pages
{
    /// <summary>
    /// Логика взаимодействия для CatalogPage.xaml
    /// </summary>
    public partial class CatalogPage : Page
    {
        public Users CurrentUser { get; set; }
        public CatalogPage(Users currentUser)
        {
            CurrentUser = currentUser;
            InitializeComponent();
        }

        private void BasicPCComponentsPageCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new BasicPCComponentsPage(CurrentUser));
        }

        private void OpenPcConfButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PCConfiguratorPage(CurrentUser));
        }

        private void OpenReasyMadeAssebliesPageButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ReadyMadeAssembliesPage(CurrentUser));
        }
    }
}
