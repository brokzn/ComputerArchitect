using ComputerArchitect.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
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

namespace ComputerArchitect.Pages.DeliveryPages
{
    /// <summary>
    /// Логика взаимодействия для OrderInProcessPage.xaml
    /// </summary>
    public partial class OrderInProcessPage : Page
    {
        public Users CurrentUser { get; set; }

        public OrderInProcessPage(Users currentUser)
        {
            InitializeComponent();
            CurrentUser = currentUser;
            LoadData();
        }

        private void LoadData()
        {
            var orderinfo = App.Database.Orders.Where(o => o.OrderId == CurrentUser.DeliveryOrderInProcess).FirstOrDefault();

            if (orderinfo != null)
            {
                OrderNumber.Content = "Информация о заказе №" + orderinfo.OrderId;
                OrderStatus.Content = "Статус: " + orderinfo.OrderStatuses.Name;
                OrderDate.Content = $"Дата создания: {orderinfo.OrderCreateDate:yyyy.MM.dd}";
                OrderCost.Content = $"Общая стоимость: {orderinfo.TotalCost:0}₽";
                OrderPaymentType.Content = $"Оплата: {orderinfo.PaymentMethodType.PaymentMethodName}";
                OrderDeliveryAdress.Content = $"Адрес доставки: {orderinfo.DeliveryAddress}";
                UserFSName.Content = $"ИФ: {orderinfo.Users.Name} {orderinfo.Users.SecondName}";
                UserPhoneNumber.Content = $"Номер телефона: {orderinfo.Users.PhoneNumber}";
                UserEmail.Content = $"Почта: {orderinfo.Users.Email}";



                

                ItemsListBox.ItemsSource = orderinfo.OrderCartItems.Where(oci => oci.OrderId == orderinfo.OrderId).ToList();










                NoOrdersLabel.Visibility = Visibility.Collapsed;
                OrderFullInfo.Visibility = Visibility.Visible;
            }
            else
            {
                NoOrdersLabel.Visibility = Visibility.Visible;
                OrderFullInfo.Visibility = Visibility.Collapsed;
            }
        }

        private void EndOrderButton_Click(object sender, RoutedEventArgs e)
        {
                EndOrderDialog.Visibility = Visibility.Visible;
                DialogBack.Visibility = Visibility.Visible;
        }

        private void EndOrderDialogYes_Click(object sender, RoutedEventArgs e)
        {
            var orderinfo = App.Database.Orders.Where(o => o.OrderId == CurrentUser.DeliveryOrderInProcess).FirstOrDefault();


            orderinfo.OrderStatusId = 3;
            App.Database.Orders.AddOrUpdate(orderinfo);
            App.Database.SaveChanges();


            CurrentUser.IsBusy = null;
            CurrentUser.DeliveryOrderInProcess = null;



            App.Database.Users.AddOrUpdate(CurrentUser);
            App.Database.SaveChanges();

            LoadData();

            EndOrderDialog.Visibility = Visibility.Collapsed;
            DialogBack.Visibility = Visibility.Collapsed;
        }

        private void EndOrderDialogNo_Click(object sender, RoutedEventArgs e)
        {
            EndOrderDialog.Visibility = Visibility.Collapsed;
            DialogBack.Visibility = Visibility.Collapsed;
        }
    }
}
