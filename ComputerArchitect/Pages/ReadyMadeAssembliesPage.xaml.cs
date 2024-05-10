using ComputerArchitect.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
    /// Логика взаимодействия для ReadyMadeAssembliesPage.xaml
    /// </summary>
    public partial class ReadyMadeAssembliesPage : Page
    {
        public event EventHandler CartUpdated;
        public Users CurrentUser { get; set; }
        public ReadyMadeAssembliesPage(Users currentUser)
        {
            CurrentUser = currentUser;
            InitializeComponent();
            LoadComponent();
            MostCheapestSort_Checked(null, null);
        }
        public class CombinedData
        {

            public ReadyMadeAssemblies Assembly { get; set; }
            public CPUS Processor { get; set; }
            public Motherboards Motherboards { get; set; }
            public Cases Cases { get; set; }
            public GPUS GPUS { get; set; }
            public Coolers Coolers { get; set; }
            public RAMS RAMS { get; set; }
            public HDDs HDDs { get; set; }
            public PowerSupplies PowerSupplies { get; set; }

            public decimal TotalCost => ((Processor?.Cost ?? 0) +
                                 (Motherboards?.Cost ?? 0) +
                                 (Cases?.Cost ?? 0) +
                                 (GPUS?.Cost ?? 0) +
                                 (Coolers?.Cost ?? 0) +
                                 (RAMS?.Cost ?? 0) +
                                 (HDDs?.Cost ?? 0) +
                                 (PowerSupplies?.Cost ?? 0));
        }
        double minValue;
        double maxValue;
        private void LoadComponent()
        {
            List<ReadyMadeAssemblies> assemblies = App.Database.ReadyMadeAssemblies.ToList();
            var combinedAssemblies = from assembly in assemblies
                                     join processor in App.Database.CPUS on assembly.CpuId equals processor.CPUId into configurationProcessorGroup
                                     from processorData in configurationProcessorGroup.DefaultIfEmpty()
                                     join motherboard in App.Database.Motherboards on assembly.MotherboardId equals motherboard.MotherboardId into configurationMotherboardGroup
                                     from motherboardData in configurationMotherboardGroup.DefaultIfEmpty()
                                     join caseItem in App.Database.Cases on assembly.CaseId equals caseItem.CaseId into configurationCaseGroup
                                     from caseData in configurationCaseGroup.DefaultIfEmpty()
                                     join gpu in App.Database.GPUS on assembly.GPUId equals gpu.GPUId into configurationGPUGroup
                                     from gpuData in configurationGPUGroup.DefaultIfEmpty()
                                     join cooler in App.Database.Coolers on assembly.FanId equals cooler.CoolerId into configurationCoolerGroup
                                     from coolerData in configurationCoolerGroup.DefaultIfEmpty()
                                     join ram in App.Database.RAMS on assembly.RAMId equals ram.RAMId into configurationRAMGroup
                                     from ramData in configurationRAMGroup.DefaultIfEmpty()
                                     join hdd in App.Database.HDDs on assembly.MemoryId equals hdd.HDDId into configurationHDDGroup
                                     from hddData in configurationHDDGroup.DefaultIfEmpty()
                                     join psu in App.Database.PowerSupplies on assembly.PowerSuppliesId equals psu.PowerSupplyId into configurationPSUGroup
                                     from psuData in configurationPSUGroup.DefaultIfEmpty()
                                     select new CombinedData
                                     {
                                         Assembly = assembly,
                                         Processor = processorData,
                                         Motherboards = motherboardData,
                                         Cases = caseData,
                                         GPUS = gpuData,
                                         Coolers = coolerData,
                                         RAMS = ramData,
                                         HDDs = hddData,
                                         PowerSupplies = psuData
                                     };

            if (combinedAssemblies.Any())
            {
                decimal minAssemblyCost = Convert.ToInt32( combinedAssemblies.Min(item => item.TotalCost));
                decimal maxAssemblyCost = Convert.ToInt32(combinedAssemblies.Max(item => item.TotalCost));
                MinPrice.Tag = "от " + minAssemblyCost.ToString();
                MaxPrice.Tag = "до " + maxAssemblyCost.ToString();
            }
            else
            {
                MinPrice.Tag = "от 0";
                MaxPrice.Tag = "до 0";
            }


            // Вывод информации о сборках.
            ComponentListBox.ItemsSource = combinedAssemblies;
            OnStorageCountLabel.Content = $"Готовые сборки {ComponentListBox.Items.Count} шт";
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
                        // Процессор
                        string processorModel = combinedData.Processor?.Model?.ToLower() ?? "";
                        // Материнская плата
                        string motherboardModel = combinedData.Motherboards?.Motherboard_Model?.ToLower() ?? "";
                        // Корпус
                        string caseModel = combinedData.Cases?.Model?.ToLower() ?? "";
                        // Видеокарта
                        string gpuModel = combinedData.GPUS?.GPU_Model?.ToLower() ?? "";
                        // Система охлаждения
                        string coolerModel = combinedData.Coolers?.Model?.ToLower() ?? "";
                        // Оперативная память
                        string ramModel = combinedData.RAMS?.RAM_Model?.ToLower() ?? "";
                        // Жесткий диск
                        string hddModel = combinedData.HDDs?.Model?.ToLower() ?? "";
                        // Блок питания
                        string psuModel = combinedData.PowerSupplies?.Model?.ToLower() ?? "";

                        return processorModel.Contains(searchText) ||
                               motherboardModel.Contains(searchText) ||
                               caseModel.Contains(searchText) ||
                               gpuModel.Contains(searchText) ||
                               coolerModel.Contains(searchText) ||
                               ramModel.Contains(searchText) ||
                               hddModel.Contains(searchText) ||
                               psuModel.Contains(searchText);
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
                    decimal totalCostA = CalculateTotalCost(a);
                    decimal totalCostB = CalculateTotalCost(b);

                    return ascending ? Decimal.Compare(totalCostA, totalCostB) : Decimal.Compare(totalCostB, totalCostA);
                });

                view = CollectionViewSource.GetDefaultView(combineds);
                ComponentListBox.ItemsSource = view;
            }
        }

        private decimal CalculateTotalCost(CombinedData data)
        {
            decimal totalCost = 0m;

            totalCost += data.Processor?.Cost ?? 0m;
            totalCost += data.Motherboards?.Cost ?? 0m;
            totalCost += data.Cases?.Cost ?? 0m;
            totalCost += data.GPUS?.Cost ?? 0m;
            totalCost += data.Coolers?.Cost ?? 0m;
            totalCost += data.RAMS?.Cost ?? 0m;
            totalCost += data.HDDs?.Cost ?? 0m;
            totalCost += data.PowerSupplies?.Cost ?? 0m;

            return totalCost;
        }


        private void MinPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }

        private void MaxPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }



        private List<CombinedData> originalCombineds;
        private void AcceptFilters_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(ComponentListBox.ItemsSource);

            if (view != null)
            {
                if (originalCombineds == null)
                {
                    originalCombineds = view.Cast<CombinedData>().ToList();
                }
                List<CombinedData> combineds = new List<CombinedData>(originalCombineds);

                // Проверка и установка минимальной цены
                if (string.IsNullOrWhiteSpace(MinPrice.Text))
                {
                    minValue = (double)combineds.Min(item => CalculateTotalCost(item));
                    MinPrice.Tag = "от " + minValue.ToString();
                }
                else
                {
                    minValue = double.Parse(MinPrice.Text);
                }

                // Проверка и установка максимальной цены
                if (string.IsNullOrWhiteSpace(MaxPrice.Text))
                {
                    maxValue = (double)combineds.Max(item => CalculateTotalCost(item));
                    MaxPrice.Tag = "до " + maxValue.ToString();
                }
                else
                {
                    maxValue = double.Parse(MaxPrice.Text);
                }

                combineds = combineds.Where(item => CalculateTotalCost(item) >= (decimal)minValue && CalculateTotalCost(item) <= (decimal)maxValue).ToList();

                // Если вы хотите отсортировать результаты по цене, раскомментируйте следующую строку
                // combineds = combineds.OrderBy(item => CalculateTotalCost(item)).ToList();

                ComponentListBox.ItemsSource = combineds;
            }
        }

        private void DecilineFilters_Click(object sender, RoutedEventArgs e)
        {
            MinPrice.Text = null;
            MaxPrice.Text = null;

            if (originalCombineds != null)
            {
                ComponentListBox.ItemsSource = originalCombineds;
            }
        }




        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var combinedData = button?.DataContext as CombinedData;

            if (combinedData != null)
            {
                var assembly = combinedData.Assembly;

                int userId = CurrentUser.Id;

                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    var userCart = context.UsersCarts
                        .Include("CartItems")
                        .FirstOrDefault(c => c.UserId == userId);

                    if (userCart == null)
                    {
                        userCart = new UsersCarts
                        {
                            UserId = userId
                        };

                        context.UsersCarts.Add(userCart);
                    }

                    // Перечень компонентов для добавления в корзину
                    var componentsToAdd = new List<int?>
            {
                assembly.CpuId,
                assembly.MotherboardId,
                assembly.CaseId,
                assembly.GPUId,
                assembly.FanId,
                assembly.RAMId,
                assembly.MemoryId,
                assembly.PowerSuppliesId
                // Добавьте сюда остальные компоненты, если есть
            };

                    foreach (var componentId in componentsToAdd)
                    {
                        if (componentId != null)
                        {
                            var cartItem = new CartItems
                            {
                                CartId = userCart.CartId,
                                UsersCarts = userCart
                            };

                            // Устанавливаем соответствующий Id компонента в зависимости от типа
                            if (componentId == assembly.CpuId)
                                cartItem.CpuId = componentId;
                            else if (componentId == assembly.MotherboardId)
                                cartItem.MotherboardId = componentId;
                            else if (componentId == assembly.CaseId)
                                cartItem.CaseId = componentId;
                            else if (componentId == assembly.GPUId)
                                cartItem.GPUId = componentId;
                            else if (componentId == assembly.FanId)
                                cartItem.FanId = componentId;
                            else if (componentId == assembly.RAMId)
                                cartItem.RAMId = componentId;
                            else if (componentId == assembly.MemoryId)
                                cartItem.MemoryId = componentId;
                            else if (componentId == assembly.PowerSuppliesId)
                                cartItem.PowerSuppliesId = componentId;

                            userCart.CartItems.Add(cartItem);
                        }
                    }

                    context.SaveChanges();

                    // Обновление состояния кнопки
                    button.Content = "Добавлено";
                    button.IsEnabled = false;
                }
            }
            else
            {
                MessageBox.Show("Ошибка: не удалось определить выбранную сборку.");
            }
            // Обновление информации о корзине
            CartUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}
