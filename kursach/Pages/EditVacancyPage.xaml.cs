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
    public partial class EditVacancyPage : Page
    {
        private readonly vacancyEntities db = new vacancyEntities();
        private Vacancies _currentVacancy;

        public EditVacancyPage(int? vacancyId = null)
        {
            InitializeComponent();
            LoadData(vacancyId);
        }

        private void LoadData(int? vacancyId)
        {
            try
            {
                // Загрузка справочников
                CityComboBox.ItemsSource = db.Cities.ToList();
                EmploymentTypeComboBox.ItemsSource = db.EmploymentTypes.ToList();
                CompanyComboBox.ItemsSource = db.Companies.Where(c => c.Id == CurrentUser.Id).ToList();

                if (vacancyId.HasValue)
                {
                    _currentVacancy = db.Vacancies.Find(vacancyId.Value);
                    if (_currentVacancy != null)
                    {
                        // Заполняем поля данными
                        TitleTextBox.Text = _currentVacancy.Title;
                        SalaryFromTextBox.Text = _currentVacancy.SalaryFrom?.ToString();
                        DescriptionTextBox.Text = _currentVacancy.Description;
                        RequirementsTextBox.Text = _currentVacancy.Requirements;
                        ResponsibilitiesTextBox.Text = _currentVacancy.Responsibilities;
                        ExperienceTextBox.Text = _currentVacancy.ExperienceRequired?.ToString();

                        if (_currentVacancy.CityId.HasValue)
                            CityComboBox.SelectedValue = _currentVacancy.CityId.Value;

                        if (_currentVacancy.EmploymentTypeId.HasValue)
                            EmploymentTypeComboBox.SelectedValue = _currentVacancy.EmploymentTypeId.Value;

                        if (_currentVacancy.CompanyId > 0)
                            CompanyComboBox.SelectedValue = _currentVacancy.CompanyId;
                    }
                }
                else
                {
                    _currentVacancy = new Vacancies
                    {
                        CompanyId = CurrentUser.Id,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        IsActive = true,
                        ViewsCount = 0
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
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text) ||
                string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ||
                CompanyComboBox.SelectedItem == null)
            {
                MessageBox.Show("Поля с * обязательны для заполнения", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _currentVacancy.Title = TitleTextBox.Text.Trim();
                _currentVacancy.Description = DescriptionTextBox.Text.Trim();
                _currentVacancy.Requirements = RequirementsTextBox.Text.Trim();
                _currentVacancy.Responsibilities = ResponsibilitiesTextBox.Text.Trim();
                _currentVacancy.UpdatedDate = DateTime.Now;

                if (decimal.TryParse(SalaryFromTextBox.Text, out decimal salary))
                    _currentVacancy.SalaryFrom = salary;
                else
                    _currentVacancy.SalaryFrom = null;

                if (int.TryParse(ExperienceTextBox.Text, out int experience))
                    _currentVacancy.ExperienceRequired = experience;
                else
                    _currentVacancy.ExperienceRequired = null;

                _currentVacancy.CityId = (CityComboBox.SelectedItem as Cities)?.Id;
                _currentVacancy.EmploymentTypeId = (EmploymentTypeComboBox.SelectedItem as EmploymentTypes)?.Id;
                _currentVacancy.CompanyId = (CompanyComboBox.SelectedItem as Companies).Id;

                if (_currentVacancy.Id == 0)
                {
                    db.Vacancies.Add(_currentVacancy);
                }

                db.SaveChanges();
                MessageBox.Show("Вакансия успешно сохранена", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new MyVacanciesPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения вакансии: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }
    }
}