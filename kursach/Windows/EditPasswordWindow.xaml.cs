using kursach.AppData;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        private readonly vacancyEntities _db;
        private readonly Users _user;

        public EditPasswordWindow(Users user)
        {
            InitializeComponent();
            _db = new vacancyEntities();

            // Загружаем свежие данные пользователя из БД
            _user = _db.Users.Find(user.Id);

            if (_user == null)
            {
                MessageBox.Show("Пользователь не найден");
                Close();
                return;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка текущего пароля
            if (_user.Password != CurrentPasswordBox.Password)
            {
                MessageBox.Show("Неверный текущий пароль");
                return;
            }

            // Проверка нового пароля
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

            // Проверка совпадения паролей
            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают");
                return;
            }

            try
            {
                // Обновляем пароль
                _user.Password = NewPasswordBox.Password;

                // Явно отмечаем сущность как измененную
                _db.Entry(_user).State = EntityState.Modified;

                // Сохраняем изменения в БД
                _db.SaveChanges();

                MessageBox.Show("Пароль успешно изменен");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении пароля: {ex.Message}");
                // Логирование ошибки (добавьте при необходимости)
                // Logger.Log(ex);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _db?.Dispose(); // Освобождаем ресурсы контекста БД
        }
    }
}