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
    public partial class EditResumePage : Page
    {
        private readonly vacancyEntities db = new vacancyEntities();
        private Resumes _currentResume;

        public EditResumePage(int? resumeId = null)
        {
            InitializeComponent();
            LoadData(resumeId);
        }

        private void LoadData(int? resumeId)
        {
            try
            {
                // Загрузка справочников
                CityComboBox.ItemsSource = db.Cities.ToList();
                EmploymentTypeComboBox.ItemsSource = db.EmploymentTypes.ToList();

                if (resumeId.HasValue)
                {
                    _currentResume = db.Resumes.Find(resumeId.Value);
                    if (_currentResume != null)
                    {
                        // Заполняем поля данными
                        TitleTextBox.Text = _currentResume.Title;
                        SalaryTextBox.Text = _currentResume.SalaryExpectation?.ToString();
                        AboutMeTextBox.Text = _currentResume.AboutMe;

                        if (_currentResume.CityId.HasValue)
                            CityComboBox.SelectedValue = _currentResume.CityId.Value;

                        if (_currentResume.EmploymentTypeId.HasValue)
                            EmploymentTypeComboBox.SelectedValue = _currentResume.EmploymentTypeId.Value;
                    }
                }
                else
                {
                    _currentResume = new Resumes
                    {
                        UserId = CurrentUser.Id,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        IsActive = true
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService.GoBack();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Название резюме обязательно для заполнения", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _currentResume.Title = TitleTextBox.Text.Trim();
                _currentResume.AboutMe = AboutMeTextBox.Text.Trim();
                _currentResume.UpdatedDate = DateTime.Now;

                if (decimal.TryParse(SalaryTextBox.Text, out decimal salary))
                    _currentResume.SalaryExpectation = salary;
                else
                    _currentResume.SalaryExpectation = null;

                _currentResume.CityId = (CityComboBox.SelectedItem as Cities)?.Id;
                _currentResume.EmploymentTypeId = (EmploymentTypeComboBox.SelectedItem as EmploymentTypes)?.Id;

                if (_currentResume.Id == 0)
                {
                    db.Resumes.Add(_currentResume);
                }

                db.SaveChanges();
                MessageBox.Show("Резюме успешно сохранено", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new MyResumesPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения резюме: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void NumericTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }
    }
}