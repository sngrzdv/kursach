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
    public partial class EditEmailWindow : Window
    {
        private vacancyEntities _db = new vacancyEntities();
        private Users _user;

        public EditEmailWindow(Users user)
        {
            InitializeComponent();
            _user = user;
            CurrentEmailText.Text = _user.Email;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(NewEmailTextBox.Text);
                if (addr.Address != NewEmailTextBox.Text)
                {
                    MessageBox.Show("Введите корректный email");
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Введите корректный email");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(NewEmailTextBox.Text))
            {
                MessageBox.Show("Введите новый email");
                return;
            }

            if (_user.Password != PasswordBox.Password)
            {
                MessageBox.Show("Неверный пароль");
                return;
            }

            if (_db.Users.Any(u => u.Email == NewEmailTextBox.Text && u.Id != _user.Id))
            {
                MessageBox.Show("Этот email уже используется");
                return;
            }

            try
            {
                _user.Email = NewEmailTextBox.Text;
                _db.SaveChanges();
                CurrentUser.Email = _user.Email;

                MessageBox.Show("Email успешно изменен");
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}