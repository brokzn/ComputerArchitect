using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using ComputerArchitect.UI.Pages;
using ComputerArchitect.ModalWindows;
using ComputerArchitect.Database;
using System.Runtime.InteropServices;
using System.Windows.Interop;


namespace ComputerArchitect
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        public MainWindow()
        {
            InitializeComponent();
            UserAuthorizationEmailTextBox.Text = "admin@mail.ru";
            UserPasswordAuthorizationPasswordBox.Password = "admin@mail.ru";
            UserPasswordAuthorizationTextBox.Text = "admin@mail.ru";
        }

        private void СloseAppButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAppNotification notification = new CloseAppNotification();
            notification.Closed += Notification_Closed;
            notification.Show();
            BlureRectangle.Visibility = Visibility.Visible;
        }

        private void Notification_Closed(object sender, EventArgs e)
        {
            BlureRectangle.Visibility = Visibility.Hidden;
        }

        private void TrayAppButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ResizeClick_Click(object sender, RoutedEventArgs e)
        {
            Resize2Click.Visibility = Visibility.Visible;
            ResizeClick.Visibility = Visibility.Hidden;

            var hwnd = new WindowInteropHelper(this).Handle;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            MONITORINFO monitorInfo = new MONITORINFO();
            monitorInfo.cbSize = Marshal.SizeOf(monitorInfo);
            if (GetMonitorInfo(monitor, ref monitorInfo))
            {
                var rcWork = monitorInfo.rcWork;
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.None;
                this.Top = rcWork.top;
                this.Left = rcWork.left;
                this.Width = rcWork.right - rcWork.left;
                this.Height = rcWork.bottom - rcWork.top;
                AppBorderWidth1.Width = new GridLength(0);
                AppBorderWidth2.Width = new GridLength(0);
                AppBorderHeight1.Height = new GridLength(0);
                AppBorderHeight2.Height = new GridLength(0);
            }
        }

        private void Resize2Click_Click(object sender, RoutedEventArgs e)
        {
            Resize2Click.Visibility = Visibility.Hidden;
            ResizeClick.Visibility = Visibility.Visible;
            WindowState = WindowState.Normal;
            this.Width = 1550;
            this.Height = 870;
            AppBorderWidth1.Width = new GridLength(3);
            AppBorderWidth2.Width = new GridLength(3);
            AppBorderHeight1.Height = new GridLength(3);
            AppBorderHeight2.Height = new GridLength(3);
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }


        //Контейнер регистрации

        //Имя пользователя
        private void UserNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UserNameTextBox.Tag = string.Empty;
            if (UserNameTextBox.Text == string.Empty)
            {
                UserNameErrorMsg.Visibility = Visibility.Collapsed;
            }
        }
        private void UserNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UserNameTextBox.Text))
            {
                UserNameSymbolCountErrorMsg.Visibility = Visibility.Collapsed;
                UserNameErrorMsg.Visibility = Visibility.Visible;
                UserNameTextBox.Tag = "Имя";
            }

        }
        private void UserNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text.Length < 2)
                {
                    UserNameSymbolCountErrorMsg.Visibility = Visibility.Visible;
                    UserNameTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CD6565"));
                }
                else
                {
                    UserNameSymbolCountErrorMsg.Visibility = Visibility.Collapsed;
                    UserNameTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF9F0"));
                }

                if (!string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Text = char.ToUpper(textBox.Text[0]) + textBox.Text.Substring(1);
                    textBox.CaretIndex = textBox.Text.Length;
                }

                UpdateRegistrationButtonState();
            };
        }
        private void UserNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-Zа-яА-Я]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        //Фамилия пользователя
        private void UserSecondNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UserSecondNameTextBox.Tag = string.Empty;
            if (UserSecondNameTextBox.Text == string.Empty)
            {
                UserSecondNameErrorMsg.Visibility = Visibility.Collapsed;
            }
        }
        private void UserSecondNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UserNameTextBox.Text))
            {
                UserSecondNameErrorMsg.Visibility = Visibility.Visible;
                UserSecondNameTextBox.Tag = "Фамилия";
                UserSecondNameSymbolCountErrorMsg.Visibility = Visibility.Collapsed;
            }
        }
        private void UserSecondNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text.Length < 2)
                {
                    UserSecondNameSymbolCountErrorMsg.Visibility = Visibility.Visible;
                    UserSecondNameTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CD6565"));
                }
                else
                {
                    UserSecondNameSymbolCountErrorMsg.Visibility = Visibility.Collapsed;
                    UserSecondNameTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF9F0"));
                }

                if (!string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Text = char.ToUpper(textBox.Text[0]) + textBox.Text.Substring(1);
                    textBox.CaretIndex = textBox.Text.Length;
                }

                UpdateRegistrationButtonState();
            }
        }
        private void UserSecondNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-Zа-яА-Я]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        //Мобильный телефон
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

            }
            else
            {
                PhoneErrorMsgEnterMask.Visibility = Visibility.Visible;
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
            }
            else
            {
                PhoneErrorMsgEnterMask.Visibility = Visibility.Collapsed;
            }

            if (string.IsNullOrEmpty(PhoneNumberTextBox.Text))
            {
                PhoneErrorMsgEnterMask.Visibility = Visibility.Collapsed;
                PhoneErrorMsgEnter.Visibility = Visibility.Visible;
                PhoneNumberTextBox.Tag = "000 000 00 00";
            }
            else
            {
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

        //Email
        //Проверка уникальности почты
        private bool IsUniqueEmail(string email)
        {

            bool isUnique = !App.Database.Users.Any(u => u.Email == email);

            if (!isUnique)
            {
                EmailErrorMsgAllReadyExist.Visibility = Visibility.Visible;
                UserRegistrationButton.IsEnabled = false;
            }
            EmailErrorMsgAllReadyExist.Visibility = Visibility.Collapsed;
            return isUnique;
        }
        private void UserEmailTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UserEmailTextBox.Tag = string.Empty;
            if (UserEmailTextBox.Text == string.Empty)
            {
                EmailErrorMsgEnter.Visibility = Visibility.Collapsed;
            }
        }
        private void UserEmailTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            EmailErrorMsgEnter.Visibility = Visibility.Collapsed;
            EmailErrorMsg.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(UserEmailTextBox.Text))
            {
                EmailErrorMsgEnter.Visibility = Visibility.Visible;
                UserEmailTextBox.Tag = "Email";
            }
        }
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

        private void UserEmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string email = UserEmailTextBox.Text;
            bool isEmailValid = IsValidEmail(email);
            bool isUniqueEmail = IsUniqueEmail(email);

            UpdateRegistrationButtonState();

            if (isEmailValid)
            {
                UserEmailTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF9F0"));
                EmailErrorMsg.Visibility = Visibility.Collapsed;
            }
            else
            {
                UserEmailTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CD6565"));
                EmailErrorMsg.Visibility = Visibility.Visible;
            }

            if (!isUniqueEmail)
            { 
                EmailErrorMsgAllReadyExist.Visibility = Visibility.Visible;
            }
        }


        //Поля паролей

        //Поле первичный пароль
        private void UserPasswordTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            imagePath = "pack://application:,,,/UI/Elements/OpenEye.png";
            image = new BitmapImage(new Uri(imagePath));
            PasswordVisibility.Source = image;
            UserPasswordPasswordBox.Password = UserPasswordTextBox.Text;
            PasswordVisibility.Visibility = Visibility.Visible;
            UserPasswordPasswordBox.Visibility = Visibility.Visible;
            UserPasswordPasswordBox.Focus();
        }
        private void UserPasswordTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UserPasswordTextBox.Text = string.Empty;

        }
        private bool PasswordV;
        string imagePath;
        BitmapImage image;
        private void PasswordVisibility_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (PasswordV == false)
            {
                UserPasswordPasswordBox.Visibility = Visibility.Collapsed;
                UserPasswordTextBox.Text = UserPasswordPasswordBox.Password;
                imagePath = "pack://application:,,,/UI/Elements/ClosedEye.png";
                image = new BitmapImage(new Uri(imagePath));
                PasswordVisibility.Source = image;
                PasswordVisibility.Height = 26;
                PasswordV = true;
                return;
            }
            UserPasswordPasswordBox.Visibility = Visibility.Visible;

            imagePath = "pack://application:,,,/UI/Elements/OpenEye.png";
            image = new BitmapImage(new Uri(imagePath));
            PasswordVisibility.Source = image;
            PasswordVisibility.Height = 26;
            PasswordV = false;
        }
        private void UserPasswordPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UserPasswordPasswordBox.Password))
            {
                UserPasswordPasswordBox.Visibility = Visibility.Collapsed;
                PasswordVisibility.Visibility = Visibility.Collapsed;
                UserPasswordTextBox.Text = string.Empty;
            }
        }
        private void UserPasswordPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateRegistrationButtonState();
            if (UserPasswordPasswordBox.Password != UserPasswordRepeatPasswordBox.Password)
            {

                UserPasswordRepeatPasswordBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CD6565"));
                PassErrorMsg.Visibility = Visibility.Visible;
            }
            else
            {
                UserPasswordRepeatPasswordBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF9F0"));
                PassErrorMsg.Visibility = Visibility.Collapsed;
            }
        }

        //Поле проверки первичного пароля
        private bool PasswordVRepeat;
        string imagePathRepeat;
        BitmapImage imageRepeat;
        private void PasswordRepeatVisibility_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PasswordVRepeat == false)
            {

                UserPasswordRepeatPasswordBox.Visibility = Visibility.Collapsed;
                UserPasswordRepeatTextBox.Text = UserPasswordRepeatPasswordBox.Password;
                imagePathRepeat = "pack://application:,,,/UI/Elements/ClosedEye.png";
                imageRepeat = new BitmapImage(new Uri(imagePathRepeat));
                PasswordRepeatVisibility.Source = imageRepeat;
                PasswordRepeatVisibility.Height = 26;
                PasswordVRepeat = true;
                
                return;
            }
            UserPasswordRepeatPasswordBox.Visibility = Visibility.Visible;
            
            imagePathRepeat = "pack://application:,,,/UI/Elements/OpenEye.png";
            imageRepeat = new BitmapImage(new Uri(imagePathRepeat));
            PasswordRepeatVisibility.Source = imageRepeat;
            PasswordRepeatVisibility.Height = 26;
            PasswordVRepeat = false;
        }

        private void UserPasswordRepeatPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UserPasswordRepeatPasswordBox.Password))
            {
                UserPasswordRepeatPasswordBox.Visibility = Visibility.Collapsed;
                PasswordRepeatVisibility.Visibility = Visibility.Collapsed;
                UserPasswordRepeatTextBox.Text = string.Empty;
            }
        }

        private void UserPasswordRepeatTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            imagePathRepeat = "pack://application:,,,/UI/Elements/OpenEye.png";
            imageRepeat = new BitmapImage(new Uri(imagePathRepeat));
            PasswordRepeatVisibility.Source = imageRepeat;
            UserPasswordRepeatPasswordBox.Password = UserPasswordRepeatTextBox.Text;
            PasswordRepeatVisibility.Visibility = Visibility.Visible;
            UserPasswordRepeatPasswordBox.Visibility = Visibility.Visible;
            UserPasswordRepeatPasswordBox.Focus();
        }
        private void UserPasswordRepeatTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UserPasswordRepeatTextBox.Text = string.Empty;
        }

        private void UserPasswordRepeatPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateRegistrationButtonState();
            if (UserPasswordPasswordBox.Password != UserPasswordRepeatPasswordBox.Password)
            {

                UserPasswordRepeatPasswordBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CD6565"));
                PassErrorMsg.Visibility = Visibility.Visible;
            }
            else
            {
                UserPasswordRepeatPasswordBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF9F0"));
                PassErrorMsg.Visibility = Visibility.Collapsed;
            }
        }
        //Соглашение с правилами
        private void RuleAcceptCB_Checked(object sender, RoutedEventArgs e)
        {
            UpdateRegistrationButtonState();
        }

        private void RuleAcceptCB_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateRegistrationButtonState();
        }
        //Проверка заполненности полей
        private void UpdateRegistrationButtonState()
        {
              bool isButtonEnabled = !string.IsNullOrEmpty(UserNameTextBox.Text)
             && !string.IsNullOrEmpty(UserSecondNameTextBox.Text)
             && PhoneNumberTextBox.Text.Length == 13
             && !string.IsNullOrEmpty(UserEmailTextBox.Text)
             && !string.IsNullOrEmpty(UserPasswordPasswordBox.Password)
             && !string.IsNullOrEmpty(UserPasswordRepeatPasswordBox.Password)
             && UserPasswordPasswordBox.Password == UserPasswordRepeatPasswordBox.Password
             && RuleAcceptCB.IsChecked == true
             && IsValidEmail(UserEmailTextBox.Text)
             && IsUniqueEmail(UserEmailTextBox.Text); 

            UserRegistrationButton.IsEnabled = isButtonEnabled;
            UserRegistrationButton.Background = isButtonEnabled ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#518EBA")) : Brushes.Gray;
        }
        
        //Пользовательское соглашение
        private void BackToRegistrationAuthorizationContainer_Click(object sender, RoutedEventArgs e)
        {
            MainContainer.Visibility = Visibility.Visible;
            UserAgreementContainer.Visibility = Visibility.Collapsed;
        }

        private void UserAgreementTB_MouseEnter(object sender, MouseEventArgs e)
        {
            UserAgreementTB.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6D6D6"));
        }

        private void UserAgreementTB_MouseLeave(object sender, MouseEventArgs e)
        {
            UserAgreementTB.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6DB2E3"));
        }

        private void UserAgreementTB_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainContainer.Visibility = Visibility.Collapsed;
            UserAgreementContainer.Visibility = Visibility.Visible;
        }



        //Контейнер регистрации
        //Переключение на регистрацию/авторизацию
        private void AuthorizationContainerLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            AuthorizationContainerLabel.Opacity = 1;
        }

        private void AuthorizationContainerLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            if (RegistrationContainerLabel.Opacity == 0.4)
            {
                AuthorizationContainerLabel.Opacity = 1;
            }
            else
            {
                AuthorizationContainerLabel.Opacity = 0.4;
            }

        }

        private void AuthorizationContainerLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            AuthorizationContainerLabel.Opacity = 1;
            RegistrationContainerLabel.Opacity = 0.4;
            AuthorizationContainer.Visibility = Visibility.Visible;
            RegistrationContainer.Visibility = Visibility.Collapsed;
        }

        private void RegistrationContainerLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            RegistrationContainerLabel.Opacity = 1;
        }

        private void RegistrationContainerLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            if (AuthorizationContainerLabel.Opacity == 0.4)
            {
                RegistrationContainerLabel.Opacity = 1;
            }
            else
            {
                RegistrationContainerLabel.Opacity = 0.4;
            }

        }

        private void RegistrationContainerLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AuthorizationContainerLabel.Opacity = 0.4;
            RegistrationContainerLabel.Opacity = 1;
            AuthorizationContainer.Visibility = Visibility.Collapsed;
            RegistrationContainer.Visibility = Visibility.Visible;
        }

        //Контейнер авторизации
        //Email

        private void UserAuthorizationEmailTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UserAuthorizationEmailTextBox.Tag = string.Empty;
            if (UserAuthorizationEmailTextBox.Text == string.Empty)
            {
                EmailAuthorizationErrorMsgEnter.Visibility = Visibility.Collapsed;
            }
        }

        private void UserAuthorizationEmailTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckFields();
            EmailAuthorizationErrorMsgEnter.Visibility = Visibility.Collapsed;
            EmailAuthorizationErrorMsg.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(UserAuthorizationEmailTextBox.Text))
            {
                EmailAuthorizationErrorMsgEnter.Visibility = Visibility.Visible;
                UserAuthorizationEmailTextBox.Tag = "Email";
            }
        }

        private void UserAuthorizationEmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckFields();
            string email = UserAuthorizationEmailTextBox.Text;
            bool isEmailValid = IsValidEmail(email);


            if (isEmailValid)
            {
                UserAuthorizationEmailTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF9F0"));
                EmailAuthorizationErrorMsg.Visibility = Visibility.Collapsed;
            }
            else
            {
                UserAuthorizationEmailTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CD6565"));
                EmailAuthorizationErrorMsg.Visibility = Visibility.Visible;
            }
        }



        //Пароль
        private bool PasswordVR;
        string imagePathR;
        BitmapImage imageR;

        //Проверка заполненности полей
        private void CheckFields()
        {
            bool isFieldsFilled = !string.IsNullOrEmpty(UserPasswordAuthorizationPasswordBox.Password)
                                && !string.IsNullOrEmpty(UserAuthorizationEmailTextBox.Text)
                                && IsValidEmail(UserAuthorizationEmailTextBox.Text);

            if (isFieldsFilled)
            {
                UserAuthorizationButton.IsEnabled = true;
            }
            else
            {

                UserAuthorizationButton.IsEnabled = false;
            }
        }
        private void PasswordAuthorizationVisibility_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PasswordVR == false)
            {
                UserPasswordAuthorizationPasswordBox.Visibility = Visibility.Collapsed;
                UserPasswordAuthorizationTextBox.Text = UserPasswordAuthorizationPasswordBox.Password;
                imagePathR = "pack://application:,,,/UI/Elements/ClosedEye.png";
                imageR = new BitmapImage(new Uri(imagePathR));
                PasswordAuthorizationVisibility.Source = imageR;
                PasswordAuthorizationVisibility.Height = 26;
                PasswordVR = true;
                return;
            }
            UserPasswordAuthorizationPasswordBox.Visibility = Visibility.Visible;

            imagePathR = "pack://application:,,,/UI/Elements/OpenEye.png";
            imageR = new BitmapImage(new Uri(imagePathR));
            PasswordAuthorizationVisibility.Source = imageR;
            PasswordAuthorizationVisibility.Height = 26;
            PasswordVR = false;
        }

        private void UserPasswordAuthorizationTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            imagePathR = "pack://application:,,,/UI/Elements/OpenEye.png";
            imageR = new BitmapImage(new Uri(imagePathR));
            PasswordAuthorizationVisibility.Source = imageR;
            UserPasswordAuthorizationPasswordBox.Password = UserPasswordAuthorizationTextBox.Text;
            PasswordAuthorizationVisibility.Visibility = Visibility.Visible;
            UserPasswordAuthorizationPasswordBox.Visibility = Visibility.Visible;
            UserPasswordAuthorizationPasswordBox.Focus();
        }

        private void UserPasswordAuthorizationTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UserPasswordTextBox.Text = string.Empty;
        }

        private void UserPasswordAuthorizationPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            CheckFields();
        }

        private void UserPasswordAuthorizationPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UserPasswordAuthorizationPasswordBox.Password))
            {
                PasswordAuthorizationErrorMsgEnter.Visibility = Visibility.Visible;
                UserPasswordAuthorizationPasswordBox.Visibility = Visibility.Collapsed;
                PasswordAuthorizationVisibility.Visibility = Visibility.Collapsed;
                UserPasswordAuthorizationTextBox.Text = string.Empty;
            }
            else
            {
                PasswordAuthorizationErrorMsgEnter.Visibility = Visibility.Collapsed;
            }
        }
        //Кнопка входа

        private void UserAuthorizationButton_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = App.Database.Users.FirstOrDefault(u => u.Email == UserAuthorizationEmailTextBox.Text && 
            u.Password == UserPasswordAuthorizationPasswordBox.Password);

            if (currentUser is null)
            {
                MessageBox.Show("Логин или пароль введены неверно!");
                return;
            }
            else
            {
                if (currentUser.UserIsBlocked is true)
                {
                    MessageBox.Show("Доступ к аккаунту заблокирован\n" + "Причина: " + currentUser.UserBlockReason);
                }
                else
                {
                    MainContainer.Visibility = Visibility.Collapsed;
                    UserAgreementContainer.Visibility = Visibility.Collapsed;
                    MainFrame.Visibility = Visibility.Visible;

                    MenuPage menuPage = new MenuPage(currentUser);

                    MainFrame.NavigationService.Navigate(menuPage);
                }
            }
        }


        //Кнопка регистрации
        private void UserRegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            
          ComputerArchitect.Database.Users user = new ComputerArchitect.Database.Users
          {
              Name = UserNameTextBox.Text,
              SecondName = UserSecondNameTextBox.Text,
              PhoneNumber = "+7 " + PhoneNumberTextBox.Text,
              Email = UserEmailTextBox.Text,
              Password = UserPasswordRepeatPasswordBox.Password,
              GenderId = 0,
              CountryId = 0,
              About = string.Empty,
          };

          using (var context = new ComputerArchitectDataBaseEntities())
          {
              context.Users.Add(user);
              context.SaveChanges();
          }
            //Отчистка введенных данных

           // MessageBox.Show("Успешная регистрация", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Asterisk);

            ClearTextBoxAndAttachLostFocus(UserNameTextBox, UserSecondNameTextBox_LostFocus);
            ClearTextBoxAndAttachLostFocus(UserSecondNameTextBox, UserSecondNameTextBox_LostFocus);
            ClearTextBoxAndAttachLostFocus(PhoneNumberTextBox, PhoneNumberTextBox_LostFocus);
            ClearTextBoxAndAttachLostFocus(UserEmailTextBox, UserEmailTextBox_LostFocus);
            ClearTextBoxAndAttachLostFocus(UserPasswordTextBox, UserPasswordTextBox_LostFocus);
            ClearTextBoxAndAttachLostFocus(UserPasswordRepeatTextBox, UserPasswordRepeatTextBox_LostFocus);
            ClearPasswordBoxAndAttachLostFocus(UserPasswordPasswordBox, UserPasswordPasswordBox_LostFocus);
            ClearPasswordBoxAndAttachLostFocus(UserPasswordRepeatPasswordBox, UserPasswordRepeatPasswordBox_LostFocus);
            AuthorizationContainer.Visibility = Visibility.Visible;
            RegistrationContainer.Visibility = Visibility.Collapsed;
            AuthorizationContainerLabel.Opacity = 1;
            RegistrationContainerLabel.Opacity = 0.4;
        }
        private void ClearTextBoxAndAttachLostFocus(TextBox textBox, RoutedEventHandler lostFocusHandler)
        {
            textBox.Text = string.Empty;
            textBox.LostFocus += lostFocusHandler;
        }

        private void ClearPasswordBoxAndAttachLostFocus(PasswordBox passwordBox, RoutedEventHandler lostFocusHandler)
        {
            passwordBox.Password = string.Empty;
            passwordBox.LostFocus += lostFocusHandler;
        }

        private void CloseWelcomeBorderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsInternetAvailable())
            {
                MessageBox.Show("Нет интернет-соединения.");
                return;
            }

            if (!IsConnected())
            {
                MessageBox.Show("Нет подключения к базе данных.");
                return;
            }

            WelcomeBorder.Visibility = Visibility.Collapsed;
        }

        public bool IsConnected()
        {

            using (var connection = new ComputerArchitectDataBaseEntities())
            {
                try
                {
                    connection.Database.Connection.Open();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }


        private bool IsInternetAvailable()
        {
            try
            {
                using (var client = new System.Net.WebClient())
                using (client.OpenRead("http://www.google.com/"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        
        

    }
}
