using ComputerArchitect.Database;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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
    public partial class MotherBoardPage : Page
    {
        public event EventHandler CartUpdated;

        private UsersCarts currentUserCart;
        public Users CurrentUser { get; set; }

        public MotherBoardPage(Users currentUser)
        {
            CurrentUser = currentUser;
            InitializeComponent();
            LoadComponent();
            MostCheapestSort_Checked(null, null);

            NewMBFormFactorComboBox.ItemsSource = App.Database.Form_Factors.ToList();
            NewMBMemoryTypeComboBox.ItemsSource = App.Database.Memory_types.ToList();
            NewMBSocketComboBox.ItemsSource = App.Database.Sockets.ToList();

            switch (CurrentUser.RoleId)
            {
                case 1:
                    AddNewMBButton.Visibility = Visibility.Visible;
                    break;
            }
        }
        public class CombinedData
        {
            public Motherboards Motherboard { get; set; }
            public Sockets Socket { get; set; }
            public Memory_types MemoryType { get; set; }
            public Form_Factors FormFactor { get; set; }
        }
        double minValue;
        double maxValue;

        private void LoadComponent()
        {
            List<Motherboards> motherboards = App.Database.Motherboards.ToList();
            List<ComputerArchitect.Database.Sockets> sockets = App.Database.Sockets.ToList();
            List<Memory_types> memoryTypes = App.Database.Memory_types.ToList();
            List<Form_Factors> formFactors = App.Database.Form_Factors.ToList();

            var combinedData = from motherboard in motherboards
                               join socket in sockets on motherboard.Socket equals socket.SocketId into motherboardSocketGroup
                               from socketData in motherboardSocketGroup.DefaultIfEmpty()
                               join memoryType in memoryTypes on motherboard.Memory_Type equals memoryType.Memory_typeId into motherboardMemoryTypeGroup
                               from memoryTypeData in motherboardMemoryTypeGroup.DefaultIfEmpty()
                               join formFactor in formFactors on motherboard.Form_Factor equals formFactor.Form_FactorId into motherboardFormFactorGroup
                               from formFactorData in motherboardFormFactorGroup.DefaultIfEmpty()
                               select new CombinedData
                               {
                                   Motherboard = motherboard,
                                   Socket = socketData,
                                   MemoryType = memoryTypeData,
                                   FormFactor = formFactorData
                               };
            minValue = (double)combinedData.Min(item => item.Motherboard.Cost.GetValueOrDefault());
            MinPrice.Tag = "от " + minValue.ToString();
            maxValue = (double)combinedData.Max(item => item.Motherboard.Cost.GetValueOrDefault());
            MaxPrice.Tag = "до " + maxValue.ToString();
            ComponentListBox.ItemsSource = combinedData;
            OnStorageCountLabel.Content = $"Материнские платы {ComponentListBox.Items.Count} товаров";

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
                        string motherboardModel = combinedData.Motherboard.Motherboard_Model.ToLower();
                        string socketName = combinedData.Socket?.SocketName.ToLower() ?? "";

                        return motherboardModel.Contains(searchText) || socketName.Contains(searchText);
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
                    decimal? costA = a.Motherboard.Cost ?? 0m;
                    decimal? costB = b.Motherboard.Cost ?? 0m;

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
                    minValue = (double)combineds.Min(item => item.Motherboard.Cost.GetValueOrDefault());
                    MinPrice.Tag = "от " + minValue.ToString();

                }
                else
                {
                    minValue = double.Parse(MinPrice.Text);
                }

                // Проверка и установка максимальной цены
                if (string.IsNullOrWhiteSpace(MaxPrice.Text))
                {
                    maxValue = (double)combineds.Max(item => item.Motherboard.Cost.GetValueOrDefault());
                    MaxPrice.Tag = "до " + maxValue.ToString();
                }
                else
                {
                    maxValue = double.Parse(MaxPrice.Text);
                }

                combineds = combineds.Where(item => item.Motherboard.Cost.HasValue &&
                                                    item.Motherboard.Cost.Value >= (decimal)minValue &&
                                                    item.Motherboard.Cost.Value <= (decimal)maxValue).ToList();

                combineds = combineds.OrderBy(item => item.Motherboard.Cost).ToList();

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

        private bool IsItemInCart(Motherboards motherboards)
        {
            if (currentUserCart != null)
            {
                return currentUserCart.CartItems.Any(item => item.MotherboardId == motherboards.MotherboardId);
            }
            return false;
        }
        private void AddToCartMotherboardButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var combinedData = button?.DataContext as CombinedData;

            if (combinedData != null)
            {
                var selectedMotherboard = combinedData.Motherboard;

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
                        MotherboardId = selectedMotherboard.MotherboardId,
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

        private void AddToCartMotherboardButton_Loaded(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var combinedData = button?.DataContext as CombinedData;

            if (combinedData != null)
            {
                int userId = CurrentUser.Id;
                bool itemInCart;

                // Проверяем наличие материнской платы в корзине на основе актуальных данных из базы данных
                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    var userCart = context.UsersCarts.Include("CartItems").FirstOrDefault(c => c.UserId == userId);

                    if (userCart != null)
                    {
                        itemInCart = userCart.CartItems.Any(item => item.MotherboardId == combinedData.Motherboard.MotherboardId);
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

        private void CloseAddNewMBDialog_Click(object sender, RoutedEventArgs e)
        {
            ClearMotherBoardFields();
            AddNewMotherBoardDialog.Visibility = Visibility.Collapsed;
        }
        private void ClearMotherBoardFields()
        {
            // Очистка текстовых полей
            NewCostTextBox.Text = "";
            NewCountOnStorageTextBox.Text = "";
            NewModelTextBox.Text = "";
            NewMBMemorySlotsTextBox.Text = "";
            NewMBMaxMemoryFrequencyMhzTextBox.Text = "";
            NewMBPcie_X16_SlotsTextBox.Text = "";

            // Сброс выбранных элементов в комбобоксах
            NewMBSocketComboBox.SelectedItem = null;
            NewMBMemoryTypeComboBox.SelectedItem = null;
            NewMBFormFactorComboBox.SelectedItem = null;

            // Очистка выбранного изображения
            selectedImageBytes = null;
        }

        private byte[] selectedImageBytes;
        private void SaveAddNewMBDialog_Click(object sender, RoutedEventArgs e)
        {
            
            if (string.IsNullOrWhiteSpace(NewCostTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewMBChipsetTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewCountOnStorageTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewModelTextBox.Text) ||
                NewMBSocketComboBox.SelectedItem == null ||
                selectedImageBytes == null ||
                string.IsNullOrWhiteSpace(NewMBMemorySlotsTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewMBMaxMemoryFrequencyMhzTextBox.Text) ||
                NewMBMemoryTypeComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(NewMBPcie_X16_SlotsTextBox.Text) ||
                NewMBFormFactorComboBox.SelectedItem == null)
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

            string chipset = NewMBChipsetTextBox.Text;

            int socketId = ((Sockets)NewMBSocketComboBox.SelectedItem).SocketId;

            int memorySlots;
            if (!ValidateIntTextBox(NewMBMemorySlotsTextBox, out memorySlots))
                return;

            int maxMemoryFrequencyMhz;
            if (!ValidateIntTextBox(NewMBMaxMemoryFrequencyMhzTextBox, out maxMemoryFrequencyMhz))
                return;

            int memoryTypeId = ((Memory_types)NewMBMemoryTypeComboBox.SelectedItem).Memory_typeId;

            int pcieX16Slots;
            if (!ValidateIntTextBox(NewMBPcie_X16_SlotsTextBox, out pcieX16Slots))
                return;

            int formFactorId = ((Form_Factors)NewMBFormFactorComboBox.SelectedItem).Form_FactorId;


            Motherboards newMotherBoard = new Motherboards
            {
                Cost = cost,
                Motherboard_Count_on_storage = storageCount,
                Motherboard_Model = model,
                Socket = socketId,
                Memory_Slots = memorySlots,
                Chipset = chipset,
                Max_Memory_Frequency_Mhz = maxMemoryFrequencyMhz,
                Memory_Type = memoryTypeId,
                Pcie_X16_Slots = pcieX16Slots,
                Form_Factor = formFactorId,
                Preview_Photo = selectedImageBytes,
            };

            try
            {
                App.Database.Motherboards.Add(newMotherBoard);
                App.Database.SaveChanges();
                MessageBox.Show("Новая запись успешно добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                AddNewMotherBoardDialog.Visibility = Visibility.Collapsed;
                LoadComponent(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении записи в базу данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ClearMotherBoardFields();
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

        private void AddNewMBButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewMotherBoardDialog.Visibility = Visibility.Visible;
        }

        private void DeleteSelectedMBButton_Loaded(object sender, RoutedEventArgs e)
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

        private void DeleteSelectedMBButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить эту материнскую плату?", "Удаление материнской платы", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Получение выбранного элемента ListBox
                var selectedItem = (sender as Button)?.DataContext as CombinedData;

                // Удаление записи из базы данных
                if (selectedItem != null)
                {
                    try
                    {
                        App.Database.Motherboards.Remove(selectedItem.Motherboard);
                        App.Database.SaveChanges();
                        LoadComponent();
                        MessageBox.Show("Материнская плата успешно удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при удалении материнской платы: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
