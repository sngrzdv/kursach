using kursach.AppData;
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
using System.Windows.Shapes;

namespace kursach.Windows
{
    public partial class EditPasswordWindow : Window
    {
        private vacancyEntities _db = new vacancyEntities();
        private Users _user;

        public EditPasswordWindow(Users user)
        {
            InitializeComponent();
            _user = user;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_user.Password != CurrentPasswordBox.Password)
            {
                MessageBox.Show("Неверный текущий пароль");
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPasswordBox.Password))
            {
                MessageBox.Show("Введите новый пароль");
                return;
            }

            if (NewPasswordBox.Password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов");
                return;
            }

            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают");
                return;
            }

            try
            {
                _user.Password = NewPasswordBox.Password;
                _db.SaveChanges();

                MessageBox.Show("Пароль успешно изменен");
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}