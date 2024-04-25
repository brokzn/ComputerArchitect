using ComputerArchitect.Database;
using ComputerArchitect.UI.Pages;
using ControlzEx.Standard;
using ComputerArchitect.Pages;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    public partial class CPUPage : Page
    {
        //ОБНОВЛЕНИЕ ИНФОРМАЦИИ О КОРЗИНЕ
        public event EventHandler CartUpdated;
        //ДОБАВЛЕНИЕ В КОРЗИНУ
        private UsersCarts currentUserCart;
        public Users CurrentUser { get; set; }
        public CPUPage(Users currentUser)
        {
            CurrentUser = currentUser;
            InitializeComponent();
            LoadComponent();
            MostCheapestSort_Checked(null, null);
            NewCPUSoketComboBox.ItemsSource = App.Database.Sockets.ToList();
            NewCPUMemoryTypeComboBox.ItemsSource = App.Database.Memory_types.ToList();

            switch (CurrentUser.RoleId)
            {
                case 1:
                    AddNewCpuButton.Visibility = Visibility.Visible;
                    break;
            }
        }

        public class CombinedData
        {
            public CPUS Processor { get; set; }
            public Sockets Socket { get; set; }
            public Manufacturers Manufacturer { get; set; }
            public Memory_types MemoryType { get; set; }
        }
        double minValue;
        double maxValue;
        private void LoadComponent()
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
                               select new CombinedData
                               {
                                   Processor = processor,
                                   Socket = socketData,
                                   Manufacturer = manufacturerData,
                                   MemoryType = memoryTypeData
                               };


            minValue = (double)combinedData.Min(item => item.Processor.Cost.GetValueOrDefault());
            MinPrice.Tag = "от " + minValue.ToString();
            maxValue = (double)combinedData.Max(item => item.Processor.Cost.GetValueOrDefault());
            MaxPrice.Tag = "до " + maxValue.ToString();

            ComponentListBox.ItemsSource = combinedData;
            OnStorageCountLabel.Content = $"Процессоры {ComponentListBox.Items.Count} шт";


            currentUserCart = App.Database.UsersCarts
        .Include("CartItems")
        .FirstOrDefault(c => c.UserId == CurrentUser.Id);
        }
        //ДОБАВЛЕНИЕ В КОРЗИНУ - изменние кнопки
        private void AddToCartCPUButton_Loaded(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var combinedData = button?.DataContext as CombinedData;

            if (combinedData != null)
            {
                if (IsItemInCart(combinedData.Processor))
                {
                    button.Content = "В корзине";
                    button.IsEnabled = false;
                }
                else
                {
                    button.Content = "Купить";
                    button.IsEnabled = true;
                }
            }
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
                        string processorModel = combinedData.Processor.Model.ToLower();
                        string manufacturerName = combinedData.Manufacturer?.ManufacturersName.ToLower() ?? "";

                        return processorModel.Contains(searchText) || manufacturerName.Contains(searchText);
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
                    decimal? costA = a.Processor.Cost ?? 0m;
                    decimal? costB = b.Processor.Cost ?? 0m;

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
                    minValue = (double)combineds.Min(item => item.Processor.Cost.GetValueOrDefault());
                    MinPrice.Tag = "от " + minValue.ToString();

                }
                else
                {
                    minValue = double.Parse(MinPrice.Text);
                }

                // Проверка и установка максимальной цены
                if (string.IsNullOrWhiteSpace(MaxPrice.Text))
                {
                    maxValue = (double)combineds.Max(item => item.Processor.Cost.GetValueOrDefault());
                    MaxPrice.Tag = "до " + maxValue.ToString();
                }
                else
                {
                    maxValue = double.Parse(MaxPrice.Text);
                }

                combineds = combineds.Where(item => item.Processor.Cost.HasValue &&
                                                    item.Processor.Cost.Value >= (decimal)minValue &&
                                                    item.Processor.Cost.Value <= (decimal)maxValue).ToList();

                combineds = combineds.OrderBy(item => item.Processor.Cost).ToList();

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


        //ДОБАВЛЕНИЕ В КОРЗИНУ
        private bool IsItemInCart(CPUS processor)
        {
            if (currentUserCart != null)
            {
                return currentUserCart.CartItems.Any(item => item.CpuId == processor.CPUId);
            }
            return false;
        }




        //ДОБАВЛЕНИЕ В КОРЗИНУ
        private void AddToCartCPUButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var combinedData = button?.DataContext as CombinedData;

            if (combinedData != null)
            {
                var selectedProcessor = combinedData.Processor;

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

                    var cartItem = new CartItems
                    {
                        CartId = userCart.CartId,
                        CpuId = selectedProcessor.CPUId,
                        UsersCarts = userCart
                    };

                    // Добавляем созданный элемент CartItems в корзину пользователя
                    userCart.CartItems.Add(cartItem);

                    context.SaveChanges();
                    // Обновление контента кнопки и блокировка её
                    button.Content = "В корзине";
                    button.IsEnabled = false;
                }
            }
            else
            {
                MessageBox.Show("Ошибка: не удалось определить выбранный процессор.");
            }
            //ОБНОВЛЕНИЕ ИНФОРМАЦИИ О КОРЗИНЕ
            CartUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void CPUShortParametersLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock cpuShortParametersLabel)
            {
                cpuShortParametersLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6DB2E3"));
            }
        }

        private void CPUShortParametersLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock cpuShortParametersLabel)
            {
                cpuShortParametersLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6D6D6"));
            }
        }

        private void CPUShortParametersLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            ListBoxItem listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)sender);

            
            var item = listBoxItem?.DataContext as CombinedData;

            
            if (item != null)
            {
                MessageBox.Show("Processor ID: " + item.Processor.CPUId);
            }
        }
        private T FindAncestor<T>(DependencyObject current)
        where T : DependencyObject
        {
            do
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            } while (current != null);
            return null;
        }

        private void ClearFields()
        {
            // Очистка текстовых полей
            NewCPUCostTextBox.Text = "";
            NewCPUStorageCountTextBox.Text = "";
            NewCPUBaseSpeedTextBox.Text = "";
            NewCPUL2TextBox.Text = "";
            NewCPUL3TextBox.Text = "";
            NewCPUModelTextBox.Text = "";
            NewCPUCoreCountTextBox.Text = "";
            NewCPUChannelsCountTextBox.Text = "";
            NewCPUSpeedMemoryTextBox.Text = "";
            NewCPUTDPTextBox.Text = "";

            // Сброс выбранных элементов в комбобоксах
            NewCPUSoketComboBox.SelectedItem = null;
            NewCPUMemoryTypeComboBox.SelectedItem = null;

            // Очистка выбранного изображения
            selectedImageBytes = null;
        }

        private byte[] ConvertImageToByteArray(string imagePath)
        {
            try
            {
                using (FileStream fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader binaryReader = new BinaryReader(fileStream))
                    {
                        return binaryReader.ReadBytes((int)fileStream.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при конвертации изображения: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private void AddNewCpuButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewCPUDialog.Visibility = Visibility.Visible;
        }

        private void CloseAddNewCPUDialog_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
            AddNewCPUDialog.Visibility = Visibility.Collapsed;
        }

        private bool ValidateDecimalTextBox(TextBox textBox, out decimal result)
        {
            if (!decimal.TryParse(textBox.Text, out result))
            {
                MessageBox.Show($"Неверный формат в поле {textBox.Tag}.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                textBox.Focus();
                textBox.SelectAll();
                return false;
            }
            return true;
        }

        private bool ValidateIntTextBox(TextBox textBox, out int result)
        {
            if (!int.TryParse(textBox.Text, out result))
            {
                MessageBox.Show($"Неверный формат в поле {textBox.Tag}.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                textBox.Focus();
                textBox.SelectAll();
                return false;
            }
            return true;
        }

        private void SaveAddNewCPUDialog_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на заполнение всех полей и выбор изображения
            if (string.IsNullOrWhiteSpace(NewCPUCostTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewCPUStorageCountTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewCPUBaseSpeedTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewCPUL2TextBox.Text) ||
                string.IsNullOrWhiteSpace(NewCPUL3TextBox.Text) ||
                string.IsNullOrWhiteSpace(NewCPUModelTextBox.Text) ||
                NewCPUSoketComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(NewCPUCoreCountTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewCPUChannelsCountTextBox.Text) ||
                NewCPUMemoryTypeComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(NewCPUSpeedMemoryTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewCPUTDPTextBox.Text) ||
                selectedImageBytes == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля и выберите фото.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка на правильный формат данных в текстовых полях
            decimal cost;
            if (!ValidateDecimalTextBox(NewCPUCostTextBox, out cost))
                return;

            int storageCount;
            if (!ValidateIntTextBox(NewCPUStorageCountTextBox, out storageCount))
                return;

            decimal baseSpeed;
            if (!ValidateDecimalTextBox(NewCPUBaseSpeedTextBox, out baseSpeed))
                return;

            decimal l2Size;
            if (!ValidateDecimalTextBox(NewCPUL2TextBox, out l2Size))
                return;

            decimal l3Size;
            if (!ValidateDecimalTextBox(NewCPUL3TextBox, out l3Size))
                return;

            string model = NewCPUModelTextBox.Text;

            int socketId = ((Sockets)NewCPUSoketComboBox.SelectedItem).SocketId;
            int coreCount;
            if (!ValidateIntTextBox(NewCPUCoreCountTextBox, out coreCount))
                return;

            int channelcount;
            if (!ValidateIntTextBox(NewCPUChannelsCountTextBox, out channelcount))
                return;

            int memoryTypeId = ((Memory_types)NewCPUMemoryTypeComboBox.SelectedItem).Memory_typeId;

            int speedMemory;
            if (!ValidateIntTextBox(NewCPUSpeedMemoryTextBox, out speedMemory))
                return;

            int tdp;
            if (!ValidateIntTextBox(NewCPUTDPTextBox, out tdp))
                return;

            // Создаем новый объект CPU с извлеченными данными
            CPUS newCPU = new CPUS
            {
                Cost = cost,
                CPU_Count_on_storage = storageCount,
                Base_processor_speed = baseSpeed,
                L2_cache_size = l2Size,
                L3_cache_size = l3Size,
                Model = model,
                Socket = socketId,
                Total_cores = coreCount,
                Number_of_channels = channelcount,
                Memory_type = memoryTypeId,
                Memory_speed = speedMemory,
                Thermal_design_power = tdp,
                Preview_Photo = selectedImageBytes,
            };

            try
            {
                App.Database.CPUS.Add(newCPU);
                App.Database.SaveChanges();
                MessageBox.Show("Новая запись успешно добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                AddNewCPUDialog.Visibility = Visibility.Collapsed;
                LoadComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении записи в базу данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            ClearFields();
        }

        private byte[] selectedImageBytes;

        private void NewCPUChoosePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedImagePath = openFileDialog.FileName;

                // Проверяем расширение выбранного файла
                string extension = System.IO.Path.GetExtension(selectedImagePath).ToLower();
                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                {
                    // Если выбран файл с поддерживаемым расширением, конвертируем его в массив байтов
                    selectedImageBytes = ConvertImageToByteArray(selectedImagePath);
                }
                else
                {
                    // Выводим уведомление об ошибке, если выбран файл с неподдерживаемым расширением
                    MessageBox.Show("Выбран неподдерживаемый формат файла. Пожалуйста, выберите изображение в формате JPG, JPEG или PNG.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteSelectedCPUButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить этот процессор?", "Удаление процессора", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Получение выбранного элемента ListBox
                var selectedItem = (sender as Button)?.DataContext as CombinedData;

                // Удаление записи из базы данных
                if (selectedItem != null)
                {
                    try
                    {
                        App.Database.CPUS.Remove(selectedItem.Processor);
                        App.Database.SaveChanges();
                        MessageBox.Show("Процессор успешно удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadComponent();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при удалении процессора: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }


        private void DeleteSelectedCPUButton_Loaded(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var combinedData = button?.DataContext as CombinedData;
            switch (CurrentUser.RoleId)
            {
                case 1:
                    if (combinedData != null)
                    {
                        button.Visibility = Visibility.Visible;
                    }
                    break;
            }
        }
    }
}
