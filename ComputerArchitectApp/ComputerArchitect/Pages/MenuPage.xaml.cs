using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
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
using ComputerArchitect.Database;
using ComputerArchitect.ModalWindows;
using ComputerArchitect.Pages;


namespace ComputerArchitect.UI.Pages
{
    /// <summary>
    /// Логика взаимодействия для MenuPage.xaml
    /// </summary>
    public partial class MenuPage : Page
    {
        public Users CurrentUser { get; set; }
        public MenuPage(Users currentUser)
        {
            InitializeComponent();
            
            CurrentUser = currentUser;
            CurrentUserNameLabel.Content = CurrentUser.Name;
            MenuFrame.NavigationService.Navigate(new CatalogPage(CurrentUser));

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

                UserAvatarSmallImage.Source = bitmapImage;
            }
            else
            {
                UserAvatarSmallImage.Source = new BitmapImage(new Uri("/UI/Elements/UserMissedPictureBig.png", UriKind.Relative));
            }
        }
        


        bool SelectTheme = true;
        private void SwapThemeButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectTheme)
            {
                ThemeLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0B35E"));
                ThemeLabel.Content = "Cветлая";
                Sun.Visibility = Visibility.Visible;
                Moon.Visibility = Visibility.Collapsed;
            }
            else
            {
                ThemeLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#bdd0e4"));
                ThemeLabel.Content = "Тёмная";
                Sun.Visibility = Visibility.Collapsed;
                Moon.Visibility = Visibility.Visible;
            }
            SelectTheme = !SelectTheme;
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            LogoutAppNotification notification = new LogoutAppNotification();
            notification.Closed += (s, args) => { this.IsEnabled = true; };
            notification.Show();
            
        }


        private void UserProfileOpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (UMVisible)
            {
                UserMenu.Visibility = Visibility.Collapsed;
                UserMenuBorder.Visibility = Visibility.Collapsed;
            }
            else
            {
                UserMenu.Visibility = Visibility.Visible;
                UserMenuBorder.Visibility = Visibility.Visible;
            }
            UMVisible = !UMVisible;

            CurrentUserNameLabel.Content = CurrentUser.Name;


            if (CurrentUser.Photo != null && CurrentUser.Photo.Length > 0)
            {
                
                BitmapImage bitmapImage = new BitmapImage();
                using (MemoryStream stream = new MemoryStream(CurrentUser.Photo))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                }

                
                UserAvatarSmallImage.Source = bitmapImage;
            }
            else
            {
                UserAvatarSmallImage.Source = new BitmapImage(new Uri("/UI/Elements/UserMissedPictureBig.png", UriKind.Relative));
            }


            MenuFrame.NavigationService.Navigate(new UserProfilePage(CurrentUser));
        }

        private void NavigationLabelCatalog_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuFrame.NavigationService.Navigate(new CatalogPage(CurrentUser));
            
        }

        private void NavigationLabelReadyMadeAssembly_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuFrame.NavigationService.Navigate(new ReadyMadeAssembliesPage(CurrentUser));
        }

        private void NavigationLabelConfigurator_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuFrame.NavigationService.Navigate(new PCConfiguratorPage(CurrentUser));
        }

        private void UserCartOpenButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFrame.NavigationService.Navigate(new UserCartPage());
        }


        bool UMVisible = false;
        

        private void OpenUserMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.Photo != null && CurrentUser.Photo.Length > 0)
            {

                BitmapImage bitmapImage = new BitmapImage();
                using (MemoryStream stream = new MemoryStream(CurrentUser.Photo))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                }


                UserAvatarSmallImage.Source = bitmapImage;
            }
            else
            {
                UserAvatarSmallImage.Source = new BitmapImage(new Uri("/UI/Elements/UserMissedPictureBig.png", UriKind.Relative));
            }
            CurrentUserNameLabel.Content = CurrentUser.Name;
            if (UMVisible)
            {
                DoubleAnimation animation = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.2) 
                };
                animation.Completed += (s, _) => UserMenu.Visibility = Visibility.Collapsed;
                animation.Completed += (s, _) => UserMenuBorder.Visibility = Visibility.Collapsed;
                UserMenuBorder.BeginAnimation(UIElement.OpacityProperty, animation);
                UserMenuBorder.BeginAnimation(UIElement.OpacityProperty, animation);
            }
            else
            {
                UserMenu.Opacity = 0.01;
                UserMenu.Visibility = Visibility.Visible;
                UserMenuBorder.Opacity = 0.01;
                UserMenuBorder.Visibility = Visibility.Visible;
                DoubleAnimation animation = new DoubleAnimation
                {
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.2) 
                };

                UserMenu.BeginAnimation(UIElement.OpacityProperty, animation);
                UserMenuBorder.BeginAnimation(UIElement.OpacityProperty, animation);
            }
            UMVisible = !UMVisible;
        }

        
    }
}
