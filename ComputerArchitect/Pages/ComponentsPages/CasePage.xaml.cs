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
    public partial class CasePage : Page
    {
        public event EventHandler CartUpdated;

        private UsersCarts currentUserCart;
        public Users CurrentUser { get; set; }
        public CasePage(Users currentUser)
        {
            CurrentUser = currentUser;
            InitializeComponent();
            LoadComponent();
            MostCheapestSort_Checked(null, null);
            NewCaseSizeNameComboBox.ItemsSource = App.Database.CaseSizes.ToList();
            switch (CurrentUser.RoleId)
            {
                case 1:
                    AddNewCaseButton.Visibility = Visibility.Visible;
                    break;
            }

        }

        double minValue;
        double maxValue;
        public class CombinedData
        {
            public Cases Case { get; set; }
            public CaseSizes CaseSizes { get; set; }
        }

        private void LoadComponent()
        {
            var combinedData = (from caseData in App.Database.Cases
                                join caseSizeData in App.Database.CaseSizes
                                on caseData.Case_Size equals caseSizeData.CaseSize_Id
                                select new CombinedData
                                {
                                    Case = caseData,
                                    CaseSizes = caseSizeData,
                                }).ToList();
            minValue = (double)combinedData.Min(item => item.Case.Cost.GetValueOrDefault());
            MinPrice.Tag = "от " + minValue.ToString();
            maxValue = (double)combinedData.Max(item => item.Case.Cost.GetValueOrDefault());
            MaxPrice.Tag = "до " + maxValue.ToString();
            ComponentListBox.ItemsSource = combinedData;
            OnStorageCountLabel.Content = $"Корпуса {ComponentListBox.Items.Count} товаров";
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
                        string caseModel = combinedData.Case.Model.ToLower();

                        return caseModel.Contains(searchText);
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
                    decimal? costA = a.Case.Cost ?? 0m;
                    decimal? costB = b.Case.Cost ?? 0m;

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
                    minValue = (double)combineds.Min(item => item.Case.Cost.GetValueOrDefault());
                    MinPrice.Tag = "от " + minValue.ToString();

                }
                else
                {
                    minValue = double.Parse(MinPrice.Text);
                }

                // Проверка и установка максимальной цены
                if (string.IsNullOrWhiteSpace(MaxPrice.Text))
                {
                    maxValue = (double)combineds.Max(item => item.Case.Cost.GetValueOrDefault());
                    MaxPrice.Tag = "до " + maxValue.ToString();
                }
                else
                {
                    maxValue = double.Parse(MaxPrice.Text);
                }

                combineds = combineds.Where(item => item.Case.Cost.HasValue &&
                                                    item.Case.Cost.Value >= (decimal)minValue &&
                                                    item.Case.Cost.Value <= (decimal)maxValue).ToList();

                combineds = combineds.OrderBy(item => item.Case.Cost).ToList();

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

        private bool IsItemInCart(Cases cases)
        {
            if (currentUserCart != null)
            {
                return currentUserCart.CartItems.Any(item => item.CaseId == cases.CaseId);
            }
            return false;
        }

        private void AddToCartCaseButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var combinedData = button?.DataContext as CombinedData;

            if (combinedData != null)
            {
                var selectedCase = combinedData.Case;

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
                        CaseId = selectedCase.CaseId,
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

        private void AddToCartCaseButton_Loaded(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var combinedData = button?.DataContext as CombinedData;

            if (combinedData != null)
            {
                int userId = CurrentUser.Id;
                bool itemInCart;

                // Проверяем наличие корпуса в корзине на основе актуальных данных из базы данных
                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    var userCart = context.UsersCarts.Include("CartItems").FirstOrDefault(c => c.UserId == userId);

                    if (userCart != null)
                    {
                        itemInCart = userCart.CartItems.Any(item => item.CaseId == combinedData.Case.CaseId);
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

        private void ClearFields()
        {
            // Очистка текстовых полей
            NewCostTextBox.Text = "";
            NewCountOnStorageTextBox.Text = "";
            NewModelTextBox.Text = "";
            NewCaseMainColorTextBox.Text = "";
            NewCompatibleMotherboardFormFactorsTextBox.Text = "";
            NewFrontPanelIOPortsTextBox.Text = "";

            // Сброс выбранных элементов в комбобоксах
            NewCaseSizeNameComboBox.SelectedItem = null;

            // Очистка выбранного изображения (если применимо)
            selectedImageBytes = null;
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

        private void SaveAddNewCaseDialog_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на заполненность всех необходимых полей и выбора изображения
            if (string.IsNullOrWhiteSpace(NewCostTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewCountOnStorageTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewModelTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewCaseMainColorTextBox.Text) ||
                NewCaseSizeNameComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(NewCompatibleMotherboardFormFactorsTextBox.Text) ||
                string.IsNullOrWhiteSpace(NewFrontPanelIOPortsTextBox.Text) ||
                selectedImageBytes == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля и выберите фото.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Валидация стоимости
            decimal cost;
            if (!ValidateDecimalTextBox(NewCostTextBox, out cost))
                return;

            // Валидация количества на складе
            int storageCount;
            if (!ValidateIntTextBox(NewCountOnStorageTextBox, out storageCount))
                return;

            // Получение данных из текстовых полей
            string model = NewModelTextBox.Text;
            string mainColor = NewCaseMainColorTextBox.Text;
            string compatibleMotherboardFormFactors = NewCompatibleMotherboardFormFactorsTextBox.Text;
            string frontPanelIOPorts = NewFrontPanelIOPortsTextBox.Text;

            // Получение идентификатора выбранного размера корпуса
            int caseSizeId = ((CaseSizes)NewCaseSizeNameComboBox.SelectedItem).CaseSize_Id;

            // Создание нового объекта корпуса (Case)
            Cases newCase = new Cases
            {
                Cost = cost,
                Cases_Count_on_storage = storageCount,
                Model = model,
                Main_Color = mainColor,
                Compatible_Motherboard_Form_Factors = compatibleMotherboardFormFactors,
                Front_Panel_IO_Ports = frontPanelIOPorts,
                Case_Size = caseSizeId,
                Preview_Photo = selectedImageBytes
            };

            try
            {
                // Добавление нового корпуса в базу данных и сохранение изменений
                App.Database.Cases.Add(newCase);
                App.Database.SaveChanges();
                MessageBox.Show("Новый корпус успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                AddNewMotherBoardDialog.Visibility = Visibility.Collapsed; // Скрытие диалогового окна после сохранения
                LoadComponent(); // Перезагрузка компонента для отображения обновленного списка корпусов
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении корпуса в базу данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Очистка полей корпуса после сохранения
            ClearFields();
        }


        private void CloseAddNewCaseDialog_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
            AddNewMotherBoardDialog.Visibility = Visibility.Collapsed;
        }

        private void AddNewCaseButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewMotherBoardDialog.Visibility = Visibility.Visible;
        }

        private void DeleteSelectedCaseButton_Loaded(object sender, RoutedEventArgs e)
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

        private void DeleteSelectedCaseButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить этот корпус?", "Удаление корпуса", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Получение выбранного элемента ListBox
                var selectedItem = (sender as Button)?.DataContext as CombinedData;

                // Удаление записи из базы данных
                if (selectedItem != null)
                {
                    try
                    {
                        App.Database.Cases.Remove(selectedItem.Case);
                        App.Database.SaveChanges();
                        LoadComponent(); // Перезагрузка компонента для обновления списка корпусов
                        MessageBox.Show("Корпус успешно удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при удалении корпуса: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

    }
}
