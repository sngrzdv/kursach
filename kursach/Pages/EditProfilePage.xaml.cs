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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace kursach.Pages
{
    public partial class EditProfilePage : Page
    {
        private vacancyEntities _db = new vacancyEntities();
        private Users _user;

        public EditProfilePage(Users user)
        {
            InitializeComponent();
            _user = user;
            LoadUserData();
        }

        private void LoadUserData()
        {
            LastNameTextBox.Text = _user.LastName;
            FirstNameTextBox.Text = _user.FirstName;
            FatherNameTextBox.Text = _user.FatherName;
            PhoneTextBox.Text = _user.Phone;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(PhoneTextBox.Text, @"^\+?[0-9\s\-\(\)]{10,}$"))
            {
                MessageBox.Show("Введите корректный номер телефона");
                return;
            }

            try
            {
                using (var db = new vacancyEntities()) // Новый контекст для сохранения
                {
                    // Находим пользователя в БД
                    var userToUpdate = db.Users.FirstOrDefault(u => u.Id == _user.Id);

                    if (userToUpdate == null)
                    {
                        MessageBox.Show("Пользователь не найден!");
                        return;
                    }

                    // Обновляем данные
                    userToUpdate.LastName = LastNameTextBox.Text;
                    userToUpdate.FirstName = FirstNameTextBox.Text;
                    userToUpdate.FatherName = FatherNameTextBox.Text;
                    userToUpdate.Phone = PhoneTextBox.Text;

                    db.SaveChanges(); // Сохраняем в БД

                    // Обновляем текущего пользователя в системе
                    CurrentUser.SetUser(userToUpdate);

                    MessageBox.Show("Данные успешно сохранены!");

                    // Вариант 1: Просто вернуться назад
                    NavigationService?.GoBack();

                    // Вариант 2: Принудительно обновить страницу профиля
                    NavigationService?.Navigate(new UserAccount());
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var errors = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);
                MessageBox.Show($"Ошибки валидации:\n{string.Join("\n", errors)}");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}