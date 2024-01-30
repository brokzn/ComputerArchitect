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
        private int LevenshteinDistance(string str1, string str2)
        {
            int len1 = str1.Length;
            int len2 = str2.Length;
            int[,] distance = new int[len1 + 1, len2 + 1];

            for (int i = 0; i <= len1; i++)
            {
                for (int j = 0; j <= len2; j++)
                {
                    if (i == 0)
                        distance[i, j] = j;
                    else if (j == 0)
                        distance[i, j] = i;
                    else
                        distance[i, j] = Math.Min(
                            Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                            distance[i - 1, j - 1] + (str1[i - 1] == str2[j - 1] ? 0 : 1)
                        );
                }
            }

            return distance[len1, len2];
        }
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

            CaseLoadComponent();
            CaseMostCheapestSort_Checked(null, null);

            UpdateUserData();
        }



        private void UpdateUserData()
        {
            using (var context = new ComputerArchitectDataBaseEntities())
            {
                int userId = CurrentUser.Id;

                UserConfiguration existingConfiguration = context.UserConfiguration
                    .FirstOrDefault(config => config.UserId == userId);

                if (existingConfiguration != null)
                {
                    CPUCombinedData selectedCPUData = ((IEnumerable<CPUCombinedData>)CPUListBox.ItemsSource)
                        .FirstOrDefault(data => data.Processor.CPUId == existingConfiguration.CpuId);

                    MotherboardCombinedData selectedMotherboardData = ((IEnumerable<MotherboardCombinedData>)MotherboardListBox.ItemsSource)
                        .FirstOrDefault(data => data.Motherboard.MotherboardId == existingConfiguration.MotherboardId);

                    CaseCombinedData selectedCaseData = ((IEnumerable<CaseCombinedData>)CaseListBox.ItemsSource)
                        .FirstOrDefault(data => data.Case.CaseId == existingConfiguration.CaseId);

                    int countOfSelectedComponents = 0;
                    int totalCost = 0;

                    if (selectedCPUData != null)
                    {
                        CPUDisplaySelectedData(selectedCPUData);
                        countOfSelectedComponents++;
                        totalCost += Convert.ToInt32(selectedCPUData.Processor.Cost);
                    }

                    if (selectedMotherboardData != null)
                    {
                        MotherboardDisplaySelectedData(selectedMotherboardData);
                        countOfSelectedComponents++;
                        totalCost += Convert.ToInt32(selectedMotherboardData.Motherboard.Cost);
                    }

                    if (selectedCaseData != null)
                    {
                        CaseDisplaySelectedData(selectedCaseData);
                        countOfSelectedComponents++;
                        totalCost += Convert.ToInt32(selectedCaseData.Case.Cost);
                    }

                    if (selectedCPUData != null && selectedMotherboardData != null
                        && selectedCPUData.Processor.Socket != selectedMotherboardData.Motherboard.Socket)
                    {
                        ComponentsСompatibilityAllPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C71919"));
                        ComponentsСompatibilityAll.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C71919"));
                    }
                    else
                    {
                        ComponentsСompatibilityAllPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9"));
                        ComponentsСompatibilityAll.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9"));
                    }

                    ConfiguratorStatusGrid.Visibility = countOfSelectedComponents > 0 ? Visibility.Visible : Visibility.Collapsed;

                    if (countOfSelectedComponents > 0)
                    {
                        Grid.SetColumnSpan(OneSelectedComponent, countOfSelectedComponents);
                    }
                    else
                    {
                        Grid.SetColumnSpan(OneSelectedComponent, 1);
                    }

                    CountOfImportantComponents.Content = "Обязательные комплектующие - " + countOfSelectedComponents + " из 7";
                    CountOfSelectedComponents.Content = "Всего выбрано: " + countOfSelectedComponents.ToString();

                    ComponentsTotalCostLabel.Content = "Итого: " + totalCost.ToString("N0") + " ₽";
                }
                else
                {
                    ConfiguratorStatusGrid.Visibility = Visibility.Collapsed;
                    CountOfSelectedComponents.Content = "0";
                    Grid.SetColumnSpan(OneSelectedComponent, 1);
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
                    existingConfiguration.CaseId = null;
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

            // Скрыть изображение и информацию о выбранном корпусе
            SelectedCaseImage.Visibility = Visibility.Collapsed;
            SelectedCaseLabelModel.Visibility = Visibility.Collapsed;
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

                        return LevenshteinDistance(processorModel, searchText) <= 2 ||
                               LevenshteinDistance(manufacturerName, searchText) <= 2;
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
                        context.SaveChanges();

                        // Вызов метода для обновления списка корпусов
                        UpdateCaseList(selectedData.Motherboard.Form_Factors?.Form_FactorName);
                    }
                    else
                    {
                        UserConfiguration newConfiguration = new UserConfiguration
                        {
                            UserId = userId,
                            MotherboardId = selectedData.Motherboard.MotherboardId,
                        };

                        context.Entry(newConfiguration).State = EntityState.Added;
                        context.SaveChanges();

                        // Вызов метода для обновления списка корпусов
                        UpdateCaseList(selectedData.Motherboard.Form_Factors?.Form_FactorName);
                    }
                }

                MotherboardDisplaySelectedData(selectedData);
            }

            UpdateUserData();
        }

        private void UpdateCaseList(string motherboardFormFactor)
        {
            // Вызов метода для обновления списка корпусов на основе нового форм-фактора материнской платы
            CaseLoadComponentByMotherboardFormFactor(motherboardFormFactor);
        }

        private void CaseLoadComponentByMotherboardFormFactor(string selectedMotherboardFormFactor)
        {
            var combinedData = (from caseData in App.Database.Cases
                                join caseSizeData in App.Database.CaseSizes
                                on caseData.Case_Size equals caseSizeData.CaseSize_Id
                                where caseData.Compatible_Motherboard_Form_Factors.Contains(selectedMotherboardFormFactor)
                                select new CaseCombinedData
                                {
                                    Case = caseData,
                                    CaseSizes = caseSizeData,
                                }).ToList();

            CaseListBox.ItemsSource = combinedData;
        }





        //==============================//
        //         КОРПУС               //
        //==============================//
        bool CaseSelectGridVisible = false;
        private void OpenCaseSelectGridButton_Click(object sender, RoutedEventArgs e)
        {
            if (CaseSelectGridVisible)
            {
                CaseSelectGrid.Visibility = Visibility.Collapsed;
                OpenCaseSelectGridButton.Content = "Выбрать";
                CaseSelectRowHeight.Height = new GridLength(2, GridUnitType.Pixel);
            }
            else
            {
                CaseSelectGrid.Visibility = Visibility.Visible;
                OpenCaseSelectGridButton.Content = "Скрыть";
                CaseSelectRowHeight.Height = new GridLength(750, GridUnitType.Pixel);
            }
            CaseSelectGridVisible = !CaseSelectGridVisible;
        }
        public class CaseCombinedData
        {
            public Cases Case { get; set; }
            public CaseSizes CaseSizes { get; set; }

            public string Compatible_Motherboard_Form_Factors { get; set; }
        }
        private void CaseLoadComponent(int? selectedMotherboardFormFactorId = null)
        {
            var combinedData = (from caseData in App.Database.Cases
                                join caseSizeData in App.Database.CaseSizes
                                on caseData.Case_Size equals caseSizeData.CaseSize_Id
                                where selectedMotherboardFormFactorId == null ||
                                      caseData.Compatible_Motherboard_Form_Factors.Contains(selectedMotherboardFormFactorId.ToString())
                                select new CaseCombinedData
                                {
                                    Case = caseData,
                                    CaseSizes = caseSizeData,
                                }).ToList();

            CaseListBox.ItemsSource = combinedData;
        }



        private void CaseSearchInCategoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(CaseListBox.ItemsSource);

            if (view != null)
            {
                string searchText = CaseSearchInCategoryTextBox.Text.Trim().ToLower();
                view.Filter = item =>
                {
                    if (item is CaseCombinedData combinedData)
                    {
                        string caseModel = combinedData.Case.Model.ToLower();

                        return caseModel.Contains(searchText);
                    }
                    return false;
                };
            }
        }
        private void CaseSortLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            CaseSortLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6D6D6"));
        }
        private void CaseSortLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            CaseSortLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6DB2E3"));
        }
        bool CaseShowSort = true;
        private void CaseSortLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CaseShowSort)
            {
                CaseSortBorder.Visibility = Visibility.Visible;
            }
            else
            {
                CaseSortBorder.Visibility = Visibility.Collapsed;
            }
            CaseShowSort = !CaseShowSort;
        }
        private void CaseMostCheapestSort_Checked(object sender, RoutedEventArgs e)
        {
            if (CaseListBox != null)
            {
                CaseSortBorder.Visibility = Visibility.Collapsed;
                CaseSortLabel.Content = "сначала недорогие";
                CaseSortListByPrice(ascending: true);
            }
            CaseShowSort = !CaseShowSort;
        }
        private void CaseMostValueableSort_Checked(object sender, RoutedEventArgs e)
        {
            if (CaseListBox != null)
            {
                CaseSortBorder.Visibility = Visibility.Collapsed;
                CaseSortLabel.Content = "сначала дорогие";
                CaseSortListByPrice(ascending: false);
            }
            CaseShowSort = !CaseShowSort;
        }
        private void CaseSortListByPrice(bool ascending)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(CaseListBox.ItemsSource);

            if (view != null)
            {

                List<CaseCombinedData> combineds = view.Cast<CaseCombinedData>().ToList();


                combineds.Sort((a, b) =>
                {
                    decimal? costA = a.Case.Cost ?? 0m;
                    decimal? costB = b.Case.Cost ?? 0m;

                    return ascending ? decimal.Compare(costA.Value, costB.Value) : decimal.Compare(costB.Value, costA.Value);
                });

                CaseListBox.ItemsSource = combineds;
            }
        }



        private void CaseListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CaseListBox.SelectedItem != null)
            {
                CaseCombinedData caseSelectedData = (CaseCombinedData)CaseListBox.SelectedItem;
                CaseDisplaySelectedData(caseSelectedData);
            }
        }

        private void CaseDisplaySelectedData(CaseCombinedData selectedData)
        {
            SelectedCaseImage.Visibility = Visibility.Visible;
            SelectedCaseLabelModel.Visibility = Visibility.Visible;
            BitmapImage bitmapImage = ByteArrayToBitmapImage(selectedData.Case.Preview_Photo);
            SelectedCaseImage.Source = bitmapImage;
            SelectedCaseLabelModel.Content = $"{selectedData.Case.Model}        {selectedData.Case.Cost:C0}        осталось: {selectedData.Case.Cases_Count_on_storage} шт";
        }

        private void CaseSelectButton_Click(object sender, RoutedEventArgs e)
        {
            CaseSelectRowHeight.Height = new GridLength(2, GridUnitType.Pixel);
            CaseSelectGrid.Visibility = Visibility.Collapsed;
            CaseSelectGridVisible = !CaseSelectGridVisible;
            SelectedCaseImage.Visibility = Visibility.Visible;
            SelectedCaseLabelModel.Visibility = Visibility.Visible;
            CaseCombinedData selectedData = ((FrameworkElement)sender).DataContext as CaseCombinedData;

            if (selectedData != null)
            {
                int userId = CurrentUser.Id;

                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    UserConfiguration existingConfiguration = context.UserConfiguration
                        .FirstOrDefault(config => config.UserId == userId);

                    if (existingConfiguration != null)
                    {
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

                        if (currentMotherboardData == null || CheckCaseCompatibility(selectedData.Case.Compatible_Motherboard_Form_Factors, currentMotherboardData.Motherboard.Form_Factors.Form_FactorName))
                        {
                            existingConfiguration.CaseId = selectedData.Case.CaseId;
                            context.Entry(existingConfiguration).State = EntityState.Modified;
                            context.SaveChanges();

                            CaseDisplaySelectedData(selectedData);
                            ComponentsСompatibilityAllPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9"));
                            ComponentsСompatibilityAll.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9"));
                        }
                        else
                        {
                            MessageBox.Show("Выбранный корпус не совместим с форм-фактором текущей материнской платы.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                            ComponentsСompatibilityAllPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C71919"));
                            ComponentsСompatibilityAll.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C71919"));

                            existingConfiguration.CaseId = selectedData.Case.CaseId;
                            context.Entry(existingConfiguration).State = EntityState.Modified;
                            context.SaveChanges();
                        }
                    }
                }
            }

            UpdateUserData();
        }



        private bool CheckCaseCompatibility(string compatibleFormFactors, string motherboardFormFactorName)
        {
            if (string.IsNullOrWhiteSpace(compatibleFormFactors))
            {
                return false;
            }

            var caseFormFactors = compatibleFormFactors.Split(',').Select(ff => ff.Trim());

            using (var context = new ComputerArchitectDataBaseEntities())
            {
                var motherboardFormFactor = context.Form_Factors
                    .Where(ff => ff.Form_FactorName == motherboardFormFactorName)
                    .FirstOrDefault();

                if (motherboardFormFactor != null)
                {
                    var motherboardFormFactorAsString = motherboardFormFactor.Form_FactorName;

                    // Проверяем, если форм-фактор материнской платы совпадает с хотя бы одним из форм-факторов корпуса
                    return caseFormFactors.Any(ff =>
                        LevenshteinDistance(ff, motherboardFormFactorAsString) <= 1 ||
                        (ff.Contains(" ") && LevenshteinDistance(ff.Replace(" ", ""), motherboardFormFactorAsString) <= 1));
                }

                return false;
            }
        }
    }
}
