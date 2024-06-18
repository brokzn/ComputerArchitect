using ComputerArchitect.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
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
    /// Логика взаимодействия для UserCartPage.xaml
    /// </summary>
    public partial class UserCartPage : Page
    {
        public event EventHandler CartUpdated;
        int totalCost = 0;
        int outofstockCount = 0;
        public Users CurrentUser { get; set; }
        public UserCartPage(Users currentUser)
        {
            CurrentUser = currentUser;
            InitializeComponent();
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
                }

                

                foreach (var cartItem in cartItems)
                {
                    CombinedData itemData = new CombinedData();
                    itemData.CartItems = cartItem;

                    if (cartItem.CPUS != null)
                    {
                        itemData.Processor = cartItem.CPUS;
                        totalCost += Convert.ToInt32(cartItem.CPUS.Cost);

                        if (cartItem.CPUS.CPU_Count_on_storage == 0)
                        {
                            outofstockCount++;
                        }
                    }

                    if (cartItem.Motherboards != null)
                    {
                        itemData.Motherboards = cartItem.Motherboards;
                        totalCost += Convert.ToInt32(cartItem.Motherboards.Cost);

                        if (cartItem.Motherboards.Motherboard_Count_on_storage == 0)
                        {
                            outofstockCount++;
                        }
                    }

                    if (cartItem.Cases != null)
                    {
                        itemData.Cases = cartItem.Cases;
                        totalCost += Convert.ToInt32(cartItem.Cases.Cost);

                        if (cartItem.Cases.Cases_Count_on_storage == 0)
                        {
                            outofstockCount++;
                        }
                    }

                    if (cartItem.GPUS != null)
                    {
                        itemData.GPUS = cartItem.GPUS;
                        totalCost += Convert.ToInt32(cartItem.GPUS.Cost);

                        if (cartItem.GPUS.GPU_Count_on_storage == 0)
                        {
                            outofstockCount++;
                        }
                    }

                    if (cartItem.Coolers != null)
                    {
                        itemData.Coolers = cartItem.Coolers;
                        totalCost += Convert.ToInt32(cartItem.Coolers.Cost);

                        if (cartItem.Coolers.Cooler_Count_on_storage == 0)
                        {
                            outofstockCount++;
                        }
                    }

                    if (cartItem.RAMS != null)
                    {
                        itemData.RAMS = cartItem.RAMS;
                        totalCost += Convert.ToInt32(cartItem.RAMS.Cost);

                        if (cartItem.RAMS.RAM_Count_on_storage == 0)
                        {
                            outofstockCount++;
                        }
                    }

                    if (cartItem.HDDs != null)
                    {
                        itemData.HDDs = cartItem.HDDs;
                        totalCost += Convert.ToInt32(cartItem.HDDs.Cost);

                        if (cartItem.HDDs.HDD_Count_on_storage == 0)
                        {
                            outofstockCount++;
                        }
                    }

                    if (cartItem.PowerSupplies != null)
                    {
                        itemData.PowerSupplies = cartItem.PowerSupplies;
                        totalCost += Convert.ToInt32(cartItem.PowerSupplies.Cost);

                        if (cartItem.PowerSupplies.PS__Count_on_storage == 0)
                        {
                            outofstockCount++;
                        }
                    }

                    combinedData.Add(itemData);
                }

                // Check if any item was out of stock
                if (outofstockCount > 0)
                {
                    CreateOrderButton.IsEnabled = false;
                    outofstockCount=0; 
                }
                else
                {
                    CreateOrderButton.IsEnabled = true;
                }

                






                UserCartListBox.ItemsSource = combinedData;
                CartItemsCount.Content = $"товары {UserCartListBox.Items.Count} шт";
                CartTotalCost.Content = totalCost + "  ₽";


                if (combinedData.Count == 0)
                {
                    EmptyCartLabelLink.Visibility = Visibility.Visible;
                    EmptyCartLabelRedirect.Visibility = Visibility.Visible;
                    EmptyCartLabel.Visibility = Visibility.Visible;
                    UserCartListBox.Visibility = Visibility.Collapsed;
                    CardInfo.Visibility = Visibility.Collapsed;
                }
                else
                {
                    EmptyCartLabelLink.Visibility = Visibility.Collapsed;
                    EmptyCartLabelRedirect.Visibility = Visibility.Collapsed;
                    EmptyCartLabel.Visibility = Visibility.Collapsed;
                    UserCartListBox.Visibility = Visibility.Visible;
                    CardInfo.Visibility = Visibility.Visible;
                }

               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке корзины пользователя: " + ex.Message);
            }

        }
        private CombinedData lastSelectedItem;
        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            lastSelectedItem = button?.DataContext as CombinedData;
            if (lastSelectedItem != null)
            {
                DeleteDialog.Visibility = Visibility.Visible;
                DeleteDialogBack.Visibility = Visibility.Visible;
            }
        }


        private int GetCost(CombinedData item)
        {
            int cost = 0;
            if (item.Processor != null)
                cost += Convert.ToInt32(item.Processor.Cost);
            if (item.Motherboards != null)
                cost += Convert.ToInt32(item.Motherboards.Cost);
            if (item.Cases != null)
                cost += Convert.ToInt32(item.Cases.Cost);
            if (item.GPUS != null)
                cost += Convert.ToInt32(item.GPUS.Cost);
            if (item.Coolers != null)
                cost += Convert.ToInt32(item.Coolers.Cost);
            if (item.RAMS != null)
                cost += Convert.ToInt32(item.RAMS.Cost);
            if (item.HDDs != null)
                cost += Convert.ToInt32(item.HDDs.Cost);
            if (item.PowerSupplies != null)
                cost += Convert.ToInt32(item.PowerSupplies.Cost);
            return cost;
        }

        private void DialogYes_Click(object sender, RoutedEventArgs e)
        {
            if (lastSelectedItem != null)
            {
                try
                {
                    if (lastSelectedItem.CartItems != null)
                    {
                        int itemId = lastSelectedItem.CartItems.ItemId;
                        CartItems itemToRemove = App.Database.CartItems.FirstOrDefault(item => item.ItemId == itemId);

                        if (itemToRemove != null)
                        {
                            App.Database.CartItems.Remove(itemToRemove);
                            App.Database.SaveChanges();
                        }

                        combinedData.Remove(lastSelectedItem);
                        UserCartListBox.ItemsSource = null;
                        UserCartListBox.ItemsSource = combinedData;

                        

                        // Пересчитываем общую стоимость
                        totalCost = combinedData.Sum(item => GetCost(item));

                        // Обновляем информацию о количестве товаров
                        CartItemsCount.Content = $"товары {combinedData.Count} шт";

                        // Обновляем общую стоимость
                        CartTotalCost.Content = totalCost + " ₽";

                        // Обновляем видимость соответствующих элементов интерфейса
                        if (combinedData.Count == 0)
                        {
                            EmptyCartLabelLink.Visibility = Visibility.Visible;
                            EmptyCartLabelRedirect.Visibility = Visibility.Visible;
                            EmptyCartLabel.Visibility = Visibility.Visible;
                            UserCartListBox.Visibility = Visibility.Collapsed;
                            CardInfo.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            EmptyCartLabelLink.Visibility = Visibility.Collapsed;
                            EmptyCartLabelRedirect.Visibility = Visibility.Collapsed;
                            EmptyCartLabel.Visibility = Visibility.Collapsed;
                            UserCartListBox.Visibility = Visibility.Visible;
                            CardInfo.Visibility = Visibility.Visible;
                        }

                        DeleteDialog.Visibility = Visibility.Collapsed;
                        DeleteDialogBack.Visibility = Visibility.Collapsed;
                        lastSelectedItem = null;
                        
                        CartUpdated?.Invoke(this, EventArgs.Empty);

                    }
                    else
                    {
                        MessageBox.Show("Объект lastSelectedItem.CartItems содержит null.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении элемента из базы данных: " + ex.Message);
                }
            }
        }

        private void DialogNo_Click(object sender, RoutedEventArgs e)
        {
            DeleteDialog.Visibility = Visibility.Collapsed;
            DeleteDialogBack.Visibility = Visibility.Collapsed;
        }

        private void EmptyCartLabelLink_MouseLeave(object sender, MouseEventArgs e)
        {
            EmptyCartLabelLink.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6DB2E3"));
        }

        private void EmptyCartLabelLink_MouseEnter(object sender, MouseEventArgs e)
        {
            EmptyCartLabelLink.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6D6D6"));
        }

        private void EmptyCartLabelLink_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new CatalogPage(CurrentUser));
        }

        private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CreateOrderPage(CurrentUser));
        }
    }
}
