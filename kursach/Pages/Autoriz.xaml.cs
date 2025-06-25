using System;
using kursach.AppData;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace kursach.Pages
{
    /// <summary>
    /// Логика взаимодействия для Autoriz.xaml
    /// </summary>
    public partial class Autoriz : Page
    {
        public Autoriz()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = tbLogin.Text.Trim();
            string password = pbPassword.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ShowError("Пожалуйста, заполните все поля");
                return;
            }

            try
            {
                using (var context = new vacancyEntities()) // Замените YourDbContext на ваш контекст БД
                {
                    // Ищем пользователя по email (логину)
                    var user = context.Users.FirstOrDefault(u => u.Email == login);

                    if (user == null)
                    {
                        ShowError("Пользователь с таким email не найден");
                        return;
                    }

                    // Проверяем пароль (в реальном проекте используйте хеширование!)
                    if (user.Password != password)
                    {
                        ShowError("Неверный пароль");
                        return;
                    }

                    // Обновляем дату последнего входа
                    user.LastLoginDate = DateTime.Now;
                    context.SaveChanges();

                    // Сохраняем данные пользователя (например, в статическом классе)
                    CurrentUser.Id = user.Id;
                    CurrentUser.RoleId = user.RoleId;
                    CurrentUser.FullName = $"{user.LastName} {user.FirstName} {user.FatherName}";

                    // Переходим на главную страницу в зависимости от роли
                    switch (user.RoleId)
                    {
                    //    *//*case 1: // Администратор
                    //        NavigationService.Navigate(new AdminPage());
                    //break;
                    //    case 2: // Работодатель
                    //    NavigationService.Navigate(new EmployerPage());
                    //    break; *//*
                        case 3: // Соискатель
                        NavigationService.Navigate(new UserPage());
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при авторизации: {ex.Message}");
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

    // Статический класс для хранения данных текущего пользователя
    public static class CurrentUser
    {
        public static int Id { get; set; }
        public static int RoleId { get; set; }
        public static string FullName { get; set; }
    }
}