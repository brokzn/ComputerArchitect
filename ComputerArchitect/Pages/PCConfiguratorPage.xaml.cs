using ComputerArchitect.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComputerArchitect.Pages
{
    /// <summary>
    /// Логика взаимодействия для PCConfiguratorPage.xaml
    /// </summary>
    public partial class PCConfiguratorPage : Page
    {
        private BitmapImage ByteArrayToBitmapImage(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0)
            {
                return null;
            }

            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }
        public Users CurrentUser { get; set; }
        public PCConfiguratorPage(Users currentUser)
        {
            CurrentUser = currentUser;
            
            Loaded += Page_Loaded;
            InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CPULoadComponent();
            CPUMostCheapestSort_Checked(null, null);


            MotherboardLoadComponent();
            MotherboardMostCheapestSort_Checked(null, null);

            UpdateUserData();

        }
        private void UpdateUserData()
        {
            using (var context = new ComputerArchitectDataBaseEntities())
            {
                int userId = CurrentUser.Id;

                // Поиск существующей конфигурации пользователя
                UserConfiguration existingConfiguration = context.UserConfiguration
                    .FirstOrDefault(config => config.UserId == userId);

                if (existingConfiguration != null)
                {
                    // Получение CombinedData, соответствующего выбранной конфигурации процессора
                    CPUCombinedData selectedCPUData = ((IEnumerable<CPUCombinedData>)CPUListBox.ItemsSource)
                        .FirstOrDefault(data => data.Processor.CPUId == existingConfiguration.CpuId);

                    // Получение CombinedData, соответствующего выбранной конфигурации материнской платы
                    MotherboardCombinedData selectedMotherboardData = ((IEnumerable<MotherboardCombinedData>)MotherboardListBox.ItemsSource)
                        .FirstOrDefault(data => data.Motherboard.MotherboardId == existingConfiguration.MotherboardId);

                    int countOfSelectedComponents = 0;
                    int totalCost = 0; // Добавлено для подсчета общей стоимости

                    if (selectedCPUData != null)
                    {
                        CPUDisplaySelectedData(selectedCPUData);
                        countOfSelectedComponents++;
                        totalCost += Convert.ToInt32(selectedCPUData.Processor.Cost); // Предположим, что у CPUCombinedData есть свойство Cost
                    }

                    if (selectedMotherboardData != null)
                    {
                        MotherboardDisplaySelectedData(selectedMotherboardData);
                        countOfSelectedComponents++;
                        totalCost += Convert.ToInt32(selectedMotherboardData.Motherboard.Cost); // Предположим, что у MotherboardCombinedData есть свойство Cost
                    }

                    // Проверка совместимости процессора и материнской платы
                    if (selectedCPUData != null && selectedMotherboardData != null
                        && selectedCPUData.Processor.Socket != selectedMotherboardData.Motherboard.Socket)
                    {
                        // Процессор и материнская плата не совместимы
                        ComponentsСompatibilityAllPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C71919"));
                        ComponentsСompatibilityAll.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C71919"));
                    }
                    else
                    {
                        // Процессор и материнская плата совместимы
                        ComponentsСompatibilityAllPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9"));
                        ComponentsСompatibilityAll.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9"));
                    }

                    // Установка видимости ConfiguratorStatusGrid в зависимости от количества выбранных компонентов
                    ConfiguratorStatusGrid.Visibility = countOfSelectedComponents > 0 ? Visibility.Visible : Visibility.Collapsed;

                    // Установка Grid.ColumnSpan в OneSelectedComponent в зависимости от количества выбранных компонентов
                    if (countOfSelectedComponents > 0)
                    {
                        Grid.SetColumnSpan(OneSelectedComponent, countOfSelectedComponents);
                    }
                    else
                    {
                        Grid.SetColumnSpan(OneSelectedComponent, 1);
                    }

                    // Отображение количества выбранных компонентов в CountOfSelectedComponents label
                    CountOfImportantComponents.Content = "Обязательные комплектующие - " + countOfSelectedComponents + " из 7";
                    CountOfSelectedComponents.Content = "Всего выбрано: " + countOfSelectedComponents.ToString();

                    // Отображение общей стоимости выбранных компонентов
                    ComponentsTotalCostLabel.Content = "Итого: " + totalCost.ToString("N0") + " ₽"; // "C" форматирует как валюту
                }
                else
                {
                    // Если оба значения равны null, скрываем ConfiguratorStatusGrid и устанавливаем CountOfSelectedComponents в 0
                    ConfiguratorStatusGrid.Visibility = Visibility.Collapsed;
                    CountOfSelectedComponents.Content = "0";

                    // Сброс RowSpan в OneSelectedComponent, если нет выбранных компонентов
                    Grid.SetColumnSpan(OneSelectedComponent, 1);

                    // Обнуление ComponentsTotalCostLabel, если нет выбранных компонентов
                    ComponentsTotalCostLabel.Content = "Итого: 0";
                }
            }
        }
        



        private void ClearConfigButton_Click(object sender, RoutedEventArgs e)
        {

            int userId = CurrentUser.Id;

            using (var context = new ComputerArchitectDataBaseEntities())
            {
                // Получение текущей конфигурации пользователя
                UserConfiguration existingConfiguration = context.UserConfiguration
                    .FirstOrDefault(config => config.UserId == userId);

                if (existingConfiguration != null)
                {
                    // Установка всех свойств конфигурации в null
                    existingConfiguration.CpuId = null;
                    existingConfiguration.MotherboardId = null;
                    // Дополните эту часть кода для других свойств конфигурации, если они есть

                    context.Entry(existingConfiguration).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }

            // Скрыть изображение и информацию о выбранном процессоре
            SelectedCPUImage.Visibility = Visibility.Collapsed;
            SelectedCPULabelModel.Visibility = Visibility.Collapsed;

            // Скрыть изображение и информацию о выбранной материнской плате
            SelectedMotherboardImage.Visibility = Visibility.Collapsed;
            SelectedMotherboardLabelModel.Visibility = Visibility.Collapsed;

            UpdateUserData();
        }




        //==============================//
        //          ПРОЦЕССОР           //
        //==============================//

        bool CPUSelectGridVisible = false;
        private void OpenCPUSelectGridButton_Click(object sender, RoutedEventArgs e)
        {
            if (CPUSelectGridVisible)
            {
                CPUSelectGrid.Visibility = Visibility.Collapsed;
                OpenCPUSelectGridButton.Content = "Выбрать";
                CPUSelectRowHeight.Height = new GridLength(2, GridUnitType.Pixel);
            }
            else
            {
                CPUSelectGrid.Visibility = Visibility.Visible;
                OpenCPUSelectGridButton.Content = "Скрыть";
                CPUSelectRowHeight.Height = new GridLength(750, GridUnitType.Pixel);
            }
            CPUSelectGridVisible = !CPUSelectGridVisible;
        }
        public class CPUCombinedData
        {
            public CPUS Processor { get; set; }
            public Sockets Socket { get; set; }
            public Manufacturers Manufacturer { get; set; }
            public Memory_types MemoryType { get; set; }
        }
        private void CPULoadComponent()
        {
            List<CPUS> processors = App.Database.CPUS.ToList();
            List<Sockets> sockets = App.Database.Sockets.ToList();
            List<Manufacturers> manufacturers = App.Database.Manufacturers.ToList();
            List<Memory_types> memoryTypes = App.Database.Memory_types.ToList();

            var combinedData = from processor in processors
                               join socket in sockets on processor.Socket equals socket.SocketId into processorSocketGroup
                               from socketData in processorSocketGroup.DefaultIfEmpty()
                               join manufacturer in manufacturers on processor.Manufacturer_Id equals manufacturer.ManufacturersId into processorManufacturerGroup
                               from manufacturerData in processorManufacturerGroup.DefaultIfEmpty()
                               join memoryType in memoryTypes on processor.Memory_type equals memoryType.Memory_typeId into processorMemoryTypeGroup
                               from memoryTypeData in processorMemoryTypeGroup.DefaultIfEmpty()
                               where App.Database.Motherboards.Any(m => m.Socket == processor.Socket)  // Фильтрация по наличию материнской платы
                               select new CPUCombinedData
                               {
                                   Processor = processor,
                                   Socket = socketData,
                                   Manufacturer = manufacturerData,
                                   MemoryType = memoryTypeData
                               };

            CPUListBox.ItemsSource = combinedData;
        }
        private void CPUSearchInCategoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(CPUListBox.ItemsSource);

            if (view != null)
            {
                string searchText = CPUSearchInCategoryTextBox.Text.Trim().ToLower();
                view.Filter = item =>
                {
                    if (item is CPUCombinedData combinedData)
                    {
                        string processorModel = combinedData.Processor.Model.ToLower();
                        string manufacturerName = combinedData.Manufacturer?.ManufacturersName.ToLower() ?? "";

                        return processorModel.Contains(searchText) || manufacturerName.Contains(searchText);
                    }
                    return false;
                };
            }
        }
        private void CPUSortLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            CPUSortLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6D6D6"));
        }
        private void CPUSortLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            CPUSortLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6DB2E3"));
        }
        bool CPUShowSort = true;
        private void CPUSortLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CPUShowSort)
            {
                CPUSortBorder.Visibility = Visibility.Visible;
            }
            else
            {
                CPUSortBorder.Visibility = Visibility.Collapsed;
            }
            CPUShowSort = !CPUShowSort;
        }
        private void CPUMostCheapestSort_Checked(object sender, RoutedEventArgs e)
        {
            if (CPUListBox != null)
            {
                CPUSortBorder.Visibility = Visibility.Collapsed;
                CPUSortLabel.Content = "сначала недорогие";
                CPUSortListByPrice(ascending: true);
            }
            CPUShowSort = !CPUShowSort;
        }
        private void CPUMostValueableSort_Checked(object sender, RoutedEventArgs e)
        {
            if (CPUListBox != null)
            {
                CPUSortBorder.Visibility = Visibility.Collapsed;
                CPUSortLabel.Content = "сначала дорогие";
                CPUSortListByPrice(ascending: false);
            }
            CPUShowSort = !CPUShowSort;
        }
        private void CPUSortListByPrice(bool ascending)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(CPUListBox.ItemsSource);

            if (view != null)
            {

                List<CPUCombinedData> combineds = view.Cast<CPUCombinedData>().ToList();


                combineds.Sort((a, b) =>
                {
                    decimal? costA = a.Processor.Cost ?? 0m;
                    decimal? costB = b.Processor.Cost ?? 0m;

                    return ascending ? decimal.Compare(costA.Value, costB.Value) : decimal.Compare(costB.Value, costA.Value);
                });

                CPUListBox.ItemsSource = combineds;
            }
        }
        private void CPUListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CPUListBox.SelectedItem != null)
            {
                CPUCombinedData selectedData = (CPUCombinedData)CPUListBox.SelectedItem;
                int selectedProcessorSocketId = selectedData.Socket.SocketId;
                
                MotherboardLoadComponent(selectedProcessorSocketId);
            }
        }
        private void CPUDisplaySelectedData(CPUCombinedData selectedData)
        {
            SelectedCPUImage.Visibility = Visibility.Visible;
            SelectedCPULabelModel.Visibility = Visibility.Visible;
            BitmapImage bitmapImage = ByteArrayToBitmapImage(selectedData.Processor.Preview_Photo);
            SelectedCPUImage.Source = bitmapImage;
            SelectedCPULabelModel.Content = $"{selectedData.Processor.Model}        {selectedData.Processor.Cost:C0}        осталось: {selectedData.Processor.CPU_Count_on_storage} шт";
            MotherboardLoadComponent(selectedData.Socket?.SocketId);
        }
        private void CPUSelectButton_Click(object sender, RoutedEventArgs e)
        {
            CPUSelectRowHeight.Height = new GridLength(2, GridUnitType.Pixel);
            CPUSelectGrid.Visibility = Visibility.Collapsed;
            CPUSelectGridVisible = !CPUSelectGridVisible;
            SelectedCPUImage.Visibility = Visibility.Visible;
            SelectedCPULabelModel.Visibility = Visibility.Visible;
            CPUCombinedData selectedData = ((FrameworkElement)sender).DataContext as CPUCombinedData;

            if (selectedData != null)
            {
                int userId = CurrentUser.Id;

                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    // Получение текущей конфигурации пользователя
                    UserConfiguration existingConfiguration = context.UserConfiguration
                        .FirstOrDefault(config => config.UserId == userId);

                    if (existingConfiguration != null)
                    {
                        // Получение информации о материнской плате из текущей конфигурации
                        MotherboardCombinedData currentMotherboardData = null;
                        if (existingConfiguration.MotherboardId != null)
                        {
                            currentMotherboardData = context.Motherboards
                                .Where(m => m.MotherboardId == existingConfiguration.MotherboardId)
                                .Join(context.Sockets,
                                      mb => mb.Socket,
                                      socket => socket.SocketId,
                                      (mb, socket) => new MotherboardCombinedData
                                      {
                                          Motherboard = mb,
                                          Socket = socket,
                                      })
                                .FirstOrDefault();
                        }

                        if (currentMotherboardData == null || selectedData.Processor.Socket == currentMotherboardData.Socket.SocketId)
                        {
                            // Процессор совместим с материнской платой
                            existingConfiguration.CpuId = selectedData.Processor.CPUId;
                            context.Entry(existingConfiguration).State = EntityState.Modified;
                            context.SaveChanges();

                            CPUDisplaySelectedData(selectedData);
                            ComponentsСompatibilityAllPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9"));
                            ComponentsСompatibilityAll.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9"));
                        }
                        else
                        {
                            // Процессор несовместим с текущей материнской платой
                            MessageBox.Show("Выбранный процессор не совместим с текущей материнской платой.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                            ComponentsСompatibilityAllPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C71919"));
                            ComponentsСompatibilityAll.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C71919"));

                            // Продолжить выполнение кода, даже если процессор несовместим
                            existingConfiguration.CpuId = selectedData.Processor.CPUId;
                            context.Entry(existingConfiguration).State = EntityState.Modified;
                            context.SaveChanges();
                        }
                    }
                }
            }
            UpdateUserData();
        }







        //==============================//
        //         МАТЕРИНКА            //
        //==============================//
        bool MotherboardSelectGridVisible = false;
        private void OpenMotherboardSelectGridButton_Click(object sender, RoutedEventArgs e)
        {
            if (MotherboardSelectGridVisible)
            {
                MotherboardSelectGrid.Visibility = Visibility.Collapsed;
                OpenMotherboardSelectGridButton.Content = "Выбрать";
                MotherboardSelectRowHeight.Height = new GridLength(2, GridUnitType.Pixel);
            }
            else
            {
                MotherboardSelectGrid.Visibility = Visibility.Visible;
                OpenMotherboardSelectGridButton.Content = "Скрыть";
                MotherboardSelectRowHeight.Height = new GridLength(750, GridUnitType.Pixel);
            }
            MotherboardSelectGridVisible = !MotherboardSelectGridVisible;
        }
        public class MotherboardCombinedData
        {
            public Motherboards Motherboard { get; set; }
            public Sockets Socket { get; set; }
            public Memory_types MemoryType { get; set; }
            public Form_Factors FormFactor { get; set; }
        }
        private void MotherboardLoadComponent(int? selectedProcessorSocketId = null)
        {
 
                List<Motherboards> motherboards = App.Database.Motherboards.ToList();
                List<ComputerArchitect.Database.Sockets> sockets = App.Database.Sockets.ToList();
                List<Memory_types> memoryTypes = App.Database.Memory_types.ToList();
                List<Form_Factors> formFactors = App.Database.Form_Factors.ToList();

                var compatibleMotherboards = from motherboard in motherboards
                                             join socket in sockets on motherboard.Socket equals socket.SocketId into motherboardSocketGroup
                                             from socketData in motherboardSocketGroup.DefaultIfEmpty()
                                             join memoryType in memoryTypes on motherboard.Memory_Type equals memoryType.Memory_typeId into motherboardMemoryTypeGroup
                                             from memoryTypeData in motherboardMemoryTypeGroup.DefaultIfEmpty()
                                             join formFactor in formFactors on motherboard.Form_Factor equals formFactor.Form_FactorId into motherboardFormFactorGroup
                                             from formFactorData in motherboardFormFactorGroup.DefaultIfEmpty()
                                             where selectedProcessorSocketId == null || socketData?.SocketId == selectedProcessorSocketId
                                             select new MotherboardCombinedData
                                             {
                                                 Motherboard = motherboard,
                                                 Socket = socketData,
                                                 MemoryType = memoryTypeData,
                                                 FormFactor = formFactorData
                                             };

               
                    MotherboardListBox.ItemsSource = compatibleMotherboards;
             
        }
        private void MotherboardSearchInCategoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(MotherboardListBox.ItemsSource);

            if (view != null)
            {
                string searchText = MotherboardSearchInCategoryTextBox.Text.Trim().ToLower();
                view.Filter = item =>
                {
                    if (item is MotherboardCombinedData combinedData)
                    {
                        string motherboardModel = combinedData.Motherboard.Motherboard_Model.ToLower();
                        string socketName = combinedData.Socket?.SocketName.ToLower() ?? "";

                        return motherboardModel.Contains(searchText) || socketName.Contains(searchText);
                    }
                    return false;
                };
            }
        }
        private void MotherboardSortLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            MotherboardSortLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6D6D6"));
        }
        private void MotherboardSortLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            MotherboardSortLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6DB2E3"));
        }
        bool MotherboardShowSort = true;
        private void MotherboardSortLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (MotherboardShowSort)
            {
                MotherboardSortBorder.Visibility = Visibility.Visible;
            }
            else
            {
                MotherboardSortBorder.Visibility = Visibility.Collapsed;
            }
            MotherboardShowSort = !MotherboardShowSort;
        }
        private void MotherboardMostCheapestSort_Checked(object sender, RoutedEventArgs e)
        {
            if (MotherboardListBox != null)
            {
                MotherboardSortBorder.Visibility = Visibility.Collapsed;
                MotherboardSortLabel.Content = "сначала недорогие";
                MotherboardSortListByPrice(ascending: true);
            }
            MotherboardShowSort = !MotherboardShowSort;
        }
        private void MotherboardMostValueableSort_Checked(object sender, RoutedEventArgs e)
        {
            if (MotherboardListBox != null)
            {
                MotherboardSortBorder.Visibility = Visibility.Collapsed;
                MotherboardSortLabel.Content = "сначала дорогие";
                MotherboardSortListByPrice(ascending: false);
            }
            MotherboardShowSort = !MotherboardShowSort;
        }
        private void MotherboardSortListByPrice(bool ascending)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(MotherboardListBox.ItemsSource);

            if (view != null)
            {

                List<MotherboardCombinedData> combineds = view.Cast<MotherboardCombinedData>().ToList();


                combineds.Sort((a, b) =>
                {
                    decimal? costA = a.Motherboard.Cost ?? 0m;
                    decimal? costB = b.Motherboard.Cost ?? 0m;

                    return ascending ? decimal.Compare(costA.Value, costB.Value) : decimal.Compare(costB.Value, costA.Value);
                });

                MotherboardListBox.ItemsSource = combineds;
            }
        }
        private void MotherboardListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MotherboardListBox.SelectedItem != null)
            {
                MotherboardCombinedData MotherboardselectedData = (MotherboardCombinedData)MotherboardListBox.SelectedItem;
                MotherboardDisplaySelectedData(MotherboardselectedData);
            }
        }
        private void MotherboardDisplaySelectedData(MotherboardCombinedData selectedData)
        {
             SelectedMotherboardImage.Visibility = Visibility.Visible;
             SelectedMotherboardLabelModel.Visibility = Visibility.Visible;
             BitmapImage bitmapImage = ByteArrayToBitmapImage(selectedData.Motherboard.Preview_Photo);
             SelectedMotherboardImage.Source = bitmapImage;
             SelectedMotherboardLabelModel.Content = $"{selectedData.Motherboard.Motherboard_Model}        {selectedData.Motherboard.Cost:C0}        осталось: {selectedData.Motherboard.Motherboard_Count_on_storage} шт";
        }
        private void MotherboardSelectButton_Click(object sender, RoutedEventArgs e)
        {

            MotherboardSelectRowHeight.Height = new GridLength(2, GridUnitType.Pixel);
            MotherboardSelectGrid.Visibility = Visibility.Collapsed;
            MotherboardSelectGridVisible = !MotherboardSelectGridVisible;
            SelectedMotherboardImage.Visibility = Visibility.Visible;
            SelectedMotherboardLabelModel.Visibility = Visibility.Visible;
            MotherboardCombinedData selectedData = ((FrameworkElement)sender).DataContext as MotherboardCombinedData;

            if (selectedData != null)
            {
                int userId = CurrentUser.Id;

                using (var context = new ComputerArchitectDataBaseEntities())
                {

                    UserConfiguration existingConfiguration = context.UserConfiguration
                        .FirstOrDefault(config => config.UserId == userId);

                    if (existingConfiguration != null)
                    {
                        existingConfiguration.MotherboardId = selectedData.Motherboard.MotherboardId;
                        context.Entry(existingConfiguration).State = EntityState.Modified;
                    }
                    else
                    {

                        UserConfiguration newConfiguration = new UserConfiguration
                        {
                            UserId = userId,
                            MotherboardId = selectedData.Motherboard.MotherboardId,
                        };

                        context.Entry(newConfiguration).State = EntityState.Added;
                    }

                    context.SaveChanges();
                }

                MotherboardDisplaySelectedData(selectedData);
            }

            UpdateUserData();
        }

    }
}
