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
            // Получение информации о заказе
            var orderinfo = App.Database.Orders
                                .Where(o => o.OrderId == CurrentUser.DeliveryOrderInProcess)
                                .FirstOrDefault();

            if (orderinfo != null)
            {
                // Установка статуса заказа как завершенного
                orderinfo.OrderStatusId = 3;
                App.Database.Orders.AddOrUpdate(orderinfo);
                App.Database.SaveChanges();

                // Обновление количества компонентов на складе
                foreach (var orderCartItem in orderinfo.OrderCartItems)
                {
                    if (orderCartItem.CpuId.HasValue)
                    {
                        var cpu = App.Database.CPUS.Where(c => c.CPUId == orderCartItem.CpuId.Value).FirstOrDefault();
                        if (cpu != null)
                        {
                            cpu.CPU_Count_on_storage -= 1; // Предполагаем, что количество каждого компонента в заказе всегда 1
                            App.Database.CPUS.AddOrUpdate(cpu);
                        }
                    }

                    if (orderCartItem.MotherboardId.HasValue)
                    {
                        var motherboard = App.Database.Motherboards.Where(m => m.MotherboardId == orderCartItem.MotherboardId.Value).FirstOrDefault();
                        if (motherboard != null)
                        {
                            motherboard.Motherboard_Count_on_storage -= 1;
                            App.Database.Motherboards.AddOrUpdate(motherboard);
                        }
                    }

                    if (orderCartItem.GPUId.HasValue)
                    {
                        var gpu = App.Database.GPUS.Where(g => g.GPUId == orderCartItem.GPUId.Value).FirstOrDefault();
                        if (gpu != null)
                        {
                            gpu.GPU_Count_on_storage -= 1;
                            App.Database.GPUS.AddOrUpdate(gpu);
                        }
                    }

                    // Добавьте аналогичные проверки для других типов компонентов (Cases, Coolers, HDDs, PowerSupplies, RAMS)

                    if (orderCartItem.CaseId.HasValue)
                    {
                        var caseComponent = App.Database.Cases.Where(c => c.CaseId == orderCartItem.CaseId.Value).FirstOrDefault();
                        if (caseComponent != null)
                        {
                            caseComponent.Cases_Count_on_storage -= 1;
                            App.Database.Cases.AddOrUpdate(caseComponent);
                        }
                    }

                    if (orderCartItem.FanId.HasValue)
                    {
                        var fan = App.Database.Coolers.Where(f => f.CoolerId == orderCartItem.FanId.Value).FirstOrDefault();
                        if (fan != null)
                        {
                            fan.Cooler_Count_on_storage -= 1;
                            App.Database.Coolers.AddOrUpdate(fan);
                        }
                    }

                    if (orderCartItem.RAMId.HasValue)
                    {
                        var ram = App.Database.RAMS.Where(r => r.RAMId == orderCartItem.RAMId.Value).FirstOrDefault();
                        if (ram != null)
                        {
                            ram.RAM_Count_on_storage -= 1;
                            App.Database.RAMS.AddOrUpdate(ram);
                        }
                    }

                    if (orderCartItem.MemoryId.HasValue)
                    {
                        var memory = App.Database.HDDs.Where(h => h.HDDId == orderCartItem.MemoryId.Value).FirstOrDefault();
                        if (memory != null)
                        {
                            memory.HDD_Count_on_storage -= 1;
                            App.Database.HDDs.AddOrUpdate(memory);
                        }
                    }

                    if (orderCartItem.PowerSuppliesId.HasValue)
                    {
                        var powerSupply = App.Database.PowerSupplies.Where(p => p.PowerSupplyId == orderCartItem.PowerSuppliesId.Value).FirstOrDefault();
                        if (powerSupply != null)
                        {
                            powerSupply.PS__Count_on_storage -= 1;
                            App.Database.PowerSupplies.AddOrUpdate(powerSupply);
                        }
                    }
                }
                App.Database.SaveChanges();

                // Сброс состояния пользователя
                CurrentUser.IsBusy = null;
                CurrentUser.DeliveryOrderInProcess = null;
                App.Database.Users.AddOrUpdate(CurrentUser);
                App.Database.SaveChanges();

                // Перезагрузка данных
                LoadData();

                // Закрытие диалога
                EndOrderDialog.Visibility = Visibility.Collapsed;
                DialogBack.Visibility = Visibility.Collapsed;
            }
        }

        private void EndOrderDialogNo_Click(object sender, RoutedEventArgs e)
        {
            EndOrderDialog.Visibility = Visibility.Collapsed;
            DialogBack.Visibility = Visibility.Collapsed;
        }
    }
}
