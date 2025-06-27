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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace kursach.Pages
{
    /// <summary>
    /// Логика взаимодействия для VacancyDetails.xaml
    /// </summary>
    public partial class VacancyDetails : Page
    {
        private readonly vacancyEntities _db = new vacancyEntities();
        private readonly int _vacancyId;
        public VacancyDetails(int vacancyId)
        {
            InitializeComponent();
            _vacancyId = vacancyId;
            Loaded += VacancyDetailsPage_Loaded;
        }

        private void VacancyDetailsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadVacancyDetails();
        }

        private void LoadVacancyDetails()
        {
            try
            {
                var vacancy = _db.Vacancies
                    .Include(v => v.Companies)
                    .Include(v => v.Cities)
                    .Include(v => v.EmploymentTypes)
                    .FirstOrDefault(v => v.Id == _vacancyId);

                if (vacancy == null)
                {
                    MessageBox.Show("Вакансия не найдена", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService.GoBack();
                    return;
                }

                // Заполняем данные
                TitleText.Text = vacancy.Title;
                CompanyText.Text = vacancy.Companies?.Name ?? "Компания не указана";
                SalaryText.Text = vacancy.SalaryFrom.HasValue ?
                    $"{vacancy.SalaryFrom:N0} руб." : "Зарплата не указана";
                CityText.Text = vacancy.Cities?.Name ?? "Город не указан";
                EmploymentTypeText.Text = vacancy.EmploymentTypes?.Name ?? "Тип занятости не указан";
                DescriptionText.Text = vacancy.Description ?? "Описание не указано";
                RequirementsText.Text = vacancy.Requirements ?? "Требования не указаны";
                ResponsibilitiesText.Text = vacancy.Responsibilities ?? "Обязанности не указаны";
                ExperienceText.Text = $"Требуемый опыт: {vacancy.ExperienceRequired ?? 0} лет";
                CreatedDateText.Text = $"Дата публикации: {vacancy.CreatedDate:dd.MM.yyyy}";

                // Увеличиваем счетчик просмотров
                vacancy.ViewsCount++;
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService.GoBack();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CurrentUser.IsAuthenticated)
                {
                    MessageBox.Show("Для отклика на вакансию необходимо авторизоваться",
                        "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!CurrentUser.IsJobSeeker)
                {
                    MessageBox.Show("Только соискатели могут откликаться на вакансии",
                        "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var db = new vacancyEntities())
                {
                    var hasResume = db.Resumes.Any(r => r.UserId == CurrentUser.Id);
                    if (!hasResume)
                    {
                        MessageBox.Show("Для отклика на вакансию необходимо создать резюме",
                            "Требуется резюме", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                /*NavigationService.Navigate(new MyResumePage(_vacancyId));*/
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отклике: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}