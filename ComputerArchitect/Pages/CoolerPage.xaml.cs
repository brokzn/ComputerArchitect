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
using static System.Net.Mime.MediaTypeNames;

namespace ComputerArchitect.Pages
{
    /// <summary>
    /// Логика взаимодействия для CPUPage.xaml
    /// </summary>
    public partial class CoolerPage : Page
    {
        public event EventHandler CartUpdated;
        public Users CurrentUser { get; set; }
        private UsersCarts currentUserCart;
        public CoolerPage(Users currentUser)
        {
            CurrentUser = currentUser;
            InitializeComponent();
            LoadComponent();
            MostCheapestSort_Checked(null, null);
            NewMBSocketComboBox.ItemsSource = App.Database.Sockets.ToList();
            switch (CurrentUser.RoleId)
            {
                case 1:
                    AddNewCoolerButton.Visibility = Visibility.Visible;
                    break;
            }
        }
        public class CombinedData
        {
            public Coolers Cooler { get; set; }
            public Sockets Socket { get; set; }
        }
        double minValue;
        double maxValue;
        private void LoadComponent()
        {
            List<Coolers> coolers = App.Database.Coolers.ToList();
            List<Sockets> sockets = App.Database.Sockets.ToList();

            var combinedData = from cooler in coolers
                               join socket in sockets on cooler.Socket_Type equals socket.SocketId into coolerSocketGroup
                               from socketData in coolerSocketGroup.DefaultIfEmpty()
                               select new CombinedData
                               {
                                   Cooler = cooler,
                                   Socket = socketData
                               };
            minValue = (double)combinedData.Min(item => item.Cooler.Cost.GetValueOrDefault());
            MinPrice.Tag = "от " + minValue.ToString();
            maxValue = (double)combinedData.Max(item => item.Cooler.Cost.GetValueOrDefault());
            MaxPrice.Tag = "до " + maxValue.ToString();
            ComponentListBox.ItemsSource = combinedData;
            OnStorageCountLabel.Content = $"Охлаждение для процессоров {ComponentListBox.Items.Count} шт";

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
                        string coolerModel = combinedData.Cooler.Model.ToLower();

                        return coolerModel.Contains(searchText);
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
                    decimal? costA = a.Cooler.Cost ?? 0m;
                    decimal? costB = b.Cooler.Cost ?? 0m;

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
                    minValue = (double)combineds.Min(item => item.Cooler.Cost.GetValueOrDefault());
                    MinPrice.Tag = "от " + minValue.ToString();

                }
                else
                {
                    minValue = double.Parse(MinPrice.Text);
                }

                // Проверка и установка максимальной цены
                if (string.IsNullOrWhiteSpace(MaxPrice.Text))
                {
                    maxValue = (double)combineds.Max(item => item.Cooler.Cost.GetValueOrDefault());
                    MaxPrice.Tag = "до " + maxValue.ToString();
                }
                else
                {
                    maxValue = double.Parse(MaxPrice.Text);
                }

                combineds = combineds.Where(item => item.Cooler.Cost.HasValue &&
                                                    item.Cooler.Cost.Value >= (decimal)minValue &&
                                                    item.Cooler.Cost.Value <= (decimal)maxValue).ToList();

                combineds = combineds.OrderBy(item => item.Cooler.Cost).ToList();

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
        private bool IsItemInCart(Coolers coolers)
        {
            if (currentUserCart != null)
            {
                return currentUserCart.CartItems.Any(item => item.FanId == coolers.CoolerId);
            }
            return false;
        }

        private void AddToCartCoolerButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var combinedData = button?.DataContext as CombinedData;

