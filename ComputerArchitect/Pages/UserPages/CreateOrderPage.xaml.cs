using ComputerArchitect.Database;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace ComputerArchitect.Pages
{
    /// <summary>
    /// Логика взаимодействия для CreateOrderPage.xaml
    /// </summary>
    public partial class CreateOrderPage : Page
    {
        decimal totalCost = 0;
        public event EventHandler CartUpdated;
        public Users CurrentUser { get; set; }
        public CreateOrderPage(Users currentUser)
        {
            CurrentUser = currentUser;
            InitializeComponent();
            RadioButtonSelfTake.IsChecked = true;
            RadioButtonMoney.IsChecked = true;
            InfoUserEmailTextBox.Text = currentUser.Email;
            InfoUserPhoneTextBox.Text = currentUser.PhoneNumber;
            LoadUserCart();
        }

        public class CombinedData
        {
            public CPUS Processor { get; set; }
            public Motherboards Motherboards { get; set; }
            public Cases Cases { get; set; }
            public GPUS GPUS { get; set; }
            public Coolers Coolers { get; set; }
            public RAMS RAMS { get; set; }
            public HDDs HDDs { get; set; }
            public PowerSupplies PowerSupplies { get; set; }
            public int CartItemId { get; set; }
            public CartItems CartItems { get; set; }
        }

        List<CombinedData> combinedData = new List<CombinedData>();
        private void LoadUserCart()
        {
            List<CartItems> cartItems = new List<CartItems>();


            try
            {
                int userId = CurrentUser.Id;

                int? userCartId = App.Database.UsersCarts
                    .Where(cart => cart.UserId == userId)
                    .Select(cart => cart.CartId)
                    .FirstOrDefault();

                if (userCartId != null)
                {
                    cartItems = App.Database.CartItems
                        .Where(item => item.CartId == userCartId)
                        .ToList();
                }
                else
                {
                    MessageBox.Show("Корзина для текущего пользователя не найдена.");
                    return;
                }

                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    int totalItemCount = 0;
                    

                    foreach (var item in cartItems)
                    {
                        totalItemCount += (item.CpuCount ?? 0) + (item.MotherboardCount ?? 0) + (item.CaseCount ?? 0) +
                                          (item.GPUCount ?? 0) + (item.FanCount ?? 0) + (item.RAMCount ?? 0) +
                                          (item.MemoryCount ?? 0) + (item.PowerSuppliesCount ?? 0);

                        totalCost += ((item.CpuCount ?? 0) * (item.CPUS?.Cost ?? 0)) +
                                     ((item.MotherboardCount ?? 0) * (item.Motherboards?.Cost ?? 0)) +
                                     ((item.CaseCount ?? 0) * (item.Cases?.Cost ?? 0)) +
                                     ((item.GPUCount ?? 0) * (item.GPUS?.Cost ?? 0)) +
                                     ((item.FanCount ?? 0) * (item.Coolers?.Cost ?? 0)) +
                                     ((item.RAMCount ?? 0) * (item.RAMS?.Cost ?? 0)) +
                                     ((item.MemoryCount ?? 0) * (item.HDDs?.Cost ?? 0)) +
                                     ((item.PowerSuppliesCount ?? 0) * (item.PowerSupplies?.Cost ?? 0));
                    }
                    CartItemsCount.Content = "товары " + totalItemCount.ToString() + " шт.";
                    CartTotalCost.Content = $"{totalCost:N0} ₽";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке корзины пользователя: " + ex.Message);
            }
        }



        private void RadioButtonSelfTake_Checked(object sender, RoutedEventArgs e)
        {
            InfoUserAdressTextBox.Text = string.Empty;
            SelfTakeBorder.Visibility = Visibility.Visible;
            DeliveryBorder.Visibility = Visibility.Collapsed;
        }

        private void RadioButtonDelivery_Checked(object sender, RoutedEventArgs e)
        {
            InfoUserAdressTextBox.Text = CurrentUser.About;
            SelfTakeBorder.Visibility = Visibility.Collapsed;
            DeliveryBorder.Visibility = Visibility.Visible;
        }


        

        private void CreateNewOrder()
        {
            Error.Visibility = Visibility.Collapsed;
            int userId = CurrentUser.Id;

            using (var context = new ComputerArchitectDataBaseEntities())
            {
                var userCart = context.UsersCarts.Include("CartItems")
                                                 .FirstOrDefault(u => u.UserId == userId);

                List<CartItems> cartItems = new List<CartItems>();
                int totalItemCount = 0;


                foreach (var item in cartItems)
                {
                    totalItemCount += (item.CpuCount ?? 0) + (item.MotherboardCount ?? 0) + (item.CaseCount ?? 0) +
                                      (item.GPUCount ?? 0) + (item.FanCount ?? 0) + (item.RAMCount ?? 0) +
                                      (item.MemoryCount ?? 0) + (item.PowerSuppliesCount ?? 0);

                    totalCost += ((item.CpuCount ?? 0) * (item.CPUS?.Cost ?? 0)) +
                                 ((item.MotherboardCount ?? 0) * (item.Motherboards?.Cost ?? 0)) +
                                 ((item.CaseCount ?? 0) * (item.Cases?.Cost ?? 0)) +
                                 ((item.GPUCount ?? 0) * (item.GPUS?.Cost ?? 0)) +
                                 ((item.FanCount ?? 0) * (item.Coolers?.Cost ?? 0)) +
                                 ((item.RAMCount ?? 0) * (item.RAMS?.Cost ?? 0)) +
                                 ((item.MemoryCount ?? 0) * (item.HDDs?.Cost ?? 0)) +
                                 ((item.PowerSuppliesCount ?? 0) * (item.PowerSupplies?.Cost ?? 0));
                }

                if (userCart != null)
                {
                    int deliverymethod;

                    if (RadioButtonDelivery.IsChecked == true)
                    {
                        deliverymethod = 2;
                    }
                    else
                    {
                        deliverymethod = 1;
                    }

                    int paymentmethod;

                    if (RadioButtonOnline.IsChecked == true)
                    {
                        paymentmethod = 2;
                    }
                    else
                    {
                        paymentmethod = 1;
                    }

                    var newOrder = new Orders
                    {
                        UserId = userId,
                        PhoneNumber = userCart.Users.PhoneNumber,
                        Email = userCart.Users.Email,
                        TotalCost = totalCost,
                        DeliveryMethod = deliverymethod,
                        PaymentMethod = paymentmethod,
                        DeliveryAddress = InfoUserAdressTextBox.Text,
                        OrderCreateDate = DateTime.Now,
                        OrderStatusId = 1,
                    };

                    context.Orders.Add(newOrder);
                    context.SaveChanges();

                    
                    foreach (var cartItem in userCart.CartItems)
                    {
                        var orderCartItem = new OrderCartItems
                        {
                            OrderId = newOrder.OrderId,
                            CpuId = cartItem.CpuId,
                            MotherboardId = cartItem.MotherboardId,
                            CaseId = cartItem.CaseId,
                            GPUId = cartItem.GPUId,
                            FanId = cartItem.FanId,
                            RAMId = cartItem.RAMId,
                            MemoryId = cartItem.MemoryId,
                            PowerSuppliesId = cartItem.PowerSuppliesId,
                            CpuCount = cartItem.CpuCount,
                            MotherboardCount = cartItem.MotherboardCount,
                            CaseCount = cartItem.CaseCount,
                            GPUCount = cartItem.GPUCount,
                            FanCount = cartItem.FanCount,
                            RAMCount = cartItem.RAMCount,
                            MemoryCount = cartItem.MemoryCount,
                            PowerSuppliesCount = cartItem.PowerSuppliesCount,
                        };

                        context.OrderCartItems.Add(orderCartItem);
                    }

                   
                    context.SaveChanges();

                    // Создаем копию списка элементов для удаления
                    var itemsToRemove = userCart.CartItems.ToList();

                    // Перечисляем копию списка и удаляем элементы из основной коллекции
                    foreach (var cartItem in itemsToRemove)
                    {
                        context.CartItems.Remove(cartItem);
                    }

                    
                    context.SaveChanges();
                    CartUpdated?.Invoke(this, EventArgs.Empty);
                    DialogBack.Visibility = Visibility.Visible;
                    OrderCreateMessageDialog.Visibility = Visibility.Visible;
                }
            }
        }
        private void GenerateAndDisplayQRCode(string content)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            System.Drawing.Bitmap qrCodeImage = qrCode.GetGraphic(20);

            // Convert System.Drawing.Bitmap to BitmapImage
            var bitmap = new System.Drawing.Bitmap(qrCodeImage);
            var bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            bitmapImage.BeginInit();
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();


            QRCodeImage.Source = bitmapImage; 
        }

        private void AddNewOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonDelivery.IsChecked == true && string.IsNullOrEmpty(InfoUserAdressTextBox.Text))
            {
                Error.Visibility = Visibility.Visible;
            }
            else
            {
                if (RadioButtonOnline.IsChecked == true)
                {
                    string exampleUrl = "https://github.com/brokzn";
                    GenerateAndDisplayQRCode(exampleUrl);
                    OnlinePayMethodDialog.Visibility = Visibility.Visible;
                    DialogBack.Visibility = Visibility.Visible;
                }
                else
                {
                    CreateNewOrder();
                }
            }
        }

        private void InfoUserAdressTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Error.Visibility = Visibility.Collapsed;
        }

        private void OnlinePayMethodDialogYes_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonDelivery.IsChecked == true && string.IsNullOrEmpty(InfoUserAdressTextBox.Text))
            {
                Error.Visibility = Visibility.Visible;
                OnlinePayMethodDialog.Visibility = Visibility.Collapsed;
                DialogBack.Visibility = Visibility.Collapsed;
            }
            else
            {
                CreateNewOrder();
                OnlinePayMethodDialog.Visibility = Visibility.Collapsed;
            }
        }

        private void OnlinePayMethodDialogNo_Click(object sender, RoutedEventArgs e)
        {
            OnlinePayMethodDialog.Visibility = Visibility.Collapsed;
            DialogBack.Visibility = Visibility.Collapsed;
        }

        private void OrderCreateMessageDialogYes_Click(object sender, RoutedEventArgs e)
        {
            DialogBack.Visibility = Visibility.Collapsed;
            OrderCreateMessageDialog.Visibility = Visibility.Collapsed;
            CartUpdated?.Invoke(this, EventArgs.Empty);
            NavigationService.Navigate(new CatalogPage(CurrentUser));
        }
    }
}
