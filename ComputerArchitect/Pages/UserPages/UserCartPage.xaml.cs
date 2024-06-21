using ComputerArchitect.Database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;


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

                
                if (outofstockCount > 0)
                {
                    CreateOrderButton.IsEnabled = false;
                    outofstockCount = 0;
                }
                else
                {
                    CreateOrderButton.IsEnabled = true;
                }

                UserCartListBox.ItemsSource = combinedData;
                
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

                        if (outofstockCount > 0)
                        {
                            CreateOrderButton.IsEnabled = false;
                            outofstockCount = 0;
                        }
                        else
                        {
                            CreateOrderButton.IsEnabled = true;
                        }
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

        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var combinedData = button?.DataContext as CombinedData;

            if (combinedData != null && combinedData.CartItems != null)
            {
                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    var cartItem = context.CartItems.FirstOrDefault(ci => ci.ItemId == combinedData.CartItems.ItemId);

                    if (cartItem != null)
                    {
                        UpdateItemUI(button, combinedData, cartItem);
                        UpdateCartSummary(button, context);
                    }
                }
            }
        }

        private void MinusCount_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var combinedData = button?.DataContext as CombinedData;

            if (combinedData != null && combinedData.CartItems != null)
            {
                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    var cartItem = context.CartItems.FirstOrDefault(ci => ci.ItemId == combinedData.CartItems.ItemId);

                    if (cartItem != null)
                    {
                        var itemCount = GetItemCount(cartItem, combinedData);
                        if (itemCount > 1)
                        {
                            SetItemCount(cartItem, combinedData, itemCount - 1);
                            context.SaveChanges();
                            UpdateItemUI(button, combinedData, cartItem);
                            UpdateCartSummary(button, context);
                        }
                        else
                        {
                            
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Ошибка: не удалось определить выбранный элемент.");
            }
        }

        private void PlusCount_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var combinedData = button?.DataContext as CombinedData;

            if (combinedData != null && combinedData.CartItems != null)
            {
                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    var cartItem = context.CartItems.FirstOrDefault(ci => ci.ItemId == combinedData.CartItems.ItemId);

                    if (cartItem != null)
                    {
                        var itemCount = GetItemCount(cartItem, combinedData);
                        SetItemCount(cartItem, combinedData, itemCount + 1);
                        context.SaveChanges();
                        UpdateItemUI(button, combinedData, cartItem);
                        UpdateCartSummary(button, context);
                    }
                }
            }
            else
            {
                MessageBox.Show("Ошибка: не удалось определить выбранный элемент.");
            }
        }

        private int GetItemCount(CartItems cartItem, CombinedData combinedData)
        {
            if (combinedData.CartItems.CpuId != null) return cartItem.CpuCount ?? 0;
            if (combinedData.CartItems.MotherboardId != null) return cartItem.MotherboardCount ?? 0;
            if (combinedData.CartItems.CaseId != null) return cartItem.CaseCount ?? 0;
            if (combinedData.CartItems.GPUId != null) return cartItem.GPUCount ?? 0;
            if (combinedData.CartItems.FanId != null) return cartItem.FanCount ?? 0;
            if (combinedData.CartItems.RAMId != null) return cartItem.RAMCount ?? 0;
            if (combinedData.CartItems.MemoryId != null) return cartItem.MemoryCount ?? 0;
            if (combinedData.CartItems.PowerSuppliesId != null) return cartItem.PowerSuppliesCount ?? 0;
            return 0;
        }

        private void SetItemCount(CartItems cartItem, CombinedData combinedData, int count)
        {
            if (combinedData.CartItems.CpuId != null) cartItem.CpuCount = count;
            if (combinedData.CartItems.MotherboardId != null) cartItem.MotherboardCount = count;
            if (combinedData.CartItems.CaseId != null) cartItem.CaseCount = count;
            if (combinedData.CartItems.GPUId != null) cartItem.GPUCount = count;
            if (combinedData.CartItems.FanId != null) cartItem.FanCount = count;
            if (combinedData.CartItems.RAMId != null) cartItem.RAMCount = count;
            if (combinedData.CartItems.MemoryId != null) cartItem.MemoryCount = count;
            if (combinedData.CartItems.PowerSuppliesId != null) cartItem.PowerSuppliesCount = count;
        }

        private void UpdateItemUI(Button button, CombinedData combinedData, CartItems cartItem)
        {
            int itemCount = GetItemCount(cartItem, combinedData);
            var textBlockItemCount = FindVisualChild<TextBlock>(button.Parent, "ItemCountText");
            if (textBlockItemCount != null)
                textBlockItemCount.Text = itemCount.ToString();

            decimal? itemCost = GetItemCost(combinedData);
            var textBlockTotalCost = FindVisualChild<TextBlock>(button.Parent, GetTotalCostTextBlockName(combinedData));
            if (textBlockTotalCost != null && itemCost.HasValue)
                textBlockTotalCost.Text = $"{(itemCount * itemCost.Value):N0} ₽";
        }

        private decimal? GetItemCost(CombinedData combinedData)
        {
            if (combinedData.CartItems.CpuId != null) return combinedData.CartItems.CPUS?.Cost;
            if (combinedData.CartItems.MotherboardId != null) return combinedData.CartItems.Motherboards?.Cost;
            if (combinedData.CartItems.CaseId != null) return combinedData.CartItems.Cases?.Cost;
            if (combinedData.CartItems.GPUId != null) return combinedData.CartItems.GPUS?.Cost;
            if (combinedData.CartItems.FanId != null) return combinedData.CartItems.Coolers?.Cost;
            if (combinedData.CartItems.RAMId != null) return combinedData.CartItems.RAMS?.Cost;
            if (combinedData.CartItems.MemoryId != null) return combinedData.CartItems.HDDs?.Cost;
            if (combinedData.CartItems.PowerSuppliesId != null) return combinedData.CartItems.PowerSupplies?.Cost;
            return null;
        }

        private string GetTotalCostTextBlockName(CombinedData combinedData)
        {
            if (combinedData.CartItems.CpuId != null) return "CPUTotalCostText";
            if (combinedData.CartItems.MotherboardId != null) return "MotherboardTotalCostText";
            if (combinedData.CartItems.CaseId != null) return "CaseTotalCostText";
            if (combinedData.CartItems.GPUId != null) return "GPUTotalCostText";
            if (combinedData.CartItems.FanId != null) return "FanTotalCostText";
            if (combinedData.CartItems.RAMId != null) return "RAMTotalCostText";
            if (combinedData.CartItems.MemoryId != null) return "MemoryTotalCostText";
            if (combinedData.CartItems.PowerSuppliesId != null) return "PowerSuppliesTotalCostText";
            return "TotalCostText"; 
        }

        private void UpdateCartSummary(Button button, ComputerArchitectDataBaseEntities context)
        {
            var cartItems = context.CartItems.ToList();
            int totalItemCount = 0;
            decimal totalCost = 0;

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

        private T FindVisualChild<T>(DependencyObject parent, string name) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild && (child as FrameworkElement)?.Name == name)
                {
                    return typedChild;
                }
                else
                {
                    var childOfChild = FindVisualChild<T>(child, name);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        
    }
}
