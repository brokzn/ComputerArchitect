using ComputerArchitect.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ListBox = System.Windows.Controls.ListBox;

namespace ComputerArchitect.Pages.AdminPages
{
    /// <summary>
    /// Логика взаимодействия для EndingComponentsPage.xaml
    /// </summary>
    public partial class EndingComponentsPage : Page
    {
        public EndingComponentsPage()
        {
            InitializeComponent();
            LoadLabelInfo();
        }

        private void LoadLabelInfo()
        {
            EndingCPUButton.Content = "Процессоры (" + App.Database.CPUS.Where(c => c.CPU_Count_on_storage < 6).Count() + ")";
            EndingMotherboardButton.Content = "Мат. платы (" + App.Database.Motherboards.Where(c => c.Motherboard_Count_on_storage < 6).Count() + ")";
            EndingCaseButton.Content = "Корпуса (" + App.Database.Cases.Where(c => c.Cases_Count_on_storage < 6).Count() + ")";
            EndingGPUButton.Content = "Видеокарты (" + App.Database.GPUS.Where(c => c.GPU_Count_on_storage < 6).Count() + ")";
            EndingCoolerButton.Content = "Охл. процессора (" + App.Database.Coolers.Where(c => c.Cooler_Count_on_storage < 6).Count() + ")";
            EndingRAMButton.Content = "ОЗУ (" + App.Database.RAMS.Where(c => c.RAM_Count_on_storage < 6).Count() + ")";
            EndingMemoryButton.Content = "ЖД (" + App.Database.HDDs.Where(c => c.HDD_Count_on_storage < 6).Count() + ")";
            EndingPSButton.Content = "Блоки питания (" + App.Database.PowerSupplies.Where(c => c.PS__Count_on_storage < 6).Count() + ")";
        }

        private void Hide()
        {
            CPUListBox.Visibility = Visibility.Collapsed;
            MotherboardListBox.Visibility = Visibility.Collapsed;
            CaseListBox.Visibility = Visibility.Collapsed;
            GPUSListBox.Visibility = Visibility.Collapsed;
            CoolerListBox.Visibility = Visibility.Collapsed;
            RAMListBox.Visibility = Visibility.Collapsed;
            MemoryListBox.Visibility = Visibility.Collapsed;
            PSListBox.Visibility = Visibility.Collapsed;
        }


        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6D6D6"));
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6D6D6"));
        }

        private object selectedItem;
        private ListBox activeListBox;

        private void ShowListBox<T>(ListBox listBox, Func<T, bool> predicate) where T : class
        {
            Hide();
            listBox.Visibility = Visibility.Visible;
            listBox.ItemsSource = App.Database.Set<T>().Where(predicate).ToList();
            activeListBox = listBox;
        }


        private void EndingCPUButton_Click(object sender, RoutedEventArgs e)
        {
            ShowListBox(CPUListBox, (CPUS c) => c.CPU_Count_on_storage < 6);
        }

        private void EndingMotherboardButton_Click(object sender, RoutedEventArgs e)
        {
            ShowListBox(MotherboardListBox, (Motherboards m) => m.Motherboard_Count_on_storage < 6);
        }

        private void EndingCaseButton_Click(object sender, RoutedEventArgs e)
        {
            ShowListBox(CaseListBox, (Cases c) => c.Cases_Count_on_storage < 6);
        }

        private void EndingGPUButton_Click(object sender, RoutedEventArgs e)
        {
            ShowListBox(GPUSListBox, (GPUS g) => g.GPU_Count_on_storage < 6);
        }

        private void EndingCoolerButton_Click(object sender, RoutedEventArgs e)
        {
            ShowListBox(CoolerListBox, (Coolers c) => c.Cooler_Count_on_storage < 6);
        }

        private void EndingRAMButton_Click(object sender, RoutedEventArgs e)
        {
            ShowListBox(RAMListBox, (RAMS r) => r.RAM_Count_on_storage < 6);
        }

        private void EndingMemoryButton_Click(object sender, RoutedEventArgs e)
        {
            ShowListBox(MemoryListBox, (HDDs m) => m.HDD_Count_on_storage < 6);
        }

        private void EndingPSButton_Click(object sender, RoutedEventArgs e)
        {
            ShowListBox(PSListBox, (PowerSupplies ps) => ps.PS__Count_on_storage < 6);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListBox).SelectedItem != null)
            {
                selectedItem = (sender as ListBox).SelectedItem;
                AddDialog.Visibility = Visibility.Visible;
                DialogBack.Visibility = Visibility.Visible;
            }
        }

        private void AddCountButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedItem != null && int.TryParse(AddCountTB.Text, out int count))
            {
                if (selectedItem is CPUS cpu)
                {
                    cpu.CPU_Count_on_storage += count;
                }
                else if (selectedItem is Motherboards motherboard)
                {
                    motherboard.Motherboard_Count_on_storage += count;
                }
                else if (selectedItem is Cases pcCase)
                {
                    pcCase.Cases_Count_on_storage += count;
                }
                else if (selectedItem is GPUS gpu)
                {
                    gpu.GPU_Count_on_storage += count;
                }
                else if (selectedItem is Coolers cooler)
                {
                    cooler.Cooler_Count_on_storage += count;
                }
                else if (selectedItem is RAMS ram)
                {
                    ram.RAM_Count_on_storage += count;
                }
                else if (selectedItem is HDDs memory)
                {
                    memory.HDD_Count_on_storage += count;
                }
                else if (selectedItem is PowerSupplies powerSupply)
                {
                    powerSupply.PS__Count_on_storage += count;
                }

                App.Database.SaveChanges();
                RefreshListBox();
                LoadLabelInfo();
                AddDialog.Visibility = Visibility.Collapsed;
                DialogBack.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Введите корректное количество.");
            }
        }

        private void CloseDialogButton_Click(object sender, RoutedEventArgs e)
        {
            AddDialog.Visibility = Visibility.Collapsed;
            DialogBack.Visibility = Visibility.Collapsed;
        }

        private void RefreshListBox()
        {
            if (activeListBox != null)
            {
                if (activeListBox == MotherboardListBox)
                {
                    activeListBox.ItemsSource = App.Database.Motherboards.Where(m => m.Motherboard_Count_on_storage < 6).ToList();
                }
                else if (activeListBox == CaseListBox)
                {
                    activeListBox.ItemsSource = App.Database.Cases.Where(c => c.Cases_Count_on_storage < 6).ToList();
                }
                else if (activeListBox == GPUSListBox)
                {
                    activeListBox.ItemsSource = App.Database.GPUS.Where(g => g.GPU_Count_on_storage < 6).ToList();
                }
                else if (activeListBox == CoolerListBox)
                {
                    activeListBox.ItemsSource = App.Database.Coolers.Where(c => c.Cooler_Count_on_storage < 6).ToList();
                }
                else if (activeListBox == RAMListBox)
                {
                    activeListBox.ItemsSource = App.Database.RAMS.Where(r => r.RAM_Count_on_storage < 6).ToList();
                }
                else if (activeListBox == MemoryListBox)
                {
                    activeListBox.ItemsSource = App.Database.HDDs.Where(m => m.HDD_Count_on_storage < 6).ToList();
                }
                else if (activeListBox == PSListBox)
                {
                    activeListBox.ItemsSource = App.Database.PowerSupplies.Where(ps => ps.PS__Count_on_storage < 6).ToList();
                }
            }
        }
    }
}
