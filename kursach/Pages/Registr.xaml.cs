using kursach.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace kursach.Pages
{
    /// <summary>
    /// Логика взаимодействия для Registr.xaml
    /// </summary>
    public partial class Registr : Page
    {
        private readonly vacancyEntities db = new vacancyEntities();

        public Registr()
        {
            InitializeComponent();
            Loaded += (s, e) => tbFirstName.Focus();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация данных
                if (!ValidateFields())
                    return;

                // Проверка уникальности email
                if (db.Users.Any(u => u.Email == tbEmail.Text.Trim()))
                {
                    ShowError("Пользователь с таким email уже существует");
                    return;
                }

                // Создание нового пользователя
                var newUser = new Users
                {
                    FirstName = tbFirstName.Text.Trim(),
                    LastName = tbLastName.Text.Trim(),
                    FatherName = tbFatherName.Text.Trim(),
                    Email = tbEmail.Text.Trim(),
                    Phone = tbPhone.Text.Trim(),
                    Password = pbPassword.Password, // В реальном проекте хешируйте!
                    RoleId = int.Parse(((ComboBoxItem)cbRole.SelectedItem).Tag.ToString()),
                    RegistrationDate = DateTime.Now
                };

                // Сохранение в базу
                db.Users.Add(newUser);
                db.SaveChanges();

                // Автоматическая авторизация после регистрации
                CurrentUser.Id = newUser.Id;
                CurrentUser.RoleId = newUser.RoleId;
                CurrentUser.FullName = $"{newUser.LastName} {newUser.FirstName}";
                CurrentUser.Email = newUser.Email;

                MessageBox.Show("Регистрация прошла успешно!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                    NavigationService.Navigate(new Autoriz());
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка регистрации: {ex.Message}");
            }
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(tbFirstName.Text))
            {
                ShowError("Введите имя");
                return false;
            }

            if (string.IsNullOrWhiteSpace(tbLastName.Text))
            {
                ShowError("Введите фамилию");
                return false;
            }

            if (!IsValidEmail(tbEmail.Text))
            {
                ShowError("Введите корректный email");
                return false;
            }

            if (!IsValidPhone(tbPhone.Text))
            {
                ShowError("Телефон должен быть в формате +71234567890");
                return false;
            }

            if (pbPassword.Password.Length < 6)
            {
                ShowError("Пароль должен содержать минимум 6 символов");
                return false;
            }

            if (pbPassword.Password != pbConfirmPassword.Password)
            {
                ShowError("Пароли не совпадают");
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private bool IsValidPhone(string phone)
        {
            return Regex.IsMatch(phone, @"^\+7\d{10}$");
        }

        private void ShowError(string message)
        {
            tbError.Text = message;
            errorBorder.Visibility = Visibility.Visible;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Autoriz());
        }
    }
}