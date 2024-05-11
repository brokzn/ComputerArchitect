using ComputerArchitect.Database;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для CreateOrderPage.xaml
    /// </summary>
    public partial class CreateOrderPage : Page
    {
        public Users CurrentUser { get; set; }
        public CreateOrderPage(Users currentUser)
        {
            CurrentUser = currentUser;
            InitializeComponent();
            RadioButtonSelfTake.IsChecked = true;
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
            int totalCost = 0;

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

        private void RadioButtonMoney_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButtonOnline_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
