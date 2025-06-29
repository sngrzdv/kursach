using kursach.AppData;
using kursach.Pages;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
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
        private readonly vacancyEntities _db;
        private readonly Users _user;

        public EditEmailWindow(Users user)
        {
            InitializeComponent();
            _db = new vacancyEntities();

            _user = _db.Users.Find(user.Id);

            if (_user == null)
            {
                MessageBox.Show("Пользователь не найден");
                Close();
                return;
            }

            CurrentEmailText.Text = _user.Email;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValidEmail(NewEmailTextBox.Text))
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

                _db.Entry(_user).State = EntityState.Modified;
                _db.SaveChanges();

                CurrentUser.Email = _user.Email;

                MessageBox.Show("Email успешно изменен");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _db?.Dispose(); 
        }
    }
}