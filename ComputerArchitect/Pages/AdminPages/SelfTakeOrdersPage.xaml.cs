using ComputerArchitect.Database;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
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



        private void CreateAndSavePdf(Orders orderinfo)
        {
            PdfDocument document = new PdfDocument();
            document.Info.Title = $"Order {orderinfo.OrderId}";

            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont fontRegular = new XFont("Arial", 10);
            XFont fontBold = new XFont("Arial", 10);

            DrawOrderInformation(gfx, fontBold, orderinfo);
            DrawOrderItemsTable(gfx, fontRegular, orderinfo);

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "PDF файлы (*.pdf)|*.pdf|Все файлы (*.*)|*.*";
            saveFileDialog.FileName = $"Order_{orderinfo.OrderId}.pdf";
            saveFileDialog.Title = "Сохранить как PDF";
            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                string filename = saveFileDialog.FileName;
                document.Save(filename);

                MessageBox.Show($"Файл сохранен по следующему пути:\n{filename}", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DrawOrderInformation(XGraphics gfx, XFont font, Orders orderinfo)
        {
            XRect rect = new XRect(40, 70, 500, 100);
            gfx.DrawString($"Информация о заказе №{orderinfo.OrderId}", font, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(40, 100, 500, 100);
            gfx.DrawString($"Дата создания заказа: {orderinfo.OrderCreateDate:yyyy.MM.dd}", font, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(40, 130, 500, 100);
            gfx.DrawString($"Общая стоимость: {orderinfo.TotalCost:N0}₽", font, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(40, 160, 500, 100);
            gfx.DrawString($"Оплата: {orderinfo.PaymentMethodType.PaymentMethodName}", font, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(40, 190, 500, 100);
            gfx.DrawString($"Заказчик: {orderinfo.Users.Name} {orderinfo.Users.SecondName}", font, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(40, 220, 500, 100);
            gfx.DrawString($"Номер телефона: {orderinfo.Users.PhoneNumber}", font, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(40, 250, 500, 100);
            gfx.DrawString($"Почта: {orderinfo.Users.Email}", font, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(40, 280, 500, 100);
            gfx.DrawString($"Подпись заказчика ________________", font, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(40, 310, 500, 100);
            gfx.DrawString($"Подпись администратора ________________", font, XBrushes.Black, rect, XStringFormats.TopLeft);

        }

        private void DrawOrderItemsTable(XGraphics gfx, XFont font, Orders orderinfo)
        {

            const int startX = 40;
            const int startY = 350;
            const int columnWidth = 220;


            gfx.DrawString("Наименование", font, XBrushes.Black, new XRect(startX, startY, columnWidth, 20), XStringFormats.TopLeft);
            gfx.DrawString("Цена в рублях", font, XBrushes.Black, new XRect(startX + columnWidth, startY, columnWidth, 20), XStringFormats.TopLeft);
            gfx.DrawString("Кол-во", font, XBrushes.Black, new XRect(startX + 2 * columnWidth, startY, columnWidth, 20), XStringFormats.TopLeft);


            int currentY = startY + 20;


            foreach (var item in orderinfo.OrderCartItems)
            {
                if (item.CPUS != null)
                {
                    gfx.DrawString("Процессор: " + item.CPUS.Model, font, XBrushes.Black, new XRect(startX, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    gfx.DrawString(item.CPUS.Cost.ToString(), font, XBrushes.Black, new XRect(startX + columnWidth, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    gfx.DrawString(item.CpuCount.ToString(), font, XBrushes.Black, new XRect(startX + 2 * columnWidth, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    currentY += 20;
                }
            }


            foreach (var item in orderinfo.OrderCartItems)
            {
                if (item.Motherboards != null)
                {
                    gfx.DrawString("Мат. плата: " + item.Motherboards.Motherboard_Model, font, XBrushes.Black, new XRect(startX, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    gfx.DrawString(item.Motherboards.Cost.ToString(), font, XBrushes.Black, new XRect(startX + columnWidth, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    gfx.DrawString(item.MotherboardCount.ToString(), font, XBrushes.Black, new XRect(startX + 2 * columnWidth, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    currentY += 20;
                }
            }


            foreach (var item in orderinfo.OrderCartItems)
            {
                if (item.Cases != null)
                {
                    gfx.DrawString("Корпус: " + item.Cases.Model, font, XBrushes.Black, new XRect(startX, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    gfx.DrawString(item.Cases.Cost.ToString(), font, XBrushes.Black, new XRect(startX + columnWidth, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    gfx.DrawString(item.CaseCount.ToString(), font, XBrushes.Black, new XRect(startX + 2 * columnWidth, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    currentY += 20;
                }
            }


            foreach (var item in orderinfo.OrderCartItems)
            {
                if (item.GPUS != null)
                {
                    gfx.DrawString("Видеокарта: " + item.GPUS.GPU_Model, font, XBrushes.Black, new XRect(startX, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    gfx.DrawString(item.GPUS.Cost.ToString(), font, XBrushes.Black, new XRect(startX + columnWidth, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    gfx.DrawString(item.GPUCount.ToString(), font, XBrushes.Black, new XRect(startX + 2 * columnWidth, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    currentY += 20;
                }
            }


            foreach (var item in orderinfo.OrderCartItems)
            {
                if (item.Coolers != null)
                {
                    gfx.DrawString("Кулер: " + item.Coolers.Model, font, XBrushes.Black, new XRect(startX, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    gfx.DrawString(item.Coolers.Cost.ToString(), font, XBrushes.Black, new XRect(startX + columnWidth, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    gfx.DrawString(item.FanCount.ToString(), font, XBrushes.Black, new XRect(startX + 2 * columnWidth, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    currentY += 20;
                }
            }


            foreach (var item in orderinfo.OrderCartItems)
            {
                if (item.RAMS != null)
                {
                    gfx.DrawString("ОЗУ: " + item.RAMS.RAM_Model, font, XBrushes.Black, new XRect(startX, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    gfx.DrawString(item.RAMS.Cost.ToString(), font, XBrushes.Black, new XRect(startX + columnWidth, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    gfx.DrawString(item.RAMCount.ToString(), font, XBrushes.Black, new XRect(startX + 2 * columnWidth, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    currentY += 20;
                }
            }


            foreach (var item in orderinfo.OrderCartItems)
            {
                if (item.HDDs != null)
                {
                    gfx.DrawString("ЖД: " + item.HDDs.Model, font, XBrushes.Black, new XRect(startX, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    gfx.DrawString(item.HDDs.Cost.ToString(), font, XBrushes.Black, new XRect(startX + columnWidth, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    gfx.DrawString(item.MemoryCount.ToString(), font, XBrushes.Black, new XRect(startX + 2 * columnWidth, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    currentY += 20;
                }
            }
            foreach (var item in orderinfo.OrderCartItems)
            {
                if (item.PowerSupplies != null)
                {
                    gfx.DrawString("Блок питания: " + item.PowerSupplies.Model, font, XBrushes.Black, new XRect(startX, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    gfx.DrawString(item.PowerSupplies.Cost.ToString(), font, XBrushes.Black, new XRect(startX + columnWidth, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    gfx.DrawString(item.PowerSuppliesCount.ToString(), font, XBrushes.Black, new XRect(startX + 2 * columnWidth, currentY, columnWidth, 20), XStringFormats.TopLeft);
                    currentY += 20;
                }
            }
        }

        private void CreatePDFButton_Click(object sender, RoutedEventArgs e)
        {
            var orderinfo = App.Database.Orders
                                .Where(o => o.OrderId == lastSelectedOrder.OrderId)
                                .FirstOrDefault();
            if (orderinfo != null)
            {
                CreateAndSavePdf(orderinfo);
            }
            else
            {
                MessageBox.Show("Не удалось загрузить информацию о заказе.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
