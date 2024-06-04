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
            
            switch (CurrentUser.RoleId)
            {
                case 1:
                    AdminPanelOpenButton.Visibility = Visibility.Visible;
                    break;
                case 3:
                    DeliveryPanelOpenButton.Visibility = Visibility.Visible;
                    break;
            }

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

            MenuFrame.Navigated += (sender, e) =>
            {
                UpdateCounts();
                switch (MenuFrame.Content)
                {
                    case CPUPage cpuPage:
                    case MotherBoardPage motherboards:
                    case GPUPage gpuPage:
                    case RAMPage ramPage:
                    case PowerSuppliesPage powerSuppliesPage:
                    case CasePage casePage:
                    case CoolerPage coolerPage:
                    case HDDPage hddPage:
                    case PCConfiguratorPage pccPage:
                    case ReadyMadeAssembliesPage RMAP:
                    case UserCartPage userCartPage:
                    case CreateOrderPage createOrderPage:
                    case UserOrdersPage userOrdersPage:
                        EventHandler cartUpdatedHandler = (obj, args) =>
                        {
                            UpdateCounts();
                        };
                        (MenuFrame.Content as dynamic).CartUpdated += cartUpdatedHandler;
                        break;
                    default:
                        break;
                }
            };
        }
        private void UpdateCounts()
        {
            int itemCount = GetItemCountInCart();
            UpdateItemCountInCart(itemCount);

            int ordersCount = GetUserOrdersCount();
            UpdateUserOrdersCount(ordersCount);
        }


        private void UpdateItemCountInCart(int count)
        {
            if (count >= 10)
            {
                ItemCountBack.Width = 27;
                ItemCountLabel.Content = count.ToString();
            }
            else if (count >= 99)
            { 
            ItemCountLabel.Content = "99+";
            ItemCountBack.Width = 37;
            }
            else
            {
                ItemCountBack.Width = 20;
                ItemCountLabel.Content = count.ToString();
            }

            
            ItemCountLabel.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
            ItemCountBack.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private int GetItemCountInCart()
        {
            if (CurrentUser != null)
            {
                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    var userCart = context.UsersCarts
                        .Include("CartItems")
                        .FirstOrDefault(c => c.UserId == CurrentUser.Id);

                    if (userCart != null)
                    {
                        return userCart.CartItems.Count;
                    }
                }
            }
            return 0; 
        }


        private void UpdateUserOrdersCount(int orderscount)
        {
            if (orderscount >= 10)
            {
                UserOrdersCountBack.Width = 27;
                UserOrdersCountLabel.Content = orderscount.ToString();
            }
            else if (orderscount >= 99)
            {
                UserOrdersCountLabel.Content = "99+";
                UserOrdersCountBack.Width = 37;
            }
            else
            {
                UserOrdersCountBack.Width = 20;
                UserOrdersCountLabel.Content = orderscount.ToString();
            }

            UserOrdersCountLabel.Visibility = orderscount > 0 ? Visibility.Visible : Visibility.Collapsed;
            UserOrdersCountBack.Visibility = orderscount > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private int GetUserOrdersCount()
        {
            if (CurrentUser != null)
            {
                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    // Получаем количество заказов пользователя
                    var orderscount = context.Orders
                        .Count(o => o.UserId == CurrentUser.Id && o.OrderStatuses.Id == 1);

                    return orderscount;
                }
            }
            return 0;
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
            MenuFrame.NavigationService.Navigate(new UserCartPage(CurrentUser));
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            int itemCount = GetItemCountInCart();
            UpdateItemCountInCart(itemCount);
        }

        private void AdminPanelOpenButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFrame.NavigationService.Navigate(new AdminPanelPage(CurrentUser));
        }

        private void ManagerPanelOpenButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFrame.NavigationService.Navigate(new ManagerPanelPage(CurrentUser));
        }

        private void DeliveryPanelOpenButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFrame.NavigationService.Navigate(new DeliveryPanelPage(CurrentUser));
        }

        private void InfoAboutProgramOpenButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFrame.NavigationService.Navigate(new InfoAboutProgramPage());
        }

        private void OrderOpenButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFrame.NavigationService.Navigate(new UserOrdersPage(CurrentUser));
        }
    }
}
