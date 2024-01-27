using ComputerArchitect.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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
    /// Логика взаимодействия для CPUPage.xaml
    /// </summary>
    public partial class MotherBoardPage : Page
    {
        public Users CurrentUser { get; set; }

        public MotherBoardPage(Users currentUser)
        {
            CurrentUser = currentUser;
            InitializeComponent();
            LoadComponent();
            MostCheapestSort_Checked(null, null);

        }
        public class CombinedData
        {
            public Motherboards Motherboard { get; set; }
            public Sockets Socket { get; set; }
            public Memory_types MemoryType { get; set; }
            public Form_Factors FormFactor { get; set; }
        }

        private void LoadComponent()
        {
            List<Motherboards> motherboards = App.Database.Motherboards.ToList();
            List<ComputerArchitect.Database.Sockets> sockets = App.Database.Sockets.ToList();
            List<Memory_types> memoryTypes = App.Database.Memory_types.ToList();
            List<Form_Factors> formFactors = App.Database.Form_Factors.ToList(); 

            var combinedData = from motherboard in motherboards
                               join socket in sockets on motherboard.Socket equals socket.SocketId into motherboardSocketGroup
                               from socketData in motherboardSocketGroup.DefaultIfEmpty()
                               join memoryType in memoryTypes on motherboard.Memory_Type equals memoryType.Memory_typeId into motherboardMemoryTypeGroup
                               from memoryTypeData in motherboardMemoryTypeGroup.DefaultIfEmpty()
                               join formFactor in formFactors on motherboard.Form_Factor equals formFactor.Form_FactorId into motherboardFormFactorGroup
                               from formFactorData in motherboardFormFactorGroup.DefaultIfEmpty()
                               select new CombinedData
                               {
                                   Motherboard = motherboard,
                                   Socket = socketData,
                                   MemoryType = memoryTypeData,
                                   FormFactor = formFactorData
                               };

            ComponentListBox.ItemsSource = combinedData;
            OnStorageCountLabel.Content = $"Материнские платы {ComponentListBox.Items.Count} шт";
        }





        private void SearchInCategoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(ComponentListBox.ItemsSource);

            if (view != null)
            {
                string searchText = SearchInCategoryTextBox.Text.Trim().ToLower();
                view.Filter = item =>
                {
                    if (item is CombinedData combinedData)
                    {
                        string motherboardModel = combinedData.Motherboard.Motherboard_Model.ToLower();
                        string socketName = combinedData.Socket?.SocketName.ToLower() ?? "";

                        return motherboardModel.Contains(searchText) || socketName.Contains(searchText);
                    }
                    return false;
                };
            }
        }


        private void SortLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            SortLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6D6D6"));
        }

        private void SortLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            SortLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6DB2E3"));
        }
        bool ShowSort = true;
        private void SortLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ShowSort)
            {
                SortBorder.Visibility = Visibility.Visible;
            }
            else
            {
                SortBorder.Visibility = Visibility.Collapsed;
            }
            ShowSort = !ShowSort;
        }

        private void MostCheapestSort_Checked(object sender, RoutedEventArgs e)
        {
            if (ComponentListBox != null) 
            {
                SortBorder.Visibility = Visibility.Collapsed;
                SortLabel.Content = "сначала недорогие";
                SortListByPrice(ascending: true);
            }
            ShowSort = !ShowSort;
        }

        private void MostValueableSort_Checked(object sender, RoutedEventArgs e)
        {
            if (ComponentListBox != null) 
            {
                SortBorder.Visibility = Visibility.Collapsed;
                SortLabel.Content = "сначала дорогие";
                SortListByPrice(ascending: false);
            }
            ShowSort = !ShowSort;
        }

        private void SortListByPrice(bool ascending)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(ComponentListBox.ItemsSource);

            if (view != null)
            {
                
                List<CombinedData> combineds = view.Cast<CombinedData>().ToList();


                combineds.Sort((a, b) =>
                {
                    decimal? costA = a.Motherboard.Cost ?? 0m;
                    decimal? costB = b.Motherboard.Cost ?? 0m;

                    return ascending ? decimal.Compare(costA.Value, costB.Value) : decimal.Compare(costB.Value, costA.Value);
                });

                ComponentListBox.ItemsSource = combineds;
            }
        }

        private void CatalogPageOpenLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            CatalogPageOpenLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6DB2E3"));
        }
        private void CatalogPageOpenLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            CatalogPageOpenLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6D6D6"));
        }
        private void BasicPCComponentsPageOpenLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            BasicPCComponentsPageOpenLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6DB2E3"));
        }
        private void BasicPCComponentsPageOpenLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            BasicPCComponentsPageOpenLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6D6D6"));
        }
        private void CatalogPageOpenLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new CatalogPage(CurrentUser));
        }
        private void BasicPCComponentsPageOpenLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new BasicPCComponentsPage(CurrentUser));
        }
    }
}
