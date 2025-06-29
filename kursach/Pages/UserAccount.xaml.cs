using kursach.AppData;
using System;
using kursach.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data.Entity;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace kursach.Pages
{
    public partial class UserAccount : Page
    {
        // Создаем DependencyProperty для CredentialsButtonText
        public static readonly DependencyProperty CredentialsButtonTextProperty =
            DependencyProperty.Register(
                "CredentialsButtonText",
                typeof(string),
                typeof(UserAccount),
                new PropertyMetadata("Показать учетные данные"));

        public string CredentialsButtonText
        {
            get => (string)GetValue(CredentialsButtonTextProperty);
            set => SetValue(CredentialsButtonTextProperty, value);
        }

        private vacancyEntities _db = new vacancyEntities();
        private Users _currentUser;

        public UserAccount()
        {
            InitializeComponent();
            LoadUserData();
        }

        private async void LoadUserData()
        {
            try
            {
                // Загружаем пользователя с включенными связанными данными
                _currentUser = await _db.Users
                    .Include(u => u.Roles)
                    .Include(u => u.FavoriteResumes)
                    .Include(u => u.FavoriteVacancies)
                    .Include(u => u.Resumes)
                    .Include(u => u.Notifications)
                    .FirstOrDefaultAsync(u => u.Id == CurrentUser.Id);

                if (_currentUser == null)
                {
                    MessageBox.Show("Пользователь не найден");
                    NavigationService?.Navigate(new Autoriz());
                    return;
                }

                // Создаем ViewModel с актуальными данными
                DataContext = new UserAccountViewModel
                {
                    User = _currentUser,
                    RoleName = _currentUser.Roles?.Name ?? "Пользователь",
                    CredentialsButtonText = "Показать учетные данные",
                    CanBecomeEmployer = _currentUser.RoleId == 3, // 3 - это ID роли соискателя
                    CanBecomeJobSeeker = _currentUser.RoleId == 2, // 2 - это ID роли работодателя
                    ResponsesCount = 0, // Здесь нужно добавить реальный подсчет откликов
                    FavoritesCount = (_currentUser.FavoriteResumes?.Count ?? 0) +
                                   (_currentUser.FavoriteVacancies?.Count ?? 0),
                    ResumesCount = _currentUser.Resumes?.Count ?? 0,
                    NotificationsCount = _currentUser.Notifications?.Count ?? 0,
                    RegistrationDate = _currentUser.RegistrationDate
                };
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
                NavigationService?.Navigate(new Autoriz());
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new UserPage());
        }

        private void FavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new FavoritesPage());
        }

        private void NotificationsButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new NotificationWindow();
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
            // Обновляем счетчик уведомлений после закрытия окна
            LoadUserData();
        }

        private void EditProfile_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditProfilePage(_currentUser));
        }

        private void ToggleCredentials_Click(object sender, RoutedEventArgs e)
        {
            var vm = (UserAccountViewModel)DataContext;
            if (CredentialsPanel.Visibility == Visibility.Visible)
            {
                CredentialsPanel.Visibility = Visibility.Collapsed;
                vm.CredentialsButtonText = "Показать учетные данные";
            }
            else
            {
                CredentialsPanel.Visibility = Visibility.Visible;
                vm.CredentialsButtonText = "Скрыть учетные данные";
            }
        }

        private void ChangeEmail_Click(object sender, RoutedEventArgs e)
        {
            var window = new EditEmailWindow(_currentUser);
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var window = new EditPasswordWindow(_currentUser);
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
        }

        private void BecomeEmployer_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы точно хотите стать работодателем?\nПосле изменения нужно будет авторизоваться заново.",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _currentUser.RoleId = 2; // ID роли работодателя
                    _db.SaveChanges();

                    CurrentUser.Clear();
                    MessageBox.Show("Роль успешно изменена. Авторизуйтесь заново.");
                    NavigationService.Navigate(new Autoriz());
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }

        private void BecomeJobSeeker_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы точно хотите стать соискателем?\nПосле изменения нужно будет авторизоваться заново.",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _currentUser.RoleId = 3; // ID роли соискателя
                    _db.SaveChanges();

                    CurrentUser.Clear();
                    MessageBox.Show("Роль успешно изменена. Авторизуйтесь заново.");
                    NavigationService.Navigate(new Autoriz());
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите выйти из аккаунта?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                CurrentUser.Clear();
                NavigationService.Navigate(new Autoriz());
            }
        }
    }

    public class UserAccountViewModel
    {
        public Users User { get; set; }
        public string RoleName { get; set; }
        public string CredentialsButtonText { get; set; }
        public bool CanBecomeEmployer { get; set; }
        public bool CanBecomeJobSeeker { get; set; }
        public int ResponsesCount { get; set; }
        public int FavoritesCount { get; set; }
        public int ResumesCount { get; set; }
        public int NotificationsCount { get; set; }
        public System.DateTime RegistrationDate { get; set; }
    }
}