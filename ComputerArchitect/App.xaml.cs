using ComputerArchitect.Database;
using System.Windows;

namespace ComputerArchitect
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ComputerArchitectDataBaseEntities Database = new ComputerArchitectDataBaseEntities();
    }
}
