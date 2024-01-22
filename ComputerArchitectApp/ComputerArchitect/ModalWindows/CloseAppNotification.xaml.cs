using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;


namespace ComputerArchitect.ModalWindows
{
    /// <summary>
    /// Логика взаимодействия для CloseAppNotification.xaml
    /// </summary>
    public partial class CloseAppNotification : Window
    {
        public CloseAppNotification()
        {
            InitializeComponent();
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CloseNotification_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
