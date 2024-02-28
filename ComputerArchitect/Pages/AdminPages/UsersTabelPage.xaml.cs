using ComputerArchitect.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
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
    /// Логика взаимодействия для UsersTabelPage.xaml
    /// </summary>
    public partial class UsersTabelPage : Page
    {
        public UsersTabelPage()
        {
            InitializeComponent();
            LoadData();
            List<UserRoles> userRoles;

            using (var context = new ComputerArchitectDataBaseEntities())
            {
                userRoles = context.UserRoles.ToList();
            }

            UserRoleSelectComboBox.ItemsSource = userRoles;
            UserRoleSelectComboBox.DisplayMemberPath = "RoleName";
            UserRoleSelectComboBox.SelectedValuePath = "RoleId";
        }
        public class CombinedData
        {
            public Users users { get; set; }
            public UserRoles UserRoles { get; set; }
        }

        private void LoadData()
        {
            var combinedData = (from User in App.Database.Users
                                select new CombinedData
                                {
                                    users = User,
                                }).ToList();



            UsersListBox.ItemsSource = combinedData;
        }

        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            UserEditDialogBack.Visibility = Visibility.Visible;
            UserEditDialog.Visibility = Visibility.Visible;

            var button = sender as Button;
            selectedUser = button?.DataContext as CombinedData;

            if (selectedUser != null)
            {
                UserNameLabel.Text = selectedUser.users.Name;
                UserSecondNameLabel.Text = selectedUser.users.SecondName;
                UserPhoneLabel.Text = selectedUser.users.PhoneNumber;
                UserEmailLabel.Text = selectedUser.users.Email;
                UserRoleLabel.Text = selectedUser.users.UserRoles.RoleName;

                if (selectedUser.users.Photo != null)
                {
                    using (MemoryStream ms = new MemoryStream(selectedUser.users.Photo))
                    {
                        BitmapImage image = new BitmapImage();
                        image.BeginInit();
                        image.StreamSource = ms;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.EndInit();
                        UserPhotoImage.Source = image;
                    }
                }
                else
                {
                    UserPhotoImage.Source = new BitmapImage(new Uri("/UI/Elements/UserMissedPictureBig.png", UriKind.Relative));
                }

                if (selectedUser.users.UserIsBlocked is true)
                {
                    DialogUnBlock.Visibility = Visibility.Visible;
                    DialogBlock.Visibility = Visibility.Collapsed;
                }
                else
                {
                    DialogUnBlock.Visibility = Visibility.Collapsed;
                    DialogBlock.Visibility = Visibility.Visible;
                }

            }
        }

        private CombinedData selectedUser;
        private void DialogSave_Click(object sender, RoutedEventArgs e)
        {
            if (UserRoleSelectComboBox.SelectedItem is UserRoles selectedRole && selectedUser != null)
            {
                
                selectedUser.users.RoleId = selectedRole.RoleId;

                
                using (var context = new ComputerArchitectDataBaseEntities())
                {
                   
                    context.Users.AddOrUpdate(selectedUser.users);
                    context.SaveChanges();
                }
                UserRoleLabel.Text = selectedRole.RoleName;
                
                MessageBox.Show("Роль пользователя успешно обновлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Показываем уведомление о неудаче
                MessageBox.Show("Не удалось обновить роль пользователя. Пожалуйста, выберите роль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DialogBack_Click(object sender, RoutedEventArgs e)
        {
            UserEditDialogBack.Visibility = Visibility.Collapsed;
            UserEditDialog.Visibility = Visibility.Collapsed;
        }

        private void DialogUnBlock_Click(object sender, RoutedEventArgs e)
        {
            if (selectedUser != null)
            {
                selectedUser.users.UserIsBlocked = false; // Разблокировать пользователя

                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    context.Users.AddOrUpdate(selectedUser.users);
                    context.SaveChanges();
                }

                // Показать уведомление об успешном разблокировании
                MessageBox.Show("Пользователь успешно разблокирован.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                if (selectedUser.users.UserIsBlocked is true)
                {
                    DialogUnBlock.Visibility = Visibility.Visible;
                    DialogBlock.Visibility = Visibility.Collapsed;
                }
                else
                {
                    DialogUnBlock.Visibility = Visibility.Collapsed;
                    DialogBlock.Visibility = Visibility.Visible;
                }
            }
            else
            {
                // Показать уведомление о неудаче
                MessageBox.Show("Не удалось разблокировать пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DialogBlock_Click(object sender, RoutedEventArgs e)
        {
            if (selectedUser != null)
            {
                selectedUser.users.UserIsBlocked = true; // Заблокировать пользователя

                using (var context = new ComputerArchitectDataBaseEntities())
                {
                    context.Users.AddOrUpdate(selectedUser.users);
                    context.SaveChanges();
                }

                // Показать уведомление об успешной блокировке
                MessageBox.Show("Пользователь успешно заблокирован.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                if (selectedUser.users.UserIsBlocked is true)
                {
                    DialogUnBlock.Visibility = Visibility.Visible;
                    DialogBlock.Visibility = Visibility.Collapsed;
                }
                else
                {
                    DialogUnBlock.Visibility = Visibility.Collapsed;
                    DialogBlock.Visibility = Visibility.Visible;
                }
            }
            else
            {
                // Показать уведомление о неудаче
                MessageBox.Show("Не удалось заблокировать пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
