using ComputerArchitect.Database;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;



namespace ComputerArchitect.Pages
{
    /// <summary>
    /// Логика взаимодействия для UserProfilePage.xaml
    /// </summary>
    public partial class UserProfilePage : Page
    {
        private DispatcherTimer timer;
        public Users CurrentUser { get; set; }
        public UserProfilePage(Users currentUser)
        {
            //Загрузка данных текущего пользователя
            CurrentUser = currentUser;
            InitializeComponent();
            LoadUserOrdersHistoryListBoxData();
            //Загрузка списка стран из базы в InfoUserCountryCombo комбобокс
            List<Сountries> countries;

            using (var context = new ComputerArchitectDataBaseEntities())
            {
                countries = context.Сountries.ToList();
            }

            InfoUserCountryCombo.ItemsSource = countries;
            InfoUserCountryCombo.DisplayMemberPath = "CountryName";
            InfoUserCountryCombo.SelectedValuePath = "CountryId";

            if (currentUser != null)
            {
                InfoUserNameTextBox.Text = currentUser.Name;
                InfoUserSecondNameTextBox.Text = currentUser.SecondName;
                UserEmailTB.Text = currentUser.Email;

                if (currentUser.PhoneNumber.Length >= 13)
                {
                    string last10Digits = currentUser.PhoneNumber.Substring(currentUser.PhoneNumber.Length - 13);
                    PhoneNumberTextBox.Text = last10Digits;
                }


                if (currentUser.Photo != null && currentUser.Photo.Length > 0)
                {

                    BitmapImage bitmapImage = new BitmapImage();
                    using (MemoryStream stream = new MemoryStream(currentUser.Photo))
                    {
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = stream;
                        bitmapImage.EndInit();
                    }


                    UserAvatarBigImage.Source = bitmapImage;
                }
                else
                {

                    UserAvatarBigImage.Source = new BitmapImage(new Uri("/UI/Elements/UserMissedPictureBig.png", UriKind.Relative));
                }

                UserPasswordPB.Password = currentUser.Password;
                UserPasswordTB.Text = currentUser.Password;
                InfoUserAboutTextBox.Text = currentUser.About;

                int? countryId = currentUser.CountryId;
                Сountries selectedCountry = countries.FirstOrDefault(country => country.CountryId == countryId);

                if (selectedCountry != null)
                {
                    InfoUserCountryCombo.SelectedItem = selectedCountry;
                }
            }

            switch (currentUser.GenderId)
            {
                case 0:
                    RadioButtonNotPicked.IsChecked = true;
                    break;
                case 1:
                    RadioButtonGenderMale.IsChecked = true;
                    break;
                case 2:
                    RadioButtonGenderFemale.IsChecked = true;
                    break;
            }

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private void LoadUserOrdersHistoryListBoxData()
        {
            var completedUserOrders = App.Database.Orders
            .Where(o => o.UserId == CurrentUser.Id && o.OrderStatusId == 3)
            .ToList();


            UserOrdersHistoryListBox.ItemsSource = completedUserOrders;

            string MoneySpentCountCount = Convert.ToString(App.Database.Orders.Where(o => o.OrderStatusId == 3 && o.UserId == CurrentUser.Id).Sum(o => o.TotalCost));


            if(completedUserOrders.Count == 0)
            {
                MoneySpentCount.Content = "На данный момент у вас еще нет ни одного полученного заказа.";
            }
            else
            {
                MoneySpentCount.Content = "Всего получено заказов на сумму: " + MoneySpentCountCount + " ₽";
            }
  
        }


        //Таймер исчезновения уведомления об успешном обновлении
        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateSecAcceptMessage.Visibility = Visibility.Collapsed;
            UpdateInfAcceptMessage.Visibility = Visibility.Collapsed;
            timer.Stop();
        }

        //Контейнер Персональные данные
        private void ShowUserInfoGridButton_Click(object sender, RoutedEventArgs e)
        {
            UserOrderHistoryGrid.Visibility = Visibility.Collapsed;
            UserInfoGrid.Visibility = Visibility.Visible;
            UserSecurityGrid.Visibility = Visibility.Collapsed;
        }
        //Кнопка обновления персональных данных
        private void UpdateUserInfo_Click(object sender, RoutedEventArgs e)
        {

            UpdateInfAcceptMessage.Visibility = Visibility.Visible;
            timer.Start();

            Users currentUser = CurrentUser;

            if (currentUser != null)
            {
                if (InfoUserCountryCombo.SelectedItem is Сountries selectedCountry)
                {
                    currentUser.CountryId = selectedCountry.CountryId;
                }

                currentUser.Name = InfoUserNameTextBox.Text;
                currentUser.SecondName = InfoUserSecondNameTextBox.Text;

                if (RadioButtonGenderMale.IsChecked == true)
                {
                    currentUser.GenderId = 1;
                }
                else if (RadioButtonGenderFemale.IsChecked == true)
                {
                    currentUser.GenderId = 2;
                }
                else
                {
                    currentUser.GenderId = 0;
                }

                currentUser.About = InfoUserAboutTextBox.Text;

                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    context.Users.AddOrUpdate(currentUser);
                    context.SaveChanges();
                }
            }
        }
        //Выбор нового фото профиля
        private void UserAvatarTake_MouseEnter(object sender, MouseEventArgs e)
        {
            UserAvatarTake.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#518EBA"));
        }

