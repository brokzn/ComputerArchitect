using ComputerArchitect.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using ComputerArchitect.Pages.DeliveryPages;

namespace ComputerArchitect.Pages.DeliveryPages
{
    /// <summary>
    /// Логика взаимодействия для WaitingOrdersPage.xaml
    /// </summary>
    public partial class WaitingOrdersPage : Page
    {
        public Users CurrentUser { get; }
        public WaitingOrdersPage(Users currentUser)
        {
            InitializeComponent();
            CurrentUser = currentUser;
            LoadData();
        }

        private void LoadData()
        {
            var userOrders = App.Database.Orders
                .Where(o => o.OrderStatusId == 1 && o.DeliveryMethod == 2)
                .ToList();


            WaitingOrdersListBox.ItemsSource = userOrders;

            if (CurrentUser.IsBusy == 1)
            {
                WaitingOrdersListBox.Visibility = Visibility.Collapsed;
                IsBusyTB.Visibility = Visibility.Visible;
            }
            else
            {
                WaitingOrdersListBox.Visibility = Visibility.Visible;
                IsBusyTB.Visibility = Visibility.Collapsed;
            }

            if (userOrders.Count == 0)
            {
                NoOrdersTB.Visibility = Visibility.Visible;   
            }
            else
            {
                NoOrdersTB.Visibility = Visibility.Collapsed;
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

        private void OrderLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Отображаем диалоговое окно и задний фон
                StartOrderDialog.Visibility = Visibility.Visible;
                DialogBack.Visibility = Visibility.Visible;

                // Находим родительский элемент ListBoxItem, используя метод FindAncestor
                ListBoxItem listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)sender);

                // Проверяем, что удалось найти ListBoxItem и его DataContext является объектом Orders
                if (listBoxItem != null && listBoxItem.DataContext is Orders)
                {
                    // Присваиваем lastSelectedOrder выбранный заказ
                    lastSelectedOrder = (Orders)listBoxItem.DataContext;
                }
                else
                {
                    throw new Exception("Не удалось найти выбранный заказ.");
                }
            }
            catch (Exception ex)
            {
                // Обрабатываем исключение, если что-то пошло не так
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Orders lastSelectedOrder;

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
        private void DialogYes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, что CurrentUser не равен null и имеет необходимые свойства
                if (CurrentUser == null)
                {
                    throw new Exception("Не удалось получить текущего пользователя.");
                }

                // Устанавливаем значения свойств для CurrentUser
                CurrentUser.IsBusy = 1;
                CurrentUser.DeliveryOrderInProcess = lastSelectedOrder?.OrderId; // Убедитесь, что lastSelectedOrder не равен null

                lastSelectedOrder.OrderStatusId = 8;

               

                // Получаем контекст базы данных из App.Database
                using (var context = new ComputerArchitectDataBaseEntities())
                {

                    context.Orders.AddOrUpdate(lastSelectedOrder);

                    // Добавляем или обновляем запись о CurrentUser в таблице Users
                    context.Users.AddOrUpdate(u => u.Id, CurrentUser);

                    // Сохраняем изменения в базе данных
                    context.SaveChanges();
                }

                LoadData();
                StartOrderDialog.Visibility = Visibility.Collapsed;
                DialogBack.Visibility = Visibility.Collapsed;

            }
            catch (Exception ex)
            {
                // Обрабатываем исключения, возникающие в процессе выполнения операции
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DialogNo_Click(object sender, RoutedEventArgs e)
        {
            StartOrderDialog.Visibility = Visibility.Collapsed;
            DialogBack.Visibility = Visibility.Collapsed;
        }
    }
}
