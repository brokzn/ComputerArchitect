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

namespace ComputerArchitect.Pages.AdminPages
{
    /// <summary>
    /// Логика взаимодействия для SelfTakeOrdersPage.xaml
    /// </summary>
    public partial class SelfTakeOrdersPage : Page
    {
        public Users CurrentUser { get; set; }
        public SelfTakeOrdersPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var userOrders = App.Database.Orders
                .Where(o => o.OrderStatusId == 1 && o.DeliveryMethod == 1)
                .ToList();


            WaitingOrdersListBox.ItemsSource = userOrders;

           

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
        private Orders lastSelectedOrder;
        private void OrderLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)sender);

            if (listBoxItem != null)
            {
                var order = listBoxItem.DataContext as Orders;

                lastSelectedOrder = listBoxItem.DataContext as Orders;


                ItemsListBox.ItemsSource = lastSelectedOrder.OrderCartItems.Where(oci => oci.OrderId == order.OrderId).ToList();


                if (order != null)
                {
                    OrderNumber.Content = $"Заказ №{order.OrderId}";
                    OrderStatus.Content = $"Статус: {order.OrderStatuses.Name}";
                    OrderDate.Content = $"Дата создания: {order.OrderCreateDate:yyyy.MM.dd}";
                    OrderCost.Content = $"Общая стоимость: {order.TotalCost:N0}₽";
                    OrderPaymentType.Content = $"Оплата: {order.PaymentMethodType.PaymentMethodName}";
                    UserFSName.Content = $"ИФ: {order.Users.Name} {order.Users.SecondName}";
                    UserPhoneNumber.Content = $"Номер телефона: {order.Users.PhoneNumber}";
                    UserEmail.Content = $"Почта: {order.Users.Email}";

                }



                DialogBack.Visibility = Visibility.Visible;
                OrderFullInfo.Visibility = Visibility.Visible;

            }
        }

        private void EndOrderButton_Click(object sender, RoutedEventArgs e)
        {
            // Получение информации о заказе
            var orderinfo = App.Database.Orders
                                .Where(o => o.OrderId == lastSelectedOrder.OrderId)
                                .FirstOrDefault();

            if (orderinfo != null)
            {
                // Установка статуса заказа как завершенного
                orderinfo.OrderStatusId = 3;
                App.Database.Orders.AddOrUpdate(orderinfo);
                App.Database.SaveChanges();

                foreach (var orderCartItem in orderinfo.OrderCartItems)
                {
                    if (orderCartItem.CpuId.HasValue)
                    {
                        var cpu = App.Database.CPUS.Where(c => c.CPUId == orderCartItem.CpuId.Value).FirstOrDefault();
                        if (cpu != null && cpu.CPU_Count_on_storage > 0)
                        {
                            cpu.CPU_Count_on_storage -= 1;
                            App.Database.CPUS.AddOrUpdate(cpu);
                        }
                    }

                    if (orderCartItem.MotherboardId.HasValue)
                    {
                        var motherboard = App.Database.Motherboards.Where(m => m.MotherboardId == orderCartItem.MotherboardId.Value).FirstOrDefault();
                        if (motherboard != null && motherboard.Motherboard_Count_on_storage > 0)
                        {
                            motherboard.Motherboard_Count_on_storage -= 1;
                            App.Database.Motherboards.AddOrUpdate(motherboard);
                        }
                    }

                    if (orderCartItem.GPUId.HasValue)
                    {
                        var gpu = App.Database.GPUS.Where(g => g.GPUId == orderCartItem.GPUId.Value).FirstOrDefault();
                        if (gpu != null && gpu.GPU_Count_on_storage > 0)
                        {
                            gpu.GPU_Count_on_storage -= 1;
                            App.Database.GPUS.AddOrUpdate(gpu);
                        }
                    }

                    if (orderCartItem.CaseId.HasValue)
                    {
                        var caseComponent = App.Database.Cases.Where(c => c.CaseId == orderCartItem.CaseId.Value).FirstOrDefault();
                        if (caseComponent != null && caseComponent.Cases_Count_on_storage > 0)
                        {
                            caseComponent.Cases_Count_on_storage -= 1;
                            App.Database.Cases.AddOrUpdate(caseComponent);
                        }
                    }

                    if (orderCartItem.FanId.HasValue)
                    {
                        var fan = App.Database.Coolers.Where(f => f.CoolerId == orderCartItem.FanId.Value).FirstOrDefault();
                        if (fan != null && fan.Cooler_Count_on_storage > 0)
                        {
                            fan.Cooler_Count_on_storage -= 1;
                            App.Database.Coolers.AddOrUpdate(fan);
                        }
                    }

                    if (orderCartItem.RAMId.HasValue)
                    {
                        var ram = App.Database.RAMS.Where(r => r.RAMId == orderCartItem.RAMId.Value).FirstOrDefault();
                        if (ram != null && ram.RAM_Count_on_storage > 0)
                        {
                            ram.RAM_Count_on_storage -= 1;
                            App.Database.RAMS.AddOrUpdate(ram);
                        }
                    }

                    if (orderCartItem.MemoryId.HasValue)
                    {
                        var memory = App.Database.HDDs.Where(h => h.HDDId == orderCartItem.MemoryId.Value).FirstOrDefault();
                        if (memory != null && memory.HDD_Count_on_storage > 0)
                        {
                            memory.HDD_Count_on_storage -= 1;
                            App.Database.HDDs.AddOrUpdate(memory);
                        }
                    }

                    if (orderCartItem.PowerSuppliesId.HasValue)
                    {
                        var powerSupply = App.Database.PowerSupplies.Where(p => p.PowerSupplyId == orderCartItem.PowerSuppliesId.Value).FirstOrDefault();
                        if (powerSupply != null && powerSupply.PS__Count_on_storage > 0)
                        {
                            powerSupply.PS__Count_on_storage -= 1;
                            App.Database.PowerSupplies.AddOrUpdate(powerSupply);
                        }
                    }

                    
                }

                App.Database.SaveChanges();

                // Перезагрузка данных
                LoadData();

                // Закрытие диалога
                DialogBack.Visibility = Visibility.Collapsed;
                OrderFullInfo.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseInfoOrderButton_Click(object sender, RoutedEventArgs e)
        {
            DialogBack.Visibility = Visibility.Collapsed;
            OrderFullInfo.Visibility = Visibility.Collapsed;
        }
    }
}
