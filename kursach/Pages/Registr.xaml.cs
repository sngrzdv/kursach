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
        public Registr()
        {
            InitializeComponent();
            cbRole.SelectedIndex = 0; // Устанавливаем первый элемент по умолчанию
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ResetErrors();

                // Валидация данных
                ValidateFields();

                using (var db = new vacancyEntities())
                {
                    // Проверка на существование пользователя с таким email
                    if (db.Users.Any(u => u.Email == tbEmail.Text))
                    {
                        throw new Exception("Пользователь с таким email уже существует");
                    }

                    // Проверка на существование пользователя с таким телефоном
                    if (db.Users.Any(u => u.Phone == tbPhone.Text))
                    {
                        throw new Exception("Пользователь с таким телефоном уже существует");
                    }

                    // Получаем выбранную роль из ComboBox
                    var selectedRoleItem = (ComboBoxItem)cbRole.SelectedItem;
                    int roleId = int.Parse(selectedRoleItem.Tag.ToString());

                    // Создание нового пользователя
                    var newUser = new Users
                    {
                        FirstName = tbFirstName.Text.Trim(),
                        LastName = tbLastName.Text.Trim(),
                        FatherName = tbFatherName.Text.Trim(),
                        Email = tbEmail.Text.Trim(),
                        Phone = tbPhone.Text.Trim(),
                        Password = HashPassword(pbPassword.Password), // Хеширование пароля
                        RoleId = roleId,
                        RegistrationDate = DateTime.Now
                    };

                    db.Users.Add(newUser);
                    db.SaveChanges();

                    // Успешная регистрация
                    MessageBox.Show(
                        "Регистрация прошла успешно! Теперь вы можете войти в систему.",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    // Переход на страницу авторизации
                    NavigationService?.Navigate(new Autoriz());
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void ValidateFields()
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(tbFirstName.Text))
                throw new Exception("Введите имя");

            if (string.IsNullOrWhiteSpace(tbLastName.Text))
                throw new Exception("Введите фамилию");

            if (string.IsNullOrWhiteSpace(tbEmail.Text))
                throw new Exception("Введите email");

            if (!IsValidEmail(tbEmail.Text))
                throw new Exception("Введите корректный email");

            if (string.IsNullOrWhiteSpace(tbPhone.Text))
                throw new Exception("Введите телефон");

            if (!IsValidPhone(tbPhone.Text))
                throw new Exception("Введите корректный телефон (формат: +71234567890)");

            if (cbRole.SelectedItem == null)
                throw new Exception("Выберите роль");

            if (string.IsNullOrWhiteSpace(pbPassword.Password))
                throw new Exception("Введите пароль");

            if (pbPassword.Password.Length < 6)
                throw new Exception("Пароль должен содержать минимум 6 символов");

            if (pbPassword.Password != pbConfirmPassword.Password)
                throw new Exception("Пароли не совпадают");
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            try
            {
                var regex = new Regex(@"^\+?[0-9]{11,15}$");
                return regex.IsMatch(phone);
            }
            catch
            {
                return false;
            }
        }

        private string HashPassword(string password)
        {
            // В реальном проекте используйте:
            // return BCrypt.Net.BCrypt.HashPassword(password);
            return password; // Замените на реальное хеширование в продакшене
        }

        private void ShowError(string message)
        {
            tbError.Text = message;
            errorBorder.Visibility = Visibility.Visible;
        }

        private void ResetErrors()
        {
            errorBorder.Visibility = Visibility.Collapsed;
            tbError.Text = string.Empty;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Autoriz());
        }

        private void cbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Можно добавить дополнительную логику при изменении выбора роли
        }
    }
}