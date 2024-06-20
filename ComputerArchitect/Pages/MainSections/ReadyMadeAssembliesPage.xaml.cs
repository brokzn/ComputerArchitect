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
using static System.Net.Mime.MediaTypeNames;

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
            public Users users { get; set; }
            public ReadyMadeAssembleTypes assembleTypes { get; set; }
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
            // Загружаем все данные с сервера в клиентскую память
            var assemblies = App.Database.ReadyMadeAssemblies.ToList();
            var processors = App.Database.CPUS.ToList();
            var motherboards = App.Database.Motherboards.ToList();
            var cases = App.Database.Cases.ToList();
            var gpus = App.Database.GPUS.ToList();
            var coolers = App.Database.Coolers.ToList();
            var rams = App.Database.RAMS.ToList();
            var hdds = App.Database.HDDs.ToList();
            var psus = App.Database.PowerSupplies.ToList();
            var users = App.Database.Users.ToList();
            var assembleTypes = App.Database.ReadyMadeAssembleTypes.ToList();

            var combinedAssemblies = from assembly in assemblies
                                     join processor in processors on assembly.CpuId equals processor.CPUId into configurationProcessorGroup
                                     from processorData in configurationProcessorGroup.DefaultIfEmpty()
                                     join motherboard in motherboards on assembly.MotherboardId equals motherboard.MotherboardId into configurationMotherboardGroup
                                     from motherboardData in configurationMotherboardGroup.DefaultIfEmpty()
                                     join caseItem in cases on assembly.CaseId equals caseItem.CaseId into configurationCaseGroup
                                     from caseData in configurationCaseGroup.DefaultIfEmpty()
                                     join gpu in gpus on assembly.GPUId equals gpu.GPUId into configurationGPUGroup
                                     from gpuData in configurationGPUGroup.DefaultIfEmpty()
                                     join cooler in coolers on assembly.FanId equals cooler.CoolerId into configurationCoolerGroup
                                     from coolerData in configurationCoolerGroup.DefaultIfEmpty()
                                     join ram in rams on assembly.RAMId equals ram.RAMId into configurationRAMGroup
                                     from ramData in configurationRAMGroup.DefaultIfEmpty()
                                     join hdd in hdds on assembly.MemoryId equals hdd.HDDId into configurationHDDGroup
                                     from hddData in configurationHDDGroup.DefaultIfEmpty()
                                     join psu in psus on assembly.PowerSuppliesId equals psu.PowerSupplyId into configurationPSUGroup
                                     from psuData in configurationPSUGroup.DefaultIfEmpty()
                                     join user in users on assembly.UserId equals user.Id into configurationUserGroup
                                     from userData in configurationUserGroup.DefaultIfEmpty()
                                     join assembleType in assembleTypes on assembly.ReadyMadeAssembleTypeId equals assembleType.Id into configurationAssembleTypeGroup
                                     from assembleTypeData in configurationAssembleTypeGroup.DefaultIfEmpty()
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
                                         PowerSupplies = psuData,
                                         users = userData,
                                         assembleTypes = assembleTypeData,
                                     };

            var combinedList = combinedAssemblies.ToList();

            if (combinedList.Any())
            {
                decimal minAssemblyCost = combinedList.Min(item => item.TotalCost);
                decimal maxAssemblyCost = combinedList.Max(item => item.TotalCost);
                MinPrice.Tag = "от " + minAssemblyCost.ToString();
                MaxPrice.Tag = "до " + maxAssemblyCost.ToString();
            }
            else
            {
                MinPrice.Tag = "от 0";
                MaxPrice.Tag = "до 0";
            }

            // Вывод информации о сборках.
            ComponentListBox.ItemsSource = combinedList;
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
                    var componentsToAdd = new Dictionary<string, int?>
            {
                { "CpuId", assembly.CpuId },
                { "MotherboardId", assembly.MotherboardId },
                { "CaseId", assembly.CaseId },
                { "GPUId", assembly.GPUId },
                { "FanId", assembly.FanId },
                { "RAMId", assembly.RAMId },
                { "MemoryId", assembly.MemoryId },
                { "PowerSuppliesId", assembly.PowerSuppliesId }
            };

                    foreach (var component in componentsToAdd)
                    {
                        var componentId = component.Value;
                        var componentName = component.Key;

                        if (componentId != null)
                        {
                            var existingCartItem = userCart.CartItems.FirstOrDefault(ci =>
                                (componentName == "CpuId" && ci.CpuId == componentId) ||
                                (componentName == "MotherboardId" && ci.MotherboardId == componentId) ||
                                (componentName == "CaseId" && ci.CaseId == componentId) ||
                                (componentName == "GPUId" && ci.GPUId == componentId) ||
                                (componentName == "FanId" && ci.FanId == componentId) ||
                                (componentName == "RAMId" && ci.RAMId == componentId) ||
                                (componentName == "MemoryId" && ci.MemoryId == componentId) ||
                                (componentName == "PowerSuppliesId" && ci.PowerSuppliesId == componentId)
                            );

                            if (existingCartItem != null)
                            {
                                // Increment the existing component count
                                switch (componentName)
                                {
                                    case "CpuId":
                                        existingCartItem.CpuCount = (existingCartItem.CpuCount ?? 0) + 1;
                                        break;
                                    case "MotherboardId":
                                        existingCartItem.MotherboardCount = (existingCartItem.MotherboardCount ?? 0) + 1;
                                        break;
                                    case "CaseId":
                                        existingCartItem.CaseCount = (existingCartItem.CaseCount ?? 0) + 1;
                                        break;
                                    case "GPUId":
                                        existingCartItem.GPUCount = (existingCartItem.GPUCount ?? 0) + 1;
                                        break;
                                    case "FanId":
                                        existingCartItem.FanCount = (existingCartItem.FanCount ?? 0) + 1;
                                        break;
                                    case "RAMId":
                                        existingCartItem.RAMCount = (existingCartItem.RAMCount ?? 0) + 1;
                                        break;
                                    case "MemoryId":
                                        existingCartItem.MemoryCount = (existingCartItem.MemoryCount ?? 0) + 1;
                                        break;
                                    case "PowerSuppliesId":
                                        existingCartItem.PowerSuppliesCount = (existingCartItem.PowerSuppliesCount ?? 0) + 1;
                                        break;
                                }
                            }
                            else
                            {
                                var cartItem = new CartItems
                                {
                                    CartId = userCart.CartId,
                                    UsersCarts = userCart
                                };

                                // Устанавливаем соответствующий Id компонента в зависимости от типа и начальный счетчик
                                switch (componentName)
                                {
                                    case "CpuId":
                                        cartItem.CpuId = componentId;
                                        cartItem.CpuCount = 1;
                                        break;
                                    case "MotherboardId":
                                        cartItem.MotherboardId = componentId;
                                        cartItem.MotherboardCount = 1;
                                        break;
                                    case "CaseId":
                                        cartItem.CaseId = componentId;
                                        cartItem.CaseCount = 1;
                                        break;
                                    case "GPUId":
                                        cartItem.GPUId = componentId;
                                        cartItem.GPUCount = 1;
                                        break;
                                    case "FanId":
                                        cartItem.FanId = componentId;
                                        cartItem.FanCount = 1;
                                        break;
                                    case "RAMId":
                                        cartItem.RAMId = componentId;
                                        cartItem.RAMCount = 1;
                                        break;
                                    case "MemoryId":
                                        cartItem.MemoryId = componentId;
                                        cartItem.MemoryCount = 1;
                                        break;
                                    case "PowerSuppliesId":
                                        cartItem.PowerSuppliesId = componentId;
                                        cartItem.PowerSuppliesCount = 1;
                                        break;
                                }

                                userCart.CartItems.Add(cartItem);
                            }
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