            if (combinedData != null)
            {
                var selectedcoolers = combinedData.Cooler;

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
                        FanId = selectedcoolers.CoolerId,
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

        private void AddToCartCoolerButton_Loaded(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var combinedData = button?.DataContext as CombinedData;

            if (combinedData != null)
            {
                int userId = CurrentUser.Id;
                bool itemInCart;

                // Проверяем наличие кулера в корзине на основе актуальных данных из базы данных
                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    var userCart = context.UsersCarts.Include("CartItems").FirstOrDefault(c => c.UserId == userId);

                    if (userCart != null)
                    {
                        itemInCart = userCart.CartItems.Any(item => item.FanId == combinedData.Cooler.CoolerId);
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


        private void DeleteSelectedCoolerButton_Loaded(object sender, RoutedEventArgs e)
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

        private void DeleteSelectedCoolerButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить этот кулер?", "Удаление кулера", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Получение выбранного элемента ListBox
                var selectedItem = (sender as Button)?.DataContext as CombinedData;

                // Удаление записи из базы данных
                if (selectedItem != null)
                {
                    try
                    {
                        App.Database.Coolers.Remove(selectedItem.Cooler);
                        App.Database.SaveChanges();
                        LoadComponent();
                        MessageBox.Show("Кулер успешно удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при удалении кулера: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void AddNewCoolerButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewCoolerDialog.Visibility = Visibility.Visible;
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

        private void ClearFields()
        {
            // Очистка текстовых полей
            NewCostTextBox.Text = "";
            NewModelTextBox.Text = "";
            NewCountOnStorageTextBox.Text = "";
            NewBaseMaterialTextBox.Text = "";
            NewMinFanSpeedRPMTextBox.Text = "";
            NewMaxNoiseLeveldBTextBox.Text = "";
            NewFanConnectorTypeTextBox.Text = "";
            NewTDPTextBox.Text = "";

            // Сброс выбранных элементов в комбобоксах
            NewMBSocketComboBox.SelectedItem = null;

            // Очистка выбранного изображения
            selectedImageBytes = null;
        }

        private void SaveAddNewCoolerDialog_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения всех необходимых полей
            if (string.IsNullOrWhiteSpace(NewCostTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewCountOnStorageTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewModelTextBox.Text) ||
                NewMBSocketComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(NewBaseMaterialTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewMinFanSpeedRPMTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewMaxNoiseLeveldBTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewFanConnectorTypeTextBox.Text) ||
                selectedImageBytes == null||
            string.IsNullOrWhiteSpace(NewTDPTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля и выберите фото.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            decimal cost;
            if (!ValidateDecimalTextBox(NewCostTextBox, out cost))
                return;

            string model = NewModelTextBox.Text;
            int socketId = ((Sockets)NewMBSocketComboBox.SelectedItem).SocketId;
            string baseMaterial = NewBaseMaterialTextBox.Text;

            int storageCount;
            if (!ValidateIntTextBox(NewCountOnStorageTextBox, out storageCount))
                return;

            int minFanSpeedRPM;
            if (!ValidateIntTextBox(NewMinFanSpeedRPMTextBox, out minFanSpeedRPM))
                return;

            int maxNoiseLevel;
            if (!ValidateIntTextBox(NewMaxNoiseLeveldBTextBox, out maxNoiseLevel))
                return;

            string fanConnectorType = NewFanConnectorTypeTextBox.Text;

            int TDP;
            if (!ValidateIntTextBox(NewTDPTextBox, out TDP))
                return;

            // Создание нового объекта Cooler
            Coolers newCooler = new Coolers
            {
                Cost = cost,
                Cooler_Count_on_storage = storageCount,
                Model = model,
                Socket_Type = socketId,
                Base_Material = baseMaterial,
                Min_Fan_Speed_RPM = minFanSpeedRPM,
                Max_Noise_Level_dB = maxNoiseLevel,
                Fan_Connector_Type = fanConnectorType,
                TDP = TDP,
                Preview_Photo = selectedImageBytes, 
            };

            try
            {
                // Добавление нового кулера в базу данных
                App.Database.Coolers.Add(newCooler);
                App.Database.SaveChanges();
                MessageBox.Show("Новый кулер успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Скрытие диалогового окна после сохранения
                AddNewCoolerDialog.Visibility = Visibility.Collapsed;

                // Перезагрузка компонента для отображения обновленного списка кулеров
                LoadComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении кулера в базу данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Очистка полей ввода после сохранения
            ClearFields();
        }


        private void CloseAddNewCoolerDialog_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
            AddNewCoolerDialog.Visibility = Visibility.Collapsed;
        }
    }
}
