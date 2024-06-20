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
    public partial class GPUPage : Page
    {
        public event EventHandler CartUpdated;

        private UsersCarts currentUserCart;
        public Users CurrentUser { get; set; }
        public GPUPage(Users currentUser)
        {
            CurrentUser = currentUser;
            InitializeComponent();
            LoadComponent();
            MostCheapestSort_Checked(null, null);
            switch (CurrentUser.RoleId)
            {
                case 1:
                    AddNewGPUButton.Visibility = Visibility.Visible;
                    break;
            }
        }

       

        public class CombinedData
        {
            public GPUS GPUProcessor { get; set; }
        }
        double minValue;
        double maxValue;
        private void LoadComponent()
        {
            var combinedData = (from GPU in App.Database.GPUS
                                select new CombinedData
                                {
                                    GPUProcessor = GPU,
                                }).ToList();
            minValue = (double)combinedData.Min(item => item.GPUProcessor.Cost.GetValueOrDefault());
            MinPrice.Tag = "от " + minValue.ToString();
            maxValue = (double)combinedData.Max(item => item.GPUProcessor.Cost.GetValueOrDefault());
            MaxPrice.Tag = "до " + maxValue.ToString();


            ComponentListBox.ItemsSource = combinedData;
            OnStorageCountLabel.Content = $"Видеокарты {ComponentListBox.Items.Count} товаров";
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
                        string GPUModel = combinedData.GPUProcessor.GPU_Model.ToLower();

                        return GPUModel.Contains(searchText);
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
                    decimal? costA = a.GPUProcessor.Cost ?? 0m;
                    decimal? costB = b.GPUProcessor.Cost ?? 0m;

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
                    minValue = (double)combineds.Min(item => item.GPUProcessor.Cost.GetValueOrDefault());
                    MinPrice.Tag = "от " + minValue.ToString();

                }
                else
                {
                    minValue = double.Parse(MinPrice.Text);
                }

                // Проверка и установка максимальной цены
                if (string.IsNullOrWhiteSpace(MaxPrice.Text))
                {
                    maxValue = (double)combineds.Max(item => item.GPUProcessor.Cost.GetValueOrDefault());
                    MaxPrice.Tag = "до " + maxValue.ToString();
                }
                else
                {
                    maxValue = double.Parse(MaxPrice.Text);
                }

                combineds = combineds.Where(item => item.GPUProcessor.Cost.HasValue &&
                                                    item.GPUProcessor.Cost.Value >= (decimal)minValue &&
                                                    item.GPUProcessor.Cost.Value <= (decimal)maxValue).ToList();

                combineds = combineds.OrderBy(item => item.GPUProcessor.Cost).ToList();

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
        private bool IsItemInCart(GPUS GPUprocessor)
        {
            if (currentUserCart != null)
            {
                return currentUserCart.CartItems.Any(item => item.GPUId == GPUprocessor.GPUId);
            }
            return false;
        }
        private void AddToCartGPUButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var combinedData = button?.DataContext as CombinedData;

            if (combinedData != null)
            {
                var selectedGPUProcessor = combinedData.GPUProcessor;

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
                        GPUId = selectedGPUProcessor.GPUId,
                        GPUCount = 1,
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
                MessageBox.Show("Ошибка: не удалось определить выбранный товар.");
            }
        }

        private void AddToCartGPUButton_Loaded(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var combinedData = button?.DataContext as CombinedData;

            if (combinedData != null)
            {
                int userId = CurrentUser.Id;
                bool itemInCart;

                // Проверяем наличие видеокарты в корзине на основе актуальных данных из базы данных
                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    var userCart = context.UsersCarts.Include("CartItems").FirstOrDefault(c => c.UserId == userId);

                    if (userCart != null)
                    {
                        itemInCart = userCart.CartItems.Any(item => item.GPUId == combinedData.GPUProcessor.GPUId);
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


        private byte[] selectedImageBytes;

        private void ClearFields()
        {
            // Очистка текстовых полей
            NewCostTextBox.Text = "";
            NewCountOnStorageTextBox.Text = "";
            NewModelTextBox.Text = "";
            NewGPUConnectionInterfaceTextBox.Text = "";
            NewGPUVideoMemorySizeGBTextBox.Text = "";
            NewGPUVideoMemoryTypeTextBox.Text = "";
            NewGPUMemoryBusWidthBitsTextBox.Text = "";
            NewGPUVideoOutputTypeandCountTextBox.Text = "";
            NewGPUBaseFrequencyMHzTextBox.Text = "";

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

        private void AddNewGPUButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewCPUDialog.Visibility = Visibility.Visible;
        }

        private void CloseAddNewGPUDialog_Click(object sender, RoutedEventArgs e)
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

        private void SaveAddNewGPUDialog_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на заполнение всех полей и выбор изображения
            if (string.IsNullOrWhiteSpace(NewCostTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewCountOnStorageTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewModelTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewGPUConnectionInterfaceTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewGPUVideoMemorySizeGBTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewGPUVideoMemoryTypeTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewGPUMemoryBusWidthBitsTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewGPUVideoOutputTypeandCountTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewGPUBaseFrequencyMHzTextBox.Text) ||
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

            string model = NewModelTextBox.Text;

            string connectionInterface = NewGPUConnectionInterfaceTextBox.Text;

           

            string videoMemoryType = NewGPUVideoMemoryTypeTextBox.Text;

            int memoryBusWidthBits;
            if (!ValidateIntTextBox(NewGPUMemoryBusWidthBitsTextBox, out memoryBusWidthBits))
                return;

            string videoOutputTypeandCount = NewGPUVideoOutputTypeandCountTextBox.Text;

            int videoMemorySizeGB;
            if (!ValidateIntTextBox(NewGPUVideoMemorySizeGBTextBox, out videoMemorySizeGB))
                return;

            int baseFrequencyMHz;
            if (!ValidateIntTextBox(NewGPUBaseFrequencyMHzTextBox, out baseFrequencyMHz))
                return;


            // Создаем новый объект GPU с извлеченными данными
            GPUS newGPU = new GPUS
            {
                Cost = cost,
                GPU_Count_on_storage = storageCount,
                GPU_Model = model,
                Connection_Interface = connectionInterface,
                Video_Memory_Size_GB = videoMemorySizeGB,
                Video_Memory_Type = videoMemoryType,
                Memory_Bus_Width_Bits = memoryBusWidthBits,
                Video_Output_Type_and_Count = videoOutputTypeandCount,
                GPU_Base_Frequency_MHz = baseFrequencyMHz,
                Preview_Photo = selectedImageBytes,
            };

            try
            {
                App.Database.GPUS.Add(newGPU);
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

        private void DeleteSelectedGPUButton_Loaded(object sender, RoutedEventArgs e)
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

        private void DeleteSelectedGPUButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить эту видеокарту?", "Удаление видеокарты", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Получение выбранного элемента ListBox
                var selectedItem = (sender as Button)?.DataContext as CombinedData;

                // Удаление записи из базы данных
                if (selectedItem != null)
                {
                    try
                    {
                        App.Database.GPUS.Remove(selectedItem.GPUProcessor);
                        App.Database.SaveChanges();
                        MessageBox.Show("Видеокарта успешно удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadComponent();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при удалении видеокарты: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

        }
    }
}
