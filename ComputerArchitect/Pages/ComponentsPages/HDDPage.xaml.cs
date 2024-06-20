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
    public partial class HDDPage : Page
    {
        public event EventHandler CartUpdated;

        private UsersCarts currentUserCart;
        public Users CurrentUser { get; set; }
        public HDDPage(Users currentUser)
        {
            CurrentUser = currentUser;
            InitializeComponent();
            LoadComponent();
            MostCheapestSort_Checked(null, null);
            switch (CurrentUser.RoleId)
            {
                case 1:
                    AddNewHDDButton.Visibility = Visibility.Visible;
                    break;
            }
        }

        

        public class CombinedData
        {
            public HDDs Hdds { get; set; }
        }
        double minValue;
        double maxValue;
        private void LoadComponent()
        {
            List<HDDs> hddsList = App.Database.HDDs.ToList();

            var combinedData = hddsList.Select(hdd => new CombinedData { Hdds = hdd });
            minValue = (double)combinedData.Min(item => item.Hdds.Cost.GetValueOrDefault());
            MinPrice.Tag = "от " + minValue.ToString();
            maxValue = (double)combinedData.Max(item => item.Hdds.Cost.GetValueOrDefault());
            MaxPrice.Tag = "до " + maxValue.ToString();
            ComponentListBox.ItemsSource = combinedData;
            OnStorageCountLabel.Content = $"Жесткие диски {ComponentListBox.Items.Count} товаров";
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
                        string hddModel = combinedData.Hdds.Model.ToLower();
                        // Добавьте другие поля HDDs, если необходимо

                        return hddModel.Contains(searchText);
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
                    decimal? costA = a.Hdds.Cost ?? 0m;
                    decimal? costB = b.Hdds.Cost ?? 0m;

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
                    minValue = (double)combineds.Min(item => item.Hdds.Cost.GetValueOrDefault());
                    MinPrice.Tag = "от " + minValue.ToString();

                }
                else
                {
                    minValue = double.Parse(MinPrice.Text);
                }

                // Проверка и установка максимальной цены
                if (string.IsNullOrWhiteSpace(MaxPrice.Text))
                {
                    maxValue = (double)combineds.Max(item => item.Hdds.Cost.GetValueOrDefault());
                    MaxPrice.Tag = "до " + maxValue.ToString();
                }
                else
                {
                    maxValue = double.Parse(MaxPrice.Text);
                }

                combineds = combineds.Where(item => item.Hdds.Cost.HasValue &&
                                                    item.Hdds.Cost.Value >= (decimal)minValue &&
                                                    item.Hdds.Cost.Value <= (decimal)maxValue).ToList();

                combineds = combineds.OrderBy(item => item.Hdds.Cost).ToList();

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
        private bool IsItemInCart(HDDs hDDs)
        {
            if (currentUserCart != null)
            {
                return currentUserCart.CartItems.Any(item => item.MemoryId == hDDs.HDDId);
            }
            return false;
        }
        private void AddToCartHDDButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var combinedData = button?.DataContext as CombinedData;

            if (combinedData != null)
            {
                var selectedHDD = combinedData.Hdds;

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
                        MemoryId = selectedHDD.HDDId,
                        MemoryCount = 1,
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

        private void ClearFields()
        {
            NewCostTextBox.Text = "";
            NewCountOnStorageTextBox.Text = "";
            NewModelTextBox.Text = "";
            NewCapacityTBTextBox.Text = "";
            NewInterfaceTextBox.Text = "";
            NewInterfaceBandwidthGBpsTextBox.Text = "";
            NewSpindleSpeedRPMTextBox.Text = "";
            NewCacheMBTextBox.Text = "";
            selectedImageBytes = null;
        }

        private void AddToCartHDDButton_Loaded(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var combinedData = button?.DataContext as CombinedData;

            if (combinedData != null)
            {
                int userId = CurrentUser.Id;
                bool itemInCart;

                // Проверяем наличие жесткого диска в корзине на основе актуальных данных из базы данных
                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    var userCart = context.UsersCarts.Include("CartItems").FirstOrDefault(c => c.UserId == userId);

                    if (userCart != null)
                    {
                        itemInCart = userCart.CartItems.Any(item => item.MemoryId == combinedData.Hdds.HDDId);
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


        private bool ValidateDecimalTextBox(TextBox textBox, out decimal value)
        {
            if (!decimal.TryParse(textBox.Text, out value))
            {
                MessageBox.Show($"Неправильный формат числа в поле {textBox.Tag}.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                textBox.Focus();
                textBox.SelectAll();
                return false;
            }
            return true;
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


        private bool ValidateIntTextBox(TextBox textBox, out int value)
        {
            if (!int.TryParse(textBox.Text, out value))
            {
                MessageBox.Show($"Неправильный формат числа в поле {textBox.Tag}.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                textBox.Focus();
                textBox.SelectAll();
                return false;
            }
            return true;
        }

        private void AddNewHDDButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewHDDDialog.Visibility = Visibility.Visible;
        }

        private void SaveAddNewHDDDialog_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewCostTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewCountOnStorageTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewModelTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewCapacityTBTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewInterfaceTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewInterfaceBandwidthGBpsTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewSpindleSpeedRPMTextBox.Text) ||
                selectedImageBytes == null ||
                string.IsNullOrWhiteSpace(NewCacheMBTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля и выберите фото.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            decimal cost;
            if (!ValidateDecimalTextBox(NewCostTextBox, out cost))
                return;

            int storageCount;
            if (!ValidateIntTextBox(NewCountOnStorageTextBox, out storageCount))
                return;

            string model = NewModelTextBox.Text;
            int capacityGB;
            if (!ValidateIntTextBox(NewCapacityTBTextBox, out capacityGB))
                return;

            string interfaceType = NewInterfaceTextBox.Text;

            decimal interfaceBandwidth;
            if (!decimal.TryParse(NewInterfaceBandwidthGBpsTextBox.Text, out interfaceBandwidth))
            {
                MessageBox.Show("Неверный формат пропускной способности интерфейса. Пожалуйста, введите числовое значение.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int spindleSpeedRPM;
            if (!ValidateIntTextBox(NewSpindleSpeedRPMTextBox, out spindleSpeedRPM))
                return;

            int cacheMB;
            if (!ValidateIntTextBox(NewCacheMBTextBox, out cacheMB))
                return;

            

            // Создание нового объекта HDD для добавления в базу данных
            HDDs newHDD = new HDDs
            {
                Cost = cost,
                HDD_Count_on_storage = storageCount,
                Model = model,
                Capacity_TB = capacityGB,
                Interface = interfaceType,
                Interface_Bandwidth_GBps = interfaceBandwidth,
                Spindle_Speed_RPM = spindleSpeedRPM,
                Cache_MB = cacheMB,
                Preview_Photo = selectedImageBytes,
            };

            try
            {
                // Добавление нового HDD в базу данных и сохранение изменений
                App.Database.HDDs.Add(newHDD);
                App.Database.SaveChanges();
                MessageBox.Show("Новая запись успешно добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Скрытие диалогового окна после успешного добавления
                AddNewHDDDialog.Visibility = Visibility.Collapsed;

                // Обновление компонентов интерфейса (если требуется)
                LoadComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении записи в базу данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Очистка полей после сохранения
            ClearFields();
        }


        private void CloseAddNewHDDDialog_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
            AddNewHDDDialog.Visibility = Visibility.Collapsed;
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

        private void DeleteSelectedButton_Loaded(object sender, RoutedEventArgs e)
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

        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить этот жесткий диск?", "Удаление жесткого диска", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                
                var selectedItem = (sender as Button)?.DataContext as CombinedData;

                
                if (selectedItem != null)
                {
                    try
                    {
                        App.Database.HDDs.Remove(selectedItem.Hdds); 
                        App.Database.SaveChanges();
                        LoadComponent(); 
                        MessageBox.Show("Жесткий диск успешно удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при удалении жесткого диска: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

        }
    }
}
