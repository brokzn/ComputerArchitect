using ComputerArchitect.Database;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComputerArchitect.Pages
{
    /// <summary>
    /// Логика взаимодействия для CPUPage.xaml
    /// </summary>
    public partial class RAMPage : Page
    {
        public event EventHandler CartUpdated;

        private UsersCarts currentUserCart;
        public Users CurrentUser { get; set; }
        public RAMPage(Users currentUser)
        {
            CurrentUser = currentUser;
            InitializeComponent();
            LoadComponent();
            MostCheapestSort_Checked(null, null);
            NewRAMMemoryTypeComboBox.ItemsSource = App.Database.Memory_types.ToList();
            switch (CurrentUser.RoleId)
            {
                case 1:
                    AddNewRAMButton.Visibility = Visibility.Visible;
                    break;
            }
        }


        public class CombinedData
        {
            public RAMS Rams { get; set; }
            public Memory_types MemoryType { get; set; }
        }
        double minValue;
        double maxValue;
        private void LoadComponent()
        {
            List<RAMS> ramsList = App.Database.RAMS.ToList();
            List<Memory_types> memoryTypes = App.Database.Memory_types.ToList();

            var combinedData = from ram in ramsList
                               join memoryType in memoryTypes on ram.Memory_Type equals memoryType.Memory_typeId into ramMemoryTypeGroup
                               from memoryTypeData in ramMemoryTypeGroup.DefaultIfEmpty()
                               select new CombinedData
                               {
                                   Rams = ram,
                                   MemoryType = memoryTypeData
                               };
            minValue = (double)combinedData.Min(item => item.Rams.Cost.GetValueOrDefault());
            MinPrice.Tag = "от " + minValue.ToString();
            maxValue = (double)combinedData.Max(item => item.Rams.Cost.GetValueOrDefault());
            MaxPrice.Tag = "до " + maxValue.ToString();

            ComponentListBox.ItemsSource = combinedData;
            OnStorageCountLabel.Content = $"Оперативная память {ComponentListBox.Items.Count} товаров";

             currentUserCart = App.Database.UsersCarts
            .Include("CartItems")
            .FirstOrDefault(c => c.UserId == CurrentUser.Id);
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
                        string ramModel = combinedData.Rams.RAM_Model.ToLower();

                        return ramModel.Contains(searchText);
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
                    decimal? costA = a.Rams.Cost ?? 0m;
                    decimal? costB = b.Rams.Cost ?? 0m;

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
                    minValue = (double)combineds.Min(item => item.Rams.Cost.GetValueOrDefault());
                    MinPrice.Tag = "от " + minValue.ToString();

                }
                else
                {
                    minValue = double.Parse(MinPrice.Text);
                }

                // Проверка и установка максимальной цены
                if (string.IsNullOrWhiteSpace(MaxPrice.Text))
                {
                    maxValue = (double)combineds.Max(item => item.Rams.Cost.GetValueOrDefault());
                    MaxPrice.Tag = "до " + maxValue.ToString();
                }
                else
                {
                    maxValue = double.Parse(MaxPrice.Text);
                }

                combineds = combineds.Where(item => item.Rams.Cost.HasValue &&
                                                    item.Rams.Cost.Value >= (decimal)minValue &&
                                                    item.Rams.Cost.Value <= (decimal)maxValue).ToList();

                combineds = combineds.OrderBy(item => item.Rams.Cost).ToList();

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

        private bool IsItemInCart(RAMS rAMS)
        {
            if (currentUserCart != null)
            {
                return currentUserCart.CartItems.Any(item => item.RAMId == rAMS.RAMId);
            }
            return false;
        }


        private void AddToCartRAMButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var combinedData = button?.DataContext as CombinedData;

            if (combinedData != null)
            {
                var selectedram = combinedData.Rams;

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
                        RAMId = selectedram.RAMId,
                        RAMCount = 1,
                        UsersCarts = userCart
                    };

                    // Добавляем созданный элемент CartItems в корзину пользователя
                    userCart.CartItems.Add(cartItem);

                    context.SaveChanges();
                    // Обновление контента кнопки и блокировка её
                    button.Content = "В корзине";
                    button.IsEnabled = false;
                    CartUpdated?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                MessageBox.Show("Ошибка: не удалось определить выбранный процессор.");
            }
        }

        private void AddToCartRAMButton_Loaded(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var combinedData = button?.DataContext as CombinedData;

            if (combinedData != null)
            {
                int userId = CurrentUser.Id;
                bool itemInCart;

                // Проверяем наличие оперативной памяти в корзине на основе актуальных данных из базы данных
                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    var userCart = context.UsersCarts.Include("CartItems").FirstOrDefault(c => c.UserId == userId);

                    if (userCart != null)
                    {
                        itemInCart = userCart.CartItems.Any(item => item.RAMId == combinedData.Rams.RAMId);
                    }
                    else
                    {
                        itemInCart = false;
                    }
                }

                // Устанавливаем состояние кнопки в зависимости от результата проверки
                if (itemInCart)
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


        private void ClearFields()
        {
            // Очистка текстовых полей
            NewCostTextBox.Text = "";
            NewCountOnStorageTextBox.Text = "";
            NewModelTextBox.Text = "";
            NewCapacityGBTextBox.Text = "";
            NewRAMSpeedMHzTextBox.Text = "";
            NewRAMCASLatencyTextBox.Text = "";
            NewRAMRAStoCASDelayTextBox.Text = "";
            NewRAMRowPrechargeDelayTextBox.Text = "";
            NewRAMActivatetoPrechargeDelayTextBox.Text = "";

            // Сброс выбранных элементов в комбобоксах
            NewRAMMemoryTypeComboBox.SelectedItem = null;

            // Очистка выбранного изображения
            selectedImageBytes = null;
        }
        private byte[] selectedImageBytes;
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

        private void DeleteSelectedRAMButton_Loaded(object sender, RoutedEventArgs e)
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

        private void DeleteSelectedRAMButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить эту оперативную память?", "Удаление оперативной памяти", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Получение выбранного элемента ListBox
                var selectedItem = (sender as Button)?.DataContext as CombinedData;

                // Удаление записи из базы данных
                if (selectedItem != null)
                {
                    try
                    {
                        App.Database.RAMS.Remove(selectedItem.Rams);
                        App.Database.SaveChanges();
                        MessageBox.Show("Оперативная память успешно удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadComponent();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при удалении оперативной памяти: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

        }

        private void AddNewRAMButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewRAMDialog.Visibility = Visibility.Visible;
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
        private void SaveAddNewRAMDialog_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на заполнение всех полей и выбор изображения
            if (string.IsNullOrWhiteSpace(NewCostTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewCountOnStorageTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewModelTextBox.Text) ||
                NewRAMMemoryTypeComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(NewCapacityGBTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewRAMSpeedMHzTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewRAMCASLatencyTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewRAMRAStoCASDelayTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewRAMRowPrechargeDelayTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewRAMActivatetoPrechargeDelayTextBox.Text) ||
                selectedImageBytes == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля и выберите фото.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка на правильный формат данных в текстовых полях
            decimal cost;
            if (!ValidateDecimalTextBox(NewCostTextBox, out cost))
                return;

            int storageCount;
            if (!ValidateIntTextBox(NewCountOnStorageTextBox, out storageCount))
                return;

            int capacityGB;
            if (!ValidateIntTextBox(NewCapacityGBTextBox, out capacityGB))
                return;

            int speedMHz;
            if (!ValidateIntTextBox(NewRAMSpeedMHzTextBox, out speedMHz))
                return;

            int casLatency;
            if (!ValidateIntTextBox(NewRAMCASLatencyTextBox, out casLatency))
                return;

            int raStoCasDelay;
            if (!ValidateIntTextBox(NewRAMRAStoCASDelayTextBox, out raStoCasDelay))
                return;

            int rowPrechargeDelay;
            if (!ValidateIntTextBox(NewRAMRowPrechargeDelayTextBox, out rowPrechargeDelay))
                return;

            int activateToPrechargeDelay;
            if (!ValidateIntTextBox(NewRAMActivatetoPrechargeDelayTextBox, out activateToPrechargeDelay))
                return;

            // Создаем новый объект оперативной памяти с извлеченными данными
            RAMS newRAM = new RAMS
            {
                Cost = cost,
                RAM_Count_on_storage = storageCount,
                RAM_Model = NewModelTextBox.Text,
                Memory_Type = ((Memory_types)NewRAMMemoryTypeComboBox.SelectedItem).Memory_typeId,
                Capacity_GB = capacityGB,
                RAM_Speed_MHz = speedMHz,
                CAS_Latency = casLatency,
                RAS_to_CAS_Delay = raStoCasDelay,
                Row_Precharge_Delay = rowPrechargeDelay,
                Activate_to_Precharge_Delay = activateToPrechargeDelay,
                Preview_Photo = selectedImageBytes,
            };

            try
            {
                App.Database.RAMS.Add(newRAM);
                App.Database.SaveChanges();
                MessageBox.Show("Новая запись успешно добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                AddNewRAMDialog.Visibility = Visibility.Collapsed;
                LoadComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении записи в базу данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            ClearFields();

        }

        private void CloseAddNewRANDialog_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
            AddNewRAMDialog.Visibility = Visibility.Collapsed;
        }

        private void NewChoosePhotoButton_Click(object sender, RoutedEventArgs e)
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
    }
}
