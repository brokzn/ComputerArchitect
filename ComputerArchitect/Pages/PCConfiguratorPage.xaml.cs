using ComputerArchitect.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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

            GPULoadComponent();
            GPUMostCheapestSort_Checked(null, null);

            FanLoadComponent();
            FanMostCheapestSort_Checked(null, null);

            RAMLoadComponent();
            RAMMostCheapestSort_Checked(null, null);

            MemoryLoadComponent();
            MemoryMostCheapestSort_Checked(null, null);

            PowerSuppliesLoadComponent();
            PowerSuppliesMostCheapestSort_Checked(null, null);

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

                    GPUCombinedData selectedGPUData = ((IEnumerable<GPUCombinedData>)GPUListBox.ItemsSource)
                        .FirstOrDefault(data => data.GPUProcessor.GPUId == existingConfiguration.GPUId);

                    FanCombinedData selectedFanData = ((IEnumerable<FanCombinedData>)FanListBox.ItemsSource)
                        .FirstOrDefault(data => data.Cooler.CoolerId == existingConfiguration.FanId);

                    RAMCombinedData selectedRAMData = ((IEnumerable<RAMCombinedData>)RAMListBox.ItemsSource)
                        .FirstOrDefault(data => data.Rams.RAMId == existingConfiguration.RAMId);

                    MemoryCombinedData selectedMemoryData = ((IEnumerable<MemoryCombinedData>)MemoryListBox.ItemsSource)
                        .FirstOrDefault(data => data.Hdds.HDDId == existingConfiguration.MemoryId);

                    PowerSuppliesCombinedData selectedPowerSuppliesData = ((IEnumerable<PowerSuppliesCombinedData>)PowerSuppliesListBox.ItemsSource)
                        .FirstOrDefault(data => data.Powersupplies.PowerSupplyId == existingConfiguration.PowerSuppliesId);

                    int countOfSelectedComponents = 0;
                    int totalCost = 0;
                    int totalPowerWattUsing = 0;
                    if (selectedCPUData != null)
                    {
                        CPUDisplaySelectedData(selectedCPUData);
                        countOfSelectedComponents++;
                        totalCost += Convert.ToInt32(selectedCPUData.Processor.Cost);
                        totalPowerWattUsing += Convert.ToInt32(selectedCPUData.Processor.Thermal_design_power);
                    }

                    if (selectedMotherboardData != null)
                    {
                        MotherboardDisplaySelectedData(selectedMotherboardData);
                        countOfSelectedComponents++;
                        totalCost += Convert.ToInt32(selectedMotherboardData.Motherboard.Cost);
                        totalPowerWattUsing += 50;
                    }

                    if (selectedCaseData != null)
                    {
                        CaseDisplaySelectedData(selectedCaseData);
                        countOfSelectedComponents++;
                        totalCost += Convert.ToInt32(selectedCaseData.Case.Cost);
                    }

                    if (selectedGPUData != null)
                    {
                        GPUDisplaySelectedData(selectedGPUData);
                        countOfSelectedComponents++;
                        totalCost += Convert.ToInt32(selectedGPUData.GPUProcessor.Cost);
                        totalPowerWattUsing += Convert.ToInt32(selectedGPUData.GPUProcessor.Using_Power_Watt);
                    }

                    if (selectedFanData != null)
                    {
                        FanDisplaySelectedData(selectedFanData);
                        countOfSelectedComponents++;
                        totalCost += Convert.ToInt32(selectedFanData.Cooler.Cost);
                        totalPowerWattUsing += Convert.ToInt32(selectedFanData.Cooler.TDP);
                    }

                    if (selectedRAMData != null)
                    {
                        RAMDisplaySelectedData(selectedRAMData);
                        countOfSelectedComponents++;
                        totalCost += Convert.ToInt32(selectedRAMData.Rams.Cost);
                        totalPowerWattUsing += 7;
                    }

                    if (selectedMemoryData != null)
                    {
                        MemoryDisplaySelectedData(selectedMemoryData);
                        countOfSelectedComponents++;
                        totalCost += Convert.ToInt32(selectedMemoryData.Hdds.Cost);
                        totalPowerWattUsing += Convert.ToInt32(selectedMemoryData.Hdds.Max_Power_Consumption_Watts);
                    }

                    if (selectedPowerSuppliesData != null)
                    {
                        PowerSuppliesDisplaySelectedData(selectedPowerSuppliesData);
                        countOfSelectedComponents++;
                        totalCost += Convert.ToInt32(selectedPowerSuppliesData.Powersupplies.Cost);
                    }


                    //Тип ddr процессора и материнской платы
                    if (selectedCPUData != null && selectedCPUData.Processor != null && selectedCPUData.Processor.Memory_type != null
                        && selectedMotherboardData != null && selectedMotherboardData.Motherboard != null && selectedMotherboardData.Motherboard.Memory_Type != null)
                    {
                        if (selectedCPUData.Processor.Memory_type != selectedMotherboardData.Motherboard.Memory_Type)
                        {
                            SetCompatibilityColors("#C71919");
                        }
                        else
                        {
                            SetCompatibilityColors("#1EBA6F");
                        }
                    }
                    else
                    {

                    }


                    //Сокет процессора и материнской платы
                    if (selectedCPUData != null && selectedCPUData.Processor != null && selectedCPUData.Processor.Socket != null
                         && selectedMotherboardData != null && selectedMotherboardData.Motherboard != null && selectedMotherboardData.Motherboard.Socket != null)
                    {
                        if (selectedCPUData.Processor.Socket != selectedMotherboardData.Motherboard.Socket)
                        {
                            SetCompatibilityColors("#C71919");
                        }
                        else
                        {
                            SetCompatibilityColors("#1EBA6F");
                        }
                    }
                    else
                    {
                        
                    }

                    

                    //Сокет процессора и охлаждения
                    if (selectedCPUData != null && selectedCPUData.Processor != null && selectedCPUData.Processor.Socket != null
                        && selectedFanData != null && selectedFanData.Socket != null && selectedFanData.Socket != null)
                    {
                        if (selectedCPUData.Processor.Socket != selectedFanData.Socket.SocketId)
                        {
                            SetCompatibilityColors("#C71919");
                        }
                        else
                        {
                            SetCompatibilityColors("#1EBA6F");
                        }
                    }
                    else
                    {
                        
                    }



                    //Тип ddr процессора и озу
                    if (selectedCPUData != null && selectedCPUData.Processor != null && selectedCPUData.Processor.Memory_type != null
                        && selectedRAMData != null && selectedRAMData.MemoryType != null && selectedRAMData.MemoryType != null)
                    {
                        if (selectedCPUData.Processor.Memory_type != selectedRAMData.MemoryType.Memory_typeId)
                        {
                            SetCompatibilityColors("#C71919");
                        }
                        else
                        {
                            SetCompatibilityColors("#1EBA6F");
                        }
                    }
                    else
                    {
                        
                    }


                    //Тип ddr материнской платы и озу
                    if (selectedMotherboardData != null && selectedMotherboardData.Motherboard != null && selectedMotherboardData.Motherboard.Memory_Type != null
                     && selectedRAMData != null && selectedRAMData.MemoryType != null && selectedRAMData.MemoryType != null)
                    {
                        if (selectedMotherboardData.Motherboard.Memory_Type != selectedRAMData.MemoryType.Memory_typeId)
                        {
                            SetCompatibilityColors("#C71919");
                        }
                        else
                        {
                            SetCompatibilityColors("#1EBA6F");
                        }
                    }
                    else
                    {
                        
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

                    CountOfImportantComponents.Content = "Обязательные комплектующие - " + countOfSelectedComponents + " из 8";
                    CountOfSelectedComponents.Content = "Всего выбрано: " + countOfSelectedComponents.ToString();

                    ComponentsTotalCostLabel.Content = "Итого: " + totalCost.ToString("N0") + " ₽";

                }

                else
                {
                    ConfiguratorStatusGrid.Visibility = Visibility.Collapsed;
                }
                
            }
        }
   



        void SetCompatibilityColors(string colorHex)
        {
            ComponentsСompatibilityAllPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
            ComponentsСompatibilityAll.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
        }



        private void ClearConfigButton_Click(object sender, RoutedEventArgs e)
        {

            int userId = CurrentUser.Id;

            using (var context = new ComputerArchitectDataBaseEntities())
            {

                UserConfiguration existingConfiguration = context.UserConfiguration
                    .FirstOrDefault(config => config.UserId == userId);

                if (existingConfiguration != null)
                {

                    existingConfiguration.CpuId = null;
                    existingConfiguration.MotherboardId = null;
                    existingConfiguration.CaseId = null;
                    existingConfiguration.GPUId = null;
                    existingConfiguration.FanId = null;
                    existingConfiguration.RAMId = null;
                    existingConfiguration.MemoryId = null;
                    existingConfiguration.PowerSuppliesId = null;

                    context.Entry(existingConfiguration).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }

            // Скрыть изображение и информацию о выбранном процессоре
            SelectedCPUImage.Visibility = Visibility.Collapsed;
            SelectedCPULabelModel.Visibility = Visibility.Collapsed;
            SelectedCPULabelInfo.Visibility = Visibility.Collapsed;
            SelectedCPULabelCost.Visibility = Visibility.Collapsed;
            SelectedCPULabelCount.Visibility = Visibility.Collapsed;
            // Скрыть изображение и информацию о выбранной материнской плате
            SelectedMotherboardImage.Visibility = Visibility.Collapsed;
            SelectedMotherboardLabelModel.Visibility = Visibility.Collapsed;
            SelectedMotherboardLabelInfo.Visibility = Visibility.Collapsed;
            SelectedMotherboardLabelCost.Visibility = Visibility.Collapsed;
            SelectedMotherboardLabelCount.Visibility = Visibility.Collapsed;
            // Скрыть изображение и информацию о выбранном корпусе
            SelectedCaseImage.Visibility = Visibility.Collapsed;
            SelectedCaseLabelModel.Visibility = Visibility.Collapsed;
            SelectedCaseLabelInfo.Visibility = Visibility.Collapsed;
            SelectedCaseLabelCost.Visibility = Visibility.Collapsed;
            SelectedCaseLabelCount.Visibility = Visibility.Collapsed;
            // Скрыть изображение и информацию о выбранной видеокарте
            SelectedGPUImage.Visibility = Visibility.Collapsed;
            SelectedGPULabelModel.Visibility = Visibility.Collapsed;
            SelectedGPULabelInfo.Visibility = Visibility.Collapsed;
            SelectedGPULabelCost.Visibility = Visibility.Collapsed;
            SelectedGPULabelCount.Visibility = Visibility.Collapsed;
            // Скрыть изображение и информацию о выбранном охлаждении
            SelectedFanImage.Visibility = Visibility.Collapsed;
            SelectedFanLabelModel.Visibility = Visibility.Collapsed;
            SelectedFanLabelInfo.Visibility = Visibility.Collapsed;
            SelectedFanLabelCost.Visibility = Visibility.Collapsed;
            SelectedFanLabelCount.Visibility = Visibility.Collapsed;
            // Скрыть изображение и информацию о выбранном озу
            SelectedRAMImage.Visibility = Visibility.Collapsed;
            SelectedRAMLabelModel.Visibility = Visibility.Collapsed;
            SelectedRAMLabelInfo.Visibility = Visibility.Collapsed;
            SelectedRAMLabelCost.Visibility = Visibility.Collapsed;
            SelectedRAMLabelCount.Visibility = Visibility.Collapsed;
            // Скрыть изображение и информацию о выбранном диске
            SelectedMemoryImage.Visibility = Visibility.Collapsed;
            SelectedMemoryLabelModel.Visibility = Visibility.Collapsed;
            SelectedMemoryLabelInfo.Visibility = Visibility.Collapsed;
            SelectedMemoryLabelCost.Visibility = Visibility.Collapsed;
            SelectedMemoryLabelCount.Visibility = Visibility.Collapsed;
            // Скрыть изображение и информацию о выбранном блоке питания
            SelectedPowerSuppliesImage.Visibility = Visibility.Collapsed;
            SelectedPowerSuppliesLabelModel.Visibility = Visibility.Collapsed;
            SelectedPowerSuppliesLabelInfo.Visibility = Visibility.Collapsed;
            SelectedPowerSuppliesLabelCost.Visibility = Visibility.Collapsed;
            SelectedPowerSuppliesLabelCount.Visibility = Visibility.Collapsed;
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

                MotherboardLoadComponent();
            }
        }
        private void CPUDisplaySelectedData(CPUCombinedData selectedData)
        {
            SelectedCPUImage.Visibility = Visibility.Visible;
            SelectedCPULabelModel.Visibility = Visibility.Visible;
            SelectedCPULabelInfo.Visibility = Visibility.Visible;
            SelectedCPULabelCost.Visibility = Visibility.Visible;
            SelectedCPULabelCount.Visibility = Visibility.Visible;
            BitmapImage bitmapImage = ByteArrayToBitmapImage(selectedData.Processor.Preview_Photo);
            SelectedCPUImage.Source = bitmapImage;
            SelectedCPULabelModel.Content = "Процессор " + selectedData.Processor.Model;
            SelectedCPULabelInfo.Content = "[" + selectedData.Processor.Sockets.SocketName
                +", "+ selectedData.Processor.Total_cores + " x " + selectedData.Processor.Base_processor_speed +" ГГц, " +
                selectedData.Processor.Number_of_channels + "x" + selectedData.Processor.Memory_types.Memory_typeName + " " + selectedData.Processor.Memory_speed+ ", Макс. " + selectedData.Processor.Max_supported_memory + " GB RAM" + ", TDP " + selectedData.Processor.Thermal_design_power + " Вт]";

            SelectedCPULabelCost.Content = Convert.ToInt32(selectedData.Processor.Cost).ToString("N0") + " ₽";
            SelectedCPULabelCount.Content = "В наличии: " + selectedData.Processor.CPU_Count_on_storage + " шт.";
        }
        private void CPUSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenCPUSelectGridButton.Content = "Выбрать";
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

                        
                        // Процессор совместим с материнской платой
                        existingConfiguration.CpuId = selectedData.Processor.CPUId;
                        context.Entry(existingConfiguration).State = EntityState.Modified;
                        context.SaveChanges();
                        CPUDisplaySelectedData(selectedData); 
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
        private void MotherboardLoadComponent()
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
            SelectedMotherboardLabelInfo.Visibility = Visibility.Visible;
            SelectedMotherboardLabelCost.Visibility = Visibility.Visible;
            SelectedMotherboardLabelCount.Visibility = Visibility.Visible;
            BitmapImage bitmapImage = ByteArrayToBitmapImage(selectedData.Motherboard.Preview_Photo);
            SelectedMotherboardImage.Source = bitmapImage;
            SelectedMotherboardLabelModel.Content = "Материнская плата " + selectedData.Motherboard.Motherboard_Model;
            SelectedMotherboardLabelInfo.Content = "[" + selectedData.Motherboard.Sockets.SocketName
                + ", " + selectedData.Motherboard.Memory_Channels
                + "x" + selectedData.Motherboard.Memory_types.Memory_typeName
                + " " + selectedData.Motherboard.Max_Memory_Frequency_Mhz
                + ", " + selectedData.Motherboard.Form_Factors.Form_FactorName + "]";
            SelectedMotherboardLabelCost.Content = Convert.ToInt32(selectedData.Motherboard.Cost).ToString("N0") + " ₽";
            SelectedMotherboardLabelCount.Content = "В наличии: " + selectedData.Motherboard.Motherboard_Count_on_storage + " шт.";
        }
        private void MotherboardSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenMotherboardSelectGridButton.Content = "Выбрать";
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

                    
                        existingConfiguration.MotherboardId = selectedData.Motherboard.MotherboardId;
                        context.Entry(existingConfiguration).State = EntityState.Modified;
                        context.SaveChanges();

                }

                MotherboardDisplaySelectedData(selectedData);
            }

            UpdateUserData();
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
            SelectedCaseLabelInfo.Visibility = Visibility.Visible;
            SelectedCaseLabelCost.Visibility = Visibility.Visible;
            SelectedCaseLabelCount.Visibility = Visibility.Visible;
            BitmapImage bitmapImage = ByteArrayToBitmapImage(selectedData.Case.Preview_Photo);
            SelectedCaseImage.Source = bitmapImage;
            SelectedCaseLabelModel.Content = "Корпус " + selectedData.Case.Model;
            SelectedCaseLabelInfo.Content = "[" 
                + selectedData.Case.Compatible_Motherboard_Form_Factors 
                + ", " + "Форм-фактор блока питания "
                + selectedData.Case.Compatible_PSU_Form_Factors + "]";
            SelectedCaseLabelCost.Content = Convert.ToInt32(selectedData.Case.Cost).ToString("N0") + " ₽";
            SelectedCaseLabelCount.Content = "В наличии: " + selectedData.Case.Cases_Count_on_storage + " шт.";

        }
        private void CaseSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenCaseSelectGridButton.Content = "Выбрать";
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

                        
                        existingConfiguration.CaseId = selectedData.Case.CaseId;
                        context.Entry(existingConfiguration).State = EntityState.Modified;
                        context.SaveChanges();

                        CaseDisplaySelectedData(selectedData);  
                    }
                }
            }

            UpdateUserData();
        }
       






        //==============================//
        //         ВИДЕОКАРТА           //
        //==============================//
        bool GPUSelectGridVisible = false;
        private void OpenGPUSelectGridButton_Click(object sender, RoutedEventArgs e)
        {
            if (GPUSelectGridVisible)
            {
                GPUSelectGrid.Visibility = Visibility.Collapsed;
                OpenGPUSelectGridButton.Content = "Выбрать";
                GPUSelectRowHeight.Height = new GridLength(2, GridUnitType.Pixel);
            }
            else
            {
                GPUSelectGrid.Visibility = Visibility.Visible;
                OpenGPUSelectGridButton.Content = "Скрыть";
                GPUSelectRowHeight.Height = new GridLength(750, GridUnitType.Pixel);
            }
            GPUSelectGridVisible = !GPUSelectGridVisible;
        }
        public class GPUCombinedData
        {
            public GPUS GPUProcessor { get; set; }
        }
        private void GPULoadComponent()
        {
            var combinedData = (from GPU in App.Database.GPUS
                                select new GPUCombinedData
                                {
                                    GPUProcessor = GPU,
                                }).ToList();

            GPUListBox.ItemsSource = combinedData;
        }
        private void GPUSearchInCategoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(GPUListBox.ItemsSource);

            if (view != null)
            {
                string searchText = GPUSearchInCategoryTextBox.Text.Trim().ToLower();
                view.Filter = item =>
                {
                    if (item is GPUCombinedData combinedData)
                    {
                        string GPUModel = combinedData.GPUProcessor.GPU_Model.ToLower();

                        return GPUModel.Contains(searchText);
                    }
                    return false;
                };
            }
        }
        private void GPUSortLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            GPUSortLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6D6D6"));
        }
        private void GPUSortLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            GPUSortLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6DB2E3"));
        }
        bool GPUShowSort = true;
        private void GPUSortLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (GPUShowSort)
            {
                GPUSortBorder.Visibility = Visibility.Visible;
            }
            else
            {
                GPUSortBorder.Visibility = Visibility.Collapsed;
            }
            GPUShowSort = !GPUShowSort;
        }
        private void GPUMostCheapestSort_Checked(object sender, RoutedEventArgs e)
        {
            if (GPUListBox != null)
            {
                GPUSortBorder.Visibility = Visibility.Collapsed;
                GPUSortLabel.Content = "сначала недорогие";
                GPUSortListByPrice(ascending: true);
            }
            GPUShowSort = !GPUShowSort;
        }
        private void GPUMostValueableSort_Checked(object sender, RoutedEventArgs e)
        {
            if (GPUListBox != null)
            {
                GPUSortBorder.Visibility = Visibility.Collapsed;
                GPUSortLabel.Content = "сначала дорогие";
                GPUSortListByPrice(ascending: false);
            }
            GPUShowSort = !GPUShowSort;
        }
        private void GPUSortListByPrice(bool ascending)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(GPUListBox.ItemsSource);

            if (view != null)
            {

                List<GPUCombinedData> combineds = view.Cast<GPUCombinedData>().ToList();


                combineds.Sort((a, b) =>
                {
                    decimal? costA = a.GPUProcessor.Cost ?? 0m;
                    decimal? costB = b.GPUProcessor.Cost ?? 0m;

                    return ascending ? decimal.Compare(costA.Value, costB.Value) : decimal.Compare(costB.Value, costA.Value);
                });

                GPUListBox.ItemsSource = combineds;
            }
        }
        private void GPUListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GPUListBox.SelectedItem != null)
            {
                GPUCombinedData gpuSelectedData = (GPUCombinedData)GPUListBox.SelectedItem;
                GPUDisplaySelectedData(gpuSelectedData);
            }
        }
        private void GPUDisplaySelectedData(GPUCombinedData selectedData)
        {
            SelectedGPUImage.Visibility = Visibility.Visible;
            SelectedGPULabelModel.Visibility = Visibility.Visible;
            SelectedGPULabelInfo.Visibility = Visibility.Visible;
            SelectedGPULabelCost.Visibility = Visibility.Visible;
            SelectedGPULabelCount.Visibility = Visibility.Visible;
            BitmapImage bitmapImage = ByteArrayToBitmapImage(selectedData.GPUProcessor.Preview_Photo);
            SelectedGPUImage.Source = bitmapImage;
            SelectedGPULabelModel.Content = "Видеокарта " + selectedData.GPUProcessor.GPU_Model;
            SelectedGPULabelInfo.Content = "["
                + selectedData.GPUProcessor.Connection_Interface
                + " " + selectedData.GPUProcessor.Video_Memory_Size_GB
                + " ГБ " + selectedData.GPUProcessor.Video_Memory_Type
                + ", " + selectedData.GPUProcessor.Memory_Bus_Width_Bits
                + " бит, " + selectedData.GPUProcessor.Video_Output_Type_and_Count
                + ", GPU " + selectedData.GPUProcessor.GPU_Base_Frequency_MHz + " Мгц]";
            SelectedGPULabelCost.Content = Convert.ToInt32(selectedData.GPUProcessor.Cost).ToString("N0") + " ₽";
            SelectedGPULabelCount.Content = "В наличии: " + selectedData.GPUProcessor.GPU_Count_on_storage + " шт.";
        }
        private void GPUSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenGPUSelectGridButton.Content = "Выбрать";
            GPUSelectRowHeight.Height = new GridLength(2, GridUnitType.Pixel);
            GPUSelectGrid.Visibility = Visibility.Collapsed;
            GPUSelectGridVisible = !GPUSelectGridVisible;
            SelectedGPUImage.Visibility = Visibility.Visible;
            SelectedGPULabelModel.Visibility = Visibility.Visible;
            GPUCombinedData selectedData = ((FrameworkElement)sender).DataContext as GPUCombinedData;

            if (selectedData != null)
            {
                int userId = CurrentUser.Id;

                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    UserConfiguration existingConfiguration = context.UserConfiguration
                        .FirstOrDefault(config => config.UserId == userId);

                    if (existingConfiguration != null)
                    {
                        existingConfiguration.GPUId = selectedData.GPUProcessor.GPUId;
                        context.Entry(existingConfiguration).State = EntityState.Modified;
                        context.SaveChanges();

                        GPUDisplaySelectedData(selectedData);
                        ComponentsСompatibilityAllPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9"));
                        ComponentsСompatibilityAll.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9"));
                    }
                }
            }

            UpdateUserData();
        }





        //==============================//
        //         ОХЛАЖДЕНИЕ           //
        //==============================//
        bool FanSelectGridVisible = false;
        private void OpenFanSelectGridButton_Click(object sender, RoutedEventArgs e)
        {
            if (FanSelectGridVisible)
            {
                FanSelectGrid.Visibility = Visibility.Collapsed;
                OpenFanSelectGridButton.Content = "Выбрать";
                FanSelectRowHeight.Height = new GridLength(2, GridUnitType.Pixel);
            }
            else
            {
                FanSelectGrid.Visibility = Visibility.Visible;
                OpenFanSelectGridButton.Content = "Скрыть";
                FanSelectRowHeight.Height = new GridLength(750, GridUnitType.Pixel);
            }
            FanSelectGridVisible = !FanSelectGridVisible;
        }
        public class FanCombinedData
        {
            public Coolers Cooler { get; set; }
            public Sockets Socket { get; set; }
        }
        private void FanLoadComponent()
        {
            List<Coolers> coolers = App.Database.Coolers.ToList();
            List<Sockets> sockets = App.Database.Sockets.ToList();

            var combinedData = from cooler in coolers
                               join socket in sockets on cooler.Socket_Type equals socket.SocketId into coolerSocketGroup
                               from socketData in coolerSocketGroup.DefaultIfEmpty()
                               select new FanCombinedData
                               {
                                   Cooler = cooler,
                                   Socket = socketData
                               };

            FanListBox.ItemsSource = combinedData;
        }
        private void FanSearchInCategoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(FanListBox.ItemsSource);

            if (view != null)
            {
                string searchText = FanSearchInCategoryTextBox.Text.Trim().ToLower();
                view.Filter = item =>
                {
                    if (item is FanCombinedData combinedData)
                    {
                        string coolerModel = combinedData.Cooler.Model.ToLower();

                        return coolerModel.Contains(searchText);
                    }
                    return false;
                };
            }
        }
        private void FanSortLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            FanSortLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6D6D6"));
        }
        private void FanSortLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            FanSortLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6DB2E3"));
        }
        bool FanShowSort = true;
        private void FanSortLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (FanShowSort)
            {
                FanSortBorder.Visibility = Visibility.Visible;
            }
            else
            {
                FanSortBorder.Visibility = Visibility.Collapsed;
            }
            FanShowSort = !FanShowSort;
        }
        private void FanMostCheapestSort_Checked(object sender, RoutedEventArgs e)
        {
            if (FanListBox != null)
            {
                FanSortBorder.Visibility = Visibility.Collapsed;
                FanSortLabel.Content = "сначала недорогие";
                FanSortListByPrice(ascending: true);
            }
            FanShowSort = !FanShowSort;
        }
        private void FanMostValueableSort_Checked(object sender, RoutedEventArgs e)
        {
            if (FanListBox != null)
            {
                FanSortBorder.Visibility = Visibility.Collapsed;
                FanSortLabel.Content = "сначала дорогие";
                FanSortListByPrice(ascending: false);
            }
            FanShowSort = !FanShowSort;
        }
        private void FanSortListByPrice(bool ascending)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(FanListBox.ItemsSource);

            if (view != null)
            {

                List<FanCombinedData> combineds = view.Cast<FanCombinedData>().ToList();


                combineds.Sort((a, b) =>
                {
                    decimal? costA = a.Cooler.Cost ?? 0m;
                    decimal? costB = b.Cooler.Cost ?? 0m;

                    return ascending ? decimal.Compare(costA.Value, costB.Value) : decimal.Compare(costB.Value, costA.Value);
                });

                FanListBox.ItemsSource = combineds;
            }
        }
        private void FanListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FanListBox.SelectedItem != null)
            {
                FanCombinedData fanSelectedData = (FanCombinedData)FanListBox.SelectedItem;
                FanDisplaySelectedData(fanSelectedData);
            }
        }
        private void FanDisplaySelectedData(FanCombinedData selectedData)
        {
            SelectedFanImage.Visibility = Visibility.Visible;
            SelectedFanLabelModel.Visibility = Visibility.Visible;
            SelectedFanLabelInfo.Visibility = Visibility.Visible;
            SelectedFanLabelCost.Visibility = Visibility.Visible;
            SelectedFanLabelCount.Visibility = Visibility.Visible;
            BitmapImage bitmapImage = ByteArrayToBitmapImage(selectedData.Cooler.Preview_Photo);
            SelectedFanImage.Source = bitmapImage;
            SelectedFanLabelModel.Content = "Кулер " + selectedData.Cooler.Model;
            SelectedFanLabelInfo.Content = "[основание - " + selectedData.Cooler.Base_Material
                + ", " + selectedData.Cooler.Max_Fan_Speed_RPM + " об/мин, "
                + selectedData.Cooler.Sockets.SocketName + ", "
                + selectedData.Cooler.Fan_Connector_Type + ", "
                + selectedData.Cooler.TDP + " Вт]";
                
            SelectedFanLabelCost.Content = Convert.ToInt32(selectedData.Cooler.Cost).ToString("N0") + " ₽";
            SelectedFanLabelCount.Content = "В наличии: " + selectedData.Cooler.Cooler_Count_on_storage + " шт.";
        }
        private void FanSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFanSelectGridButton.Content = "Выбрать";
            FanSelectRowHeight.Height = new GridLength(2, GridUnitType.Pixel);
            FanSelectGrid.Visibility = Visibility.Collapsed;
            FanSelectGridVisible = !FanSelectGridVisible;
            SelectedFanImage.Visibility = Visibility.Visible;
            SelectedFanLabelModel.Visibility = Visibility.Visible;
            FanCombinedData selectedData = ((FrameworkElement)sender).DataContext as FanCombinedData;

            if (selectedData != null)
            {
                int userId = CurrentUser.Id;

                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    UserConfiguration existingConfiguration = context.UserConfiguration
                        .FirstOrDefault(config => config.UserId == userId);

                    if (existingConfiguration != null)
                    {
                        existingConfiguration.FanId = selectedData.Cooler.CoolerId;
                        context.Entry(existingConfiguration).State = EntityState.Modified;
                        context.SaveChanges();

                        FanDisplaySelectedData(selectedData);
                        ComponentsСompatibilityAllPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9"));
                        ComponentsСompatibilityAll.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9"));
                    }
                }
            }

            UpdateUserData();
        }


        //==============================//
        //            ОЗУ               //
        //==============================//
        bool RAMSelectGridVisible = false;
        private void OpenRAMSelectGridButton_Click(object sender, RoutedEventArgs e)
        {
            if (RAMSelectGridVisible)
            {
                RAMSelectGrid.Visibility = Visibility.Collapsed;
                OpenRAMSelectGridButton.Content = "Выбрать";
                RAMSelectRowHeight.Height = new GridLength(2, GridUnitType.Pixel);
            }
            else
            {
                RAMSelectGrid.Visibility = Visibility.Visible;
                OpenRAMSelectGridButton.Content = "Скрыть";
                RAMSelectRowHeight.Height = new GridLength(750, GridUnitType.Pixel);
            }
            RAMSelectGridVisible = !RAMSelectGridVisible;
        }
        public class RAMCombinedData
        {
            public RAMS Rams { get; set; }
            public Memory_types MemoryType { get; set; }
        }
        private void RAMLoadComponent()
        {
            List<RAMS> ramsList = App.Database.RAMS.ToList();
            List<Memory_types> memoryTypes = App.Database.Memory_types.ToList();

            var combinedData = from ram in ramsList
                               join memoryType in memoryTypes on ram.Memory_Type equals memoryType.Memory_typeId into ramMemoryTypeGroup
                               from memoryTypeData in ramMemoryTypeGroup.DefaultIfEmpty()
                               select new RAMCombinedData
                               {
                                   Rams = ram,
                                   MemoryType = memoryTypeData
                               };

            RAMListBox.ItemsSource = combinedData;
        }
        private void RAMSearchInCategoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(RAMListBox.ItemsSource);

            if (view != null)
            {
                string searchText = RAMSearchInCategoryTextBox.Text.Trim().ToLower();
                view.Filter = item =>
                {
                    if (item is RAMCombinedData combinedData)
                    {
                        string ramModel = combinedData.Rams.RAM_Model.ToLower();

                        return ramModel.Contains(searchText);
                    }
                    return false;
                };
            }
        }
        private void RAMSortLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            RAMSortLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6D6D6"));
        }
        private void RAMSortLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            RAMSortLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6DB2E3"));
        }
        bool RAMShowSort = true;
        private void RAMSortLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (RAMShowSort)
            {
                RAMSortBorder.Visibility = Visibility.Visible;
            }
            else
            {
                RAMSortBorder.Visibility = Visibility.Collapsed;
            }
            RAMShowSort = !RAMShowSort;
        }
        private void RAMMostCheapestSort_Checked(object sender, RoutedEventArgs e)
        {
            if (RAMListBox != null)
            {
                RAMSortBorder.Visibility = Visibility.Collapsed;
                RAMSortLabel.Content = "сначала недорогие";
                RAMSortListByPrice(ascending: true);
            }
            RAMShowSort = !RAMShowSort;
        }
        private void RAMMostValueableSort_Checked(object sender, RoutedEventArgs e)
        {
            if (RAMListBox != null)
            {
                RAMSortBorder.Visibility = Visibility.Collapsed;
                RAMSortLabel.Content = "сначала дорогие";
                RAMSortListByPrice(ascending: false);
            }
            RAMShowSort = !RAMShowSort;
        }
        private void RAMSortListByPrice(bool ascending)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(RAMListBox.ItemsSource);

            if (view != null)
            {

                List<RAMCombinedData> combineds = view.Cast<RAMCombinedData>().ToList();


                combineds.Sort((a, b) =>
                {
                    decimal? costA = a.Rams.Cost ?? 0m;
                    decimal? costB = b.Rams.Cost ?? 0m;

                    return ascending ? decimal.Compare(costA.Value, costB.Value) : decimal.Compare(costB.Value, costA.Value);
                });

                RAMListBox.ItemsSource = combineds;
            }
        }
        private void RAMListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RAMListBox.SelectedItem != null)
            {
                RAMCombinedData ramSelectedData = (RAMCombinedData)RAMListBox.SelectedItem;
                RAMDisplaySelectedData(ramSelectedData);
            }
        }
        private void RAMDisplaySelectedData(RAMCombinedData selectedData)
        {
            SelectedRAMImage.Visibility = Visibility.Visible;
            SelectedRAMLabelModel.Visibility = Visibility.Visible;
            SelectedRAMLabelInfo.Visibility = Visibility.Visible;
            SelectedRAMLabelCost.Visibility = Visibility.Visible;
            SelectedRAMLabelCount.Visibility = Visibility.Visible;
            BitmapImage bitmapImage = ByteArrayToBitmapImage(selectedData.Rams.Preview_Photo);
            SelectedRAMImage.Source = bitmapImage;
            SelectedRAMLabelModel.Content = "Оперативная память " + selectedData.Rams.RAM_Model;
            SelectedRAMLabelInfo.Content = "["
                + selectedData.Rams.Memory_types.Memory_typeName
                + ", " + selectedData.Rams.Capacity_GB + " ГБx1 шт, "
                + selectedData.Rams.RAM_Speed_MHz + " МГц, "
                + selectedData.Rams.CAS_Latency + "-"
                + selectedData.Rams.RAS_to_CAS_Delay + "-"
                + selectedData.Rams.Row_Precharge_Delay + "-"
                + selectedData.Rams.Activate_to_Precharge_Delay + "]";
            SelectedRAMLabelCost.Content = Convert.ToInt32(selectedData.Rams.Cost).ToString("N0") + " ₽";
            SelectedRAMLabelCount.Content = "В наличии: " + selectedData.Rams.RAM_Count_on_storage + " шт.";
        }
        private void RAMSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenRAMSelectGridButton.Content = "Выбрать";
            RAMSelectRowHeight.Height = new GridLength(2, GridUnitType.Pixel);
            RAMSelectGrid.Visibility = Visibility.Collapsed;
            RAMSelectGridVisible = !RAMSelectGridVisible;
            SelectedRAMImage.Visibility = Visibility.Visible;
            SelectedRAMLabelModel.Visibility = Visibility.Visible;
            RAMCombinedData selectedData = ((FrameworkElement)sender).DataContext as RAMCombinedData;

            if (selectedData != null)
            {
                int userId = CurrentUser.Id;

                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    UserConfiguration existingConfiguration = context.UserConfiguration
                        .FirstOrDefault(config => config.UserId == userId);

                    if (existingConfiguration != null)
                    {
                        existingConfiguration.RAMId = selectedData.Rams.RAMId;
                        context.Entry(existingConfiguration).State = EntityState.Modified;
                        context.SaveChanges();

                        RAMDisplaySelectedData(selectedData);
                        ComponentsСompatibilityAllPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9"));
                        ComponentsСompatibilityAll.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9"));
                    }
                }
            }

            UpdateUserData();

        }




        //==============================//
        //       ХРАНЕНИЕ ДАННЫХ        //
        //==============================//
        bool MemorySelectGridVisible = false;
        private void OpenMemorySelectGridButton_Click(object sender, RoutedEventArgs e)
        {
            if (MemorySelectGridVisible)
            {
                MemorySelectGrid.Visibility = Visibility.Collapsed;
                OpenMemorySelectGridButton.Content = "Выбрать";
                MemorySelectRowHeight.Height = new GridLength(2, GridUnitType.Pixel);
            }
            else
            {
                MemorySelectGrid.Visibility = Visibility.Visible;
                OpenMemorySelectGridButton.Content = "Скрыть";
                MemorySelectRowHeight.Height = new GridLength(750, GridUnitType.Pixel);
            }
            MemorySelectGridVisible = !MemorySelectGridVisible;
        }
        public class MemoryCombinedData
        {
            public HDDs Hdds { get; set; }
        }
        private void MemoryLoadComponent()
        {
            List<HDDs> hddsList = App.Database.HDDs.ToList();

            var combinedData = hddsList.Select(hdd => new MemoryCombinedData { Hdds = hdd });

            MemoryListBox.ItemsSource = combinedData;
        }
        private void MemorySearchInCategoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(MemoryListBox.ItemsSource);

            if (view != null)
            {
                string searchText = MemorySearchInCategoryTextBox.Text.Trim().ToLower();
                view.Filter = item =>
                {
                    if (item is MemoryCombinedData combinedData)
                    {
                        string hddModel = combinedData.Hdds.Model.ToLower();
                        return hddModel.Contains(searchText);
                    }
                    return false;
                };
            }
        }
        private void MemorySortLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            MemorySortLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6D6D6"));
        }
        private void MemorySortLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            MemorySortLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6DB2E3"));
        }
        bool MemoryShowSort = true;
        private void MemorySortLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (MemoryShowSort)
            {
                MemorySortBorder.Visibility = Visibility.Visible;
            }
            else
            {
                MemorySortBorder.Visibility = Visibility.Collapsed;
            }
            MemoryShowSort = !MemoryShowSort;
        }
        private void MemoryMostCheapestSort_Checked(object sender, RoutedEventArgs e)
        {
            if (MemoryListBox != null)
            {
                MemorySortBorder.Visibility = Visibility.Collapsed;
                MemorySortLabel.Content = "сначала недорогие";
                MemorySortListByPrice(ascending: true);
            }
            MemoryShowSort = !MemoryShowSort;
        }
        private void MemoryMostValueableSort_Checked(object sender, RoutedEventArgs e)
        {
            if (MemoryListBox != null)
            {
                MemorySortBorder.Visibility = Visibility.Collapsed;
                MemorySortLabel.Content = "сначала дорогие";
                MemorySortListByPrice(ascending: false);
            }
            MemoryShowSort = !MemoryShowSort;
        }
        private void MemorySortListByPrice(bool ascending)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(MemoryListBox.ItemsSource);

            if (view != null)
            {

                List<MemoryCombinedData> combineds = view.Cast<MemoryCombinedData>().ToList();


                combineds.Sort((a, b) =>
                {
                    decimal? costA = a.Hdds.Cost ?? 0m;
                    decimal? costB = b.Hdds.Cost ?? 0m;

                    return ascending ? decimal.Compare(costA.Value, costB.Value) : decimal.Compare(costB.Value, costA.Value);
                });

                MemoryListBox.ItemsSource = combineds;
            }
        }
        private void MemoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MemoryListBox.SelectedItem != null)
            {
                MemoryCombinedData memorySelectedData = (MemoryCombinedData)MemoryListBox.SelectedItem;
                MemoryDisplaySelectedData(memorySelectedData);
            }
        }
        private void MemoryDisplaySelectedData(MemoryCombinedData selectedData)
        {
            SelectedMemoryImage.Visibility = Visibility.Visible;
            SelectedMemoryLabelModel.Visibility = Visibility.Visible;
            SelectedMemoryLabelInfo.Visibility = Visibility.Visible;
            SelectedMemoryLabelCost.Visibility = Visibility.Visible;
            SelectedMemoryLabelCount.Visibility = Visibility.Visible;
            BitmapImage bitmapImage = ByteArrayToBitmapImage(selectedData.Hdds.Preview_Photo);
            SelectedMemoryImage.Source = bitmapImage;
            SelectedMemoryLabelModel.Content = selectedData.Hdds.Capacity_TB+ " ТБ Жесткий диск " + selectedData.Hdds.Model;
            SelectedMemoryLabelInfo.Content = "["
                + selectedData.Hdds.Interface + ", "
                + Convert.ToInt32(selectedData.Hdds.Interface_Bandwidth_GBps).ToString("N0") + " Гбит/с, "
                + selectedData.Hdds.Spindle_Speed_RPM + " об/мин, кэш память - "
                + selectedData.Hdds.Cache_MB + " МБ]";
            SelectedMemoryLabelCost.Content = Convert.ToInt32(selectedData.Hdds.Cost).ToString("N0") + " ₽";
            SelectedMemoryLabelCount.Content = "В наличии: " + selectedData.Hdds.HDD_Count_on_storage + " шт.";
        }
        private void MemorySelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenMemorySelectGridButton.Content = "Выбрать";
            MemorySelectRowHeight.Height = new GridLength(2, GridUnitType.Pixel);
            MemorySelectGrid.Visibility = Visibility.Collapsed;
            MemorySelectGridVisible = !MemorySelectGridVisible;
            SelectedMemoryImage.Visibility = Visibility.Visible;
            SelectedMemoryLabelModel.Visibility = Visibility.Visible;
            MemoryCombinedData selectedData = ((FrameworkElement)sender).DataContext as MemoryCombinedData;

            if (selectedData != null)
            {
                int userId = CurrentUser.Id;

                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    UserConfiguration existingConfiguration = context.UserConfiguration
                        .FirstOrDefault(config => config.UserId == userId);

                    if (existingConfiguration != null)
                    {
                        existingConfiguration.MemoryId = selectedData.Hdds.HDDId;
                        context.Entry(existingConfiguration).State = EntityState.Modified;
                        context.SaveChanges();

                        MemoryDisplaySelectedData(selectedData);
                        ComponentsСompatibilityAllPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9"));
                        ComponentsСompatibilityAll.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9"));
                    }
                }
            }

            UpdateUserData();
        }



        //==============================//
        //       БЛОК ПИТАНИЯ           //
        //==============================//
        bool PowerSuppliesSelectGridVisible = false;
        private void OpenPowerSuppliesSelectGridButton_Click(object sender, RoutedEventArgs e)
        {
            if (PowerSuppliesSelectGridVisible)
            {
                PowerSuppliesSelectGrid.Visibility = Visibility.Collapsed;
                OpenPowerSuppliesSelectGridButton.Content = "Выбрать";
                PowerSuppliesSelectRowHeight.Height = new GridLength(2, GridUnitType.Pixel);
            }
            else
            {
                PowerSuppliesSelectGrid.Visibility = Visibility.Visible;
                OpenPowerSuppliesSelectGridButton.Content = "Скрыть";
                PowerSuppliesSelectRowHeight.Height = new GridLength(750, GridUnitType.Pixel);
            }
            PowerSuppliesSelectGridVisible = !PowerSuppliesSelectGridVisible;
        }
        public class PowerSuppliesCombinedData
        {
            public PowerSupplies Powersupplies { get; set; }
        }
        private void PowerSuppliesLoadComponent()
        {
            List<PowerSupplies> powerSuppliesList = App.Database.PowerSupplies.ToList();

            var combinedData = from powerSupply in powerSuppliesList
                               select new PowerSuppliesCombinedData
                               {
                                   Powersupplies = powerSupply
                               };

            PowerSuppliesListBox.ItemsSource = combinedData;
        }
        private void PowerSuppliesSearchInCategoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(PowerSuppliesListBox.ItemsSource);

            if (view != null)
            {
                string searchText = PowerSuppliesSearchInCategoryTextBox.Text.Trim().ToLower();
                view.Filter = item =>
                {
                    if (item is PowerSuppliesCombinedData combinedData)
                    {
                        string powerSupplyModel = combinedData.Powersupplies.Model.ToLower();

                        return powerSupplyModel.Contains(searchText);
                    }
                    return false;
                };
            }
        }
        private void PowerSuppliesSortLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            PowerSuppliesSortLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6D6D6"));
        }
        private void PowerSuppliesSortLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            PowerSuppliesSortLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6DB2E3"));
        }
        bool PowerSuppliesShowSort = true;
        private void PowerSuppliesSortLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (PowerSuppliesShowSort)
            {
                PowerSuppliesSortBorder.Visibility = Visibility.Visible;
            }
            else
            {
                PowerSuppliesSortBorder.Visibility = Visibility.Collapsed;
            }
            PowerSuppliesShowSort = !PowerSuppliesShowSort;
        }
        private void PowerSuppliesMostCheapestSort_Checked(object sender, RoutedEventArgs e)
        {
            if (PowerSuppliesListBox != null)
            {
                PowerSuppliesSortBorder.Visibility = Visibility.Collapsed;
                PowerSuppliesSortLabel.Content = "сначала недорогие";
                PowerSuppliesSortListByPrice(ascending: true);
            }
            PowerSuppliesShowSort = !PowerSuppliesShowSort;
        }
        private void PowerSuppliesMostValueableSort_Checked(object sender, RoutedEventArgs e)
        {
            if (PowerSuppliesListBox != null)
            {
                PowerSuppliesSortBorder.Visibility = Visibility.Collapsed;
                PowerSuppliesSortLabel.Content = "сначала дорогие";
                PowerSuppliesSortListByPrice(ascending: false);
            }
            PowerSuppliesShowSort = !PowerSuppliesShowSort;
        }
        private void PowerSuppliesSortListByPrice(bool ascending)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(PowerSuppliesListBox.ItemsSource);

            if (view != null)
            {

                List<PowerSuppliesCombinedData> combineds = view.Cast<PowerSuppliesCombinedData>().ToList();


                combineds.Sort((a, b) =>
                {
                    decimal? costA = a.Powersupplies.Cost ?? 0m;
                    decimal? costB = b.Powersupplies.Cost ?? 0m;

                    return ascending ? decimal.Compare(costA.Value, costB.Value) : decimal.Compare(costB.Value, costA.Value);
                });

                PowerSuppliesListBox.ItemsSource = combineds;
            }
        }
        private void PowerSuppliesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PowerSuppliesListBox.SelectedItem != null)
            {
                PowerSuppliesCombinedData powersuppliesSelectedData = (PowerSuppliesCombinedData)PowerSuppliesListBox.SelectedItem;
                PowerSuppliesDisplaySelectedData(powersuppliesSelectedData);
            }
        }
        private void PowerSuppliesDisplaySelectedData(PowerSuppliesCombinedData selectedData)
        {
            SelectedPowerSuppliesImage.Visibility = Visibility.Visible;
            SelectedPowerSuppliesLabelModel.Visibility = Visibility.Visible;
            SelectedPowerSuppliesLabelInfo.Visibility = Visibility.Visible;
            SelectedPowerSuppliesLabelCost.Visibility = Visibility.Visible;
            SelectedPowerSuppliesLabelCount.Visibility = Visibility.Visible;
            BitmapImage bitmapImage = ByteArrayToBitmapImage(selectedData.Powersupplies.Preview_Photo);
            SelectedPowerSuppliesImage.Source = bitmapImage;
            SelectedPowerSuppliesLabelModel.Content = "Блок питания " + selectedData.Powersupplies.Model;
            SelectedPowerSuppliesLabelInfo.Content = "[" 
                + selectedData.Powersupplies.Power_Watt + " Вт, " 
                + selectedData.Powersupplies.Main_Power_Connector + ", " 
                + selectedData.Powersupplies.CPU_Power_Connector + ", " 
                + selectedData.Powersupplies.SATA_Connector_Count + " SATA]";
            SelectedPowerSuppliesLabelCost.Content = Convert.ToInt32(selectedData.Powersupplies.Cost).ToString("N0") + " ₽";
            SelectedPowerSuppliesLabelCount.Content = "В наличии: " + selectedData.Powersupplies.PS__Count_on_storage + " шт.";
        }
        private void PowerSuppliesSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenPowerSuppliesSelectGridButton.Content = "Выбрать";
            PowerSuppliesSelectRowHeight.Height = new GridLength(2, GridUnitType.Pixel);
            PowerSuppliesSelectGrid.Visibility = Visibility.Collapsed;
            PowerSuppliesSelectGridVisible = !PowerSuppliesSelectGridVisible;
            SelectedPowerSuppliesImage.Visibility = Visibility.Visible;
            SelectedPowerSuppliesLabelModel.Visibility = Visibility.Visible;
            PowerSuppliesCombinedData selectedData = ((FrameworkElement)sender).DataContext as PowerSuppliesCombinedData;

            if (selectedData != null)
            {
                int userId = CurrentUser.Id;

                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    UserConfiguration existingConfiguration = context.UserConfiguration
                        .FirstOrDefault(config => config.UserId == userId);

                    if (existingConfiguration != null)
                    {
                        existingConfiguration.PowerSuppliesId = selectedData.Powersupplies.PowerSupplyId;
                        context.Entry(existingConfiguration).State = EntityState.Modified;
                        context.SaveChanges();

                        PowerSuppliesDisplaySelectedData(selectedData);
                        ComponentsСompatibilityAllPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9"));
                        ComponentsСompatibilityAll.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9"));
                    }
                }
            }

            UpdateUserData();
        }
    }
}
