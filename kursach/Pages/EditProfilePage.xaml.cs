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

            _user.LastName = LastNameTextBox.Text;
            _user.FirstName = FirstNameTextBox.Text;
            _user.FatherName = FatherNameTextBox.Text;
            _user.Phone = PhoneTextBox.Text;

            try
            {
                _db.SaveChanges();
                CurrentUser.SetUser(_user);
                MessageBox.Show("Данные успешно сохранены");
                NavigationService?.GoBack();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }
    }
}