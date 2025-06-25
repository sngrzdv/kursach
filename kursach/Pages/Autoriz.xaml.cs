using System;
using kursach.AppData;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace kursach.Pages
{
    public partial class Autoriz : Page
    {
        // Подключение к базе данных
        private readonly vacancyEntities db = new vacancyEntities();

        public Autoriz()
        {
            InitializeComponent();

            // Установка фокуса на поле email при загрузке
            Loaded += (s, e) => tbLogin.Focus();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем данные из полей ввода
                string email = tbLogin.Text.Trim();
                string password = pbPassword.Password;

                // Валидация полей
                if (string.IsNullOrEmpty(email))
                {
                    ShowError("Введите email");
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    ShowError("Введите пароль");
                    return;
                }

                // Поиск пользователя в базе
                var user = db.Users.FirstOrDefault(u => u.Email == email);

                if (user == null)
                {
                    ShowError("Пользователь не найден");
                    return;
                }

                // Проверка пароля (в реальном проекте используйте хеширование!)
                if (user.Password != password)
                {
                    ShowError("Неверный пароль");
                    return;
                }

                // Сохраняем данные пользователя
                CurrentUser.Id = user.Id;
                CurrentUser.RoleId = user.RoleId;
                CurrentUser.FullName = $"{user.LastName} {user.FirstName}";
                CurrentUser.Email = user.Email;

                // Обновляем дату последнего входа
                user.LastLoginDate = DateTime.Now;
                db.SaveChanges();

                // Перенаправляем в зависимости от роли
                switch (user.RoleId)
                {
                    case 1: // Администратор
                        NavigationService.Navigate(new AdminPage());
                        break;
                    case 2: // Работодатель
                        NavigationService.Navigate(new ManagerPage());
                        break;
                    case 3: // Соискатель
                        NavigationService.Navigate(new UserPage());
                        break;
                    default:
                        ShowError("Неизвестная роль пользователя");
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка авторизации: {ex.Message}");
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            // Переход на страницу регистрации
            NavigationService.Navigate(new Registr());
        }

        private void ShowError(string message)
        {
            tbError.Text = message;
            errorBorder.Visibility = Visibility.Visible;
        }
    }
}