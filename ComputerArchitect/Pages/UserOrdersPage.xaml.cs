using ComputerArchitect.Database;
using System;
using System.Collections.Generic;
using System.Dynamic;
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
    /// Логика взаимодействия для UserOrdersPage.xaml
    /// </summary>
    public partial class UserOrdersPage : Page
    {
        public event EventHandler CartUpdated;
        public Users CurrentUser { get; set; }
        public UserOrdersPage(Users currentUser)
        {
            CurrentUser = currentUser;
            InitializeComponent();
            LoadUserOrderListBoxData();
        }

        private void LoadUserOrderListBoxData()
        {
            
            var userOrders = App.Database.Orders
                .Where(o => o.UserId == CurrentUser.Id && o.OrderStatusId == 1)
                .ToList();

           
            UserOrdersListBox.ItemsSource = userOrders;

            
            if (userOrders.Count == 0)
            {
                
                EmptyOrdersLabel.Visibility = Visibility.Visible;
                CartLink.Visibility = Visibility.Visible;
            }
            else
            {
                
                EmptyOrdersLabel.Visibility = Visibility.Collapsed;
                CartLink.Visibility = Visibility.Collapsed;
            }
        }



        private void OrderLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock OrderLabel)
            {
                OrderLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6DB2E3"));
            }
        }

        private void OrderLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock OrderLabel)
            {
                OrderLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6D6D6"));
            }
        }

        private Orders lastSelectedOrder;

        private void OrderLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)sender);

            if (listBoxItem != null)
            {
                var order = listBoxItem.DataContext as Orders;

                lastSelectedOrder = listBoxItem.DataContext as Orders;


                UserOrderItemsListBox.ItemsSource = lastSelectedOrder.OrderCartItems.Where(oci=> oci.OrderId == order.OrderId).ToList();


                if (order != null)
                {
                    OrderId.Content = $"Заказ №{order.OrderId}";
                    OrderStatus.Content = $"Статус: {order.OrderStatuses.Name}";
                    OrderDate.Content = $"Дата создания: {order.OrderCreateDate:yyyy.MM.dd}";
                    OrderCost.Content = $"Общая стоимость: {order.TotalCost:0}₽";
                    OrderPaymentType.Content = $"Оплата: {order.PaymentMethodType.PaymentMethodName}";
                    OrderDeliveryType.Content = $"Способ получения: {order.DeliveryMethodType.DeliveryMethodName}";
                }


                DialogBack.Visibility = Visibility.Visible;
                OrderDialog.Visibility = Visibility.Visible;

            }
        }
        private T FindAncestor<T>(DependencyObject current)
        where T : DependencyObject
        {
            do
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            } while (current != null);
            return null;
        }

        private void CloseOrderInfoDialog_Click(object sender, RoutedEventArgs e)
        {
            DialogBack.Visibility = Visibility.Collapsed;
            OrderDialog.Visibility = Visibility.Collapsed;
        }


        private void DialogYes_Click(object sender, RoutedEventArgs e)
        {
            if (lastSelectedOrder != null)
            {
                try
                {
                    lastSelectedOrder.OrderStatusId = 2;
                    App.Database.SaveChanges();
                    LoadUserOrderListBoxData();
                    lastSelectedOrder = null;

                    CartUpdated?.Invoke(this, EventArgs.Empty);

                    DialogBack.Visibility = Visibility.Collapsed;
                    OrderDialog.Visibility = Visibility.Collapsed;
                    DeleteDialogBack.Visibility = Visibility.Collapsed;
                    DeleteDialog.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при изменении статуса заказа: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для изменения статуса.");
            }
        }



        private void DialogNo_Click(object sender, RoutedEventArgs e)
        {
            DeleteDialogBack.Visibility = Visibility.Collapsed;
            DeleteDialog.Visibility = Visibility.Collapsed;
        }

        private void DeleteOrderInfoDialog_Click(object sender, RoutedEventArgs e)
        {
            DeleteDialogBack.Visibility = Visibility.Visible;
            DeleteDialog.Visibility = Visibility.Visible;
        }


        private void CartLink_MouseEnter(object sender, MouseEventArgs e)
        {
            CartLink.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6D6D6"));
        }

        private void CartLink_MouseLeave(object sender, MouseEventArgs e)
        {
            CartLink.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6DB2E3"));
        }

        private void CartLink_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new UserCartPage(CurrentUser));
        }
    }
}
