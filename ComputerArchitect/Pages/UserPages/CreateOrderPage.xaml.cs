using ComputerArchitect.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        int totalCost = 0;
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

                foreach (var cartItem in cartItems)
                {
                    CombinedData itemData = new CombinedData();
                    itemData.CartItems = cartItem;

                    if (cartItem.CPUS != null)
                    {
                        itemData.Processor = cartItem.CPUS;
                        totalCost += Convert.ToInt32(cartItem.CPUS.Cost);
                    }


                    if (cartItem.Motherboards != null)
                    {
                        itemData.Motherboards = cartItem.Motherboards;
                        totalCost += Convert.ToInt32(cartItem.Motherboards.Cost);
                    }
                    if (cartItem.Cases != null)
                    {
                        itemData.Cases = cartItem.Cases;
                        totalCost += Convert.ToInt32(cartItem.Cases.Cost);
                    }
                    if (cartItem.GPUS != null)
                    {
                        itemData.GPUS = cartItem.GPUS;
                        totalCost += Convert.ToInt32(cartItem.GPUS.Cost);
                    }
                    if (cartItem.Coolers != null)
                    {
                        itemData.Coolers = cartItem.Coolers;
                        totalCost += Convert.ToInt32(cartItem.Coolers.Cost);
                    }
                    if (cartItem.RAMS != null)
                    {
                        itemData.RAMS = cartItem.RAMS;
                        totalCost += Convert.ToInt32(cartItem.RAMS.Cost);
                    }
                    if (cartItem.HDDs != null)
                    {
                        itemData.HDDs = cartItem.HDDs;
                        totalCost += Convert.ToInt32(cartItem.HDDs.Cost);
                    }
                    if (cartItem.PowerSupplies != null)
                    {
                        itemData.PowerSupplies = cartItem.PowerSupplies;
                        totalCost += Convert.ToInt32(cartItem.PowerSupplies.Cost);
                    }

                    

                    combinedData.Add(itemData);
                }

                
                CartItemsCount.Content = $"товары {combinedData.Count} шт";
                CartTotalCost.Content = totalCost + "  ₽";
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
            InfoUserAdressTextBox.Text = string.Empty;
            SelfTakeBorder.Visibility = Visibility.Collapsed;
            DeliveryBorder.Visibility = Visibility.Visible;
        }


        /*private void GenerationQRCode(string text)
        {
            
            var qrCode = QRCodeWriter.CreateQrCode(text, 250, QRCodeWriter.QrErrorCorrectionLevel.Medium);

            
            var bitmap = qrCode.ToBitmap();

            
            using (var stream = new MemoryStream())
            {
                
                bitmap.ExportStream(stream, ImageFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);

                
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

               
                qrCodeImage.Source = bitmapImage;
            }
        }*/

        private void CreateNewOrder()
        {
            Error.Visibility = Visibility.Collapsed;
            int userId = CurrentUser.Id;

            using (var context = new ComputerArchitectDataBaseEntities())
            {
                var userCart = context.UsersCarts.Include("CartItems")
                                                 .FirstOrDefault(u => u.UserId == userId);

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
                            PowerSuppliesId = cartItem.PowerSuppliesId
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
                    //GenerationQRCode("https://github.com/brokzn");
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