        private void UserAvatarTake_MouseLeave(object sender, MouseEventArgs e)
        {
            UserAvatarTake.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6D6D6"));
        }

        private void UserAvatarTake_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Изображения (*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp|Все файлы (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedImagePath = openFileDialog.FileName;

                // Проверяем расширение файла
                string fileExtension = System.IO.Path.GetExtension(selectedImagePath);
                if (fileExtension != ".jpg" && fileExtension != ".jpeg" && fileExtension != ".png" && fileExtension != ".bmp")
                {
                    ImageSelectError.Visibility = Visibility.Visible;
                    return;
                }
                ImageSelectError.Visibility = Visibility.Collapsed;
                byte[] imageBytes = File.ReadAllBytes(selectedImagePath);

                BitmapImage bitmapImage = new BitmapImage();
                using (MemoryStream stream = new MemoryStream(imageBytes))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                }

                UserAvatarBigImage.Source = bitmapImage;

                CurrentUser.Photo = imageBytes;
            }
        }




        //Контейнер Безопасность
        private void ShowUserSecurityGridButton_Click(object sender, RoutedEventArgs e)
        {
            UserOrderHistoryGrid.Visibility = Visibility.Collapsed;
            UserInfoGrid.Visibility = Visibility.Collapsed;
            UserSecurityGrid.Visibility = Visibility.Visible;
        }

        bool setNewPassClick = true;
        //Пароль пользователя
        private void SetNewPassLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (setNewPassClick)
            {
                UserPasswordPB.Visibility = Visibility.Collapsed;
                UserPasswordTB.Text = UserPasswordPB.Password;
                UserPasswordTB.Visibility = Visibility.Visible;
                CloseEye.Visibility = Visibility.Visible;
                OpenEye.Visibility = Visibility.Collapsed;
            }
            else
            {
                UserPasswordPB.Visibility = Visibility.Visible;
                UserPasswordTB.Text = UserPasswordPB.Password;
                UserPasswordTB.Visibility = Visibility.Collapsed;
                CloseEye.Visibility = Visibility.Collapsed;
                OpenEye.Visibility = Visibility.Visible;
            }
            setNewPassClick = !setNewPassClick;      
        }

        private void UserPasswordTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(UserPasswordTB.Text))
            {
                PasswordErrorMsg.Visibility = Visibility.Collapsed;
                UpdateSecurityinfo.IsEnabled = true;
            }
            else
            {
                PasswordErrorMsg.Visibility = Visibility.Visible;
                UpdateSecurityinfo.IsEnabled = false;
            }
        }

        private void CloseEye_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UserPasswordPB.Visibility = Visibility.Visible;
            UserPasswordTB.Text = UserPasswordPB.Password;
            UserPasswordTB.Visibility = Visibility.Collapsed;
            CloseEye.Visibility = Visibility.Collapsed;
            OpenEye.Visibility = Visibility.Visible;
            UserPasswordPB.IsEnabled = false;
        }

        private void OpenEye_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UserPasswordPB.Visibility = Visibility.Collapsed;
            UserPasswordTB.Text = UserPasswordPB.Password;
            UserPasswordTB.Visibility = Visibility.Visible;
            CloseEye.Visibility = Visibility.Visible;
            OpenEye.Visibility = Visibility.Collapsed;
            UserPasswordPB.IsEnabled = true;
        }
        //Кнопка обновления безопасности пользователя
        private void UpdateSecurityinfo_Click(object sender, RoutedEventArgs e)
        {
            Users currentUser = CurrentUser;
            if (currentUser != null)
            {

                currentUser.Email = UserEmailTB.Text;
                currentUser.Password = UserPasswordTB.Text;
                currentUser.PhoneNumber = "+7 " + PhoneNumberTextBox.Text;

                using (var context = new ComputerArchitectDataBaseEntities())
                {

                    context.Users.AddOrUpdate(currentUser);
                    context.SaveChanges();
                }


                UpdateSecAcceptMessage.Visibility = Visibility.Visible;

                
                timer.Start();

            }
            UserPasswordPB.Password = UserPasswordTB.Text;
        }
        //Email
        //Проверка валидности почты
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        //Проверка уникальности почты
        private bool IsUniqueEmail(string email, Users currentUser)
        {
            bool isUnique = !App.Database.Users.Any(u => u.Email == email && u.Id != currentUser.Id);

            if (!isUnique)
            {
                EmailErrorMsgAllReadyExist.Visibility = Visibility.Visible;
                UpdateSecurityinfo.IsEnabled = false;
            }
            else
            {
                UpdateSecurityinfo.IsEnabled = true;
                EmailErrorMsgAllReadyExist.Visibility = Visibility.Collapsed;
            }

            return isUnique;
        }

        private void UserEmailTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            Users currentUser = CurrentUser;
            string email = UserEmailTB.Text;
            bool isEmailValid = IsValidEmail(email);
            bool isUniqueEmail = IsUniqueEmail(email, currentUser);

            if (isEmailValid)
            {
                UserEmailTB.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF9F0"));
                EmailErrorMsg.Visibility = Visibility.Collapsed;
            }
            else
            {
                UserEmailTB.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CD6565"));
                EmailErrorMsg.Visibility = Visibility.Visible;
            }

            if (!isUniqueEmail)
            {
                EmailErrorMsgAllReadyExist.Visibility = Visibility.Visible;
            }
            else
            {
                EmailErrorMsgAllReadyExist.Visibility = Visibility.Collapsed;
            }
        }

        //Телефон
        private void PhoneNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }

        private void PhoneNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string digitsOnly = new string(PhoneNumberTextBox.Text.Where(char.IsDigit).ToArray());

            if (PhoneNumberTextBox.Text.Length == 13)
            {
                PhoneErrorMsgEnterMask.Visibility = Visibility.Collapsed;
                UpdateSecurityinfo.IsEnabled = true;
            }
            else
            {
                PhoneErrorMsgEnterMask.Visibility = Visibility.Visible;
                UpdateSecurityinfo.IsEnabled = false;
            }

            string formattedNumber = "";

            if (digitsOnly.Length >= 1)
            {
                formattedNumber += digitsOnly.Substring(0, Math.Min(3, digitsOnly.Length));
            }

            if (digitsOnly.Length >= 4)
            {
                formattedNumber += " " + digitsOnly.Substring(3, Math.Min(3, digitsOnly.Length - 3));
            }

            if (digitsOnly.Length >= 7)
            {
                formattedNumber += " " + digitsOnly.Substring(6, Math.Min(2, digitsOnly.Length - 6));
            }

            if (digitsOnly.Length >= 9)
            {
                formattedNumber += " " + digitsOnly.Substring(8, Math.Min(2, digitsOnly.Length - 8));
            }

            PhoneNumberTextBox.TextChanged -= PhoneNumberTextBox_TextChanged;
            PhoneNumberTextBox.Text = formattedNumber;
            PhoneNumberTextBox.TextChanged += PhoneNumberTextBox_TextChanged;

            PhoneNumberTextBox.CaretIndex = formattedNumber.Length;
        }

        private void PhoneNumberTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PhoneNumberTextBox.Text.Length < 13)
            {
                PhoneErrorMsgEnterMask.Visibility = Visibility.Visible;
                UpdateSecurityinfo.IsEnabled = false;
            }
            else
            {
                PhoneErrorMsgEnterMask.Visibility = Visibility.Collapsed;
                UpdateSecurityinfo.IsEnabled = true;
            }

            if (string.IsNullOrEmpty(PhoneNumberTextBox.Text))
            {
                UpdateSecurityinfo.IsEnabled = false;
                PhoneErrorMsgEnterMask.Visibility = Visibility.Collapsed;
                PhoneErrorMsgEnter.Visibility = Visibility.Visible;
                PhoneNumberTextBox.Tag = "000 000 00 00";
            }
            else
            {
                UpdateSecurityinfo.IsEnabled = true;
                PhoneErrorMsgEnter.Visibility = Visibility.Collapsed;
            }
        }

        private void PhoneNumberTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PhoneNumberTextBox.Tag = string.Empty;
            if (PhoneNumberTextBox.Text == string.Empty)
            {
                PhoneErrorMsgEnter.Visibility = Visibility.Collapsed;
            }
        }

        private void UserOrdersHistory_Click(object sender, RoutedEventArgs e)
        {
            UserOrderHistoryGrid.Visibility = Visibility.Visible;
            UserInfoGrid.Visibility = Visibility.Collapsed;
            UserSecurityGrid.Visibility = Visibility.Collapsed;
        }
    }
}
