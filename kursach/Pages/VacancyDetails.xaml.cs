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

                // Проверяем состояние кнопок
                if (CurrentUser.IsAuthenticated)
                {
                    using (var db = new vacancyEntities())
                    {
                        // Проверка отклика
                        var resume = db.Resumes.FirstOrDefault(r => r.UserId == CurrentUser.Id);
                        if (resume != null)
                        {
                            var hasResponse = db.VacancyResponses
                                .Any(r => r.ResumeId == resume.Id && r.VacancyId == _vacancyId);

                            if (hasResponse)
                            {
                                RespondButton.Content = "Отклик отправлен";
                                RespondButton.IsEnabled = false;
                            }
                        }

                        // Проверка избранного
                        var isFavorite = db.FavoriteVacancies
                            .Any(f => f.UserId == CurrentUser.Id && f.VacancyId == _vacancyId);

                        if (isFavorite)
                        {
                            FavoriteButton.Content = "В избранном";
                            FavoriteButton.IsEnabled = false;
                        }
                    }
                }

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
                    // Получаем резюме пользователя
                    var resume = db.Resumes.FirstOrDefault(r => r.UserId == CurrentUser.Id);

                    if (resume == null)
                    {
                        MessageBox.Show("Для отклика на вакансию необходимо создать резюме",
                            "Требуется резюме", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Проверяем, есть ли уже отклик на эту вакансию
                    var existingResponse = db.VacancyResponses
                        .FirstOrDefault(r => r.ResumeId == resume.Id && r.VacancyId == _vacancyId);

                    if (existingResponse != null)
                    {
                        MessageBox.Show("Вы уже откликались на эту вакансию",
                            "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // Создаем отклик
                    var response = new VacancyResponses
                    {
                        VacancyId = _vacancyId,
                        ResumeId = resume.Id,
                        ResponseDate = DateTime.Now,
                        StatusId = 1, // Предполагаем, что 1 - это "Отправлен"
                        Message = "Заинтересовала вакансия"
                    };

                    db.VacancyResponses.Add(response);
                    db.SaveChanges();

                    MessageBox.Show("Ваш отклик успешно отправлен!",
                        "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Обновляем состояние кнопки
                    RespondButton.Content = "Отклик отправлен";
                    RespondButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отклике: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CurrentUser.IsAuthenticated)
                {
                    MessageBox.Show("Для добавления вакансии в избранное необходимо авторизоваться",
                        "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var db = new vacancyEntities())
                {
                    // Проверяем, есть ли уже эта вакансия в избранном
                    var existingFavorite = db.FavoriteVacancies
                        .FirstOrDefault(f => f.UserId == CurrentUser.Id && f.VacancyId == _vacancyId);

                    if (existingFavorite != null)
                    {
                        MessageBox.Show("Эта вакансия уже в вашем избранном",
                            "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // Добавляем в избранное
                    var favorite = new FavoriteVacancies
                    {
                        UserId = CurrentUser.Id,
                        VacancyId = _vacancyId,
                        AddedDate = DateTime.Now
                    };

                    db.FavoriteVacancies.Add(favorite);
                    db.SaveChanges();

                    MessageBox.Show("Вакансия добавлена в избранное",
                        "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Обновляем состояние кнопки
                    FavoriteButton.Content = "В избранном";
                    FavoriteButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении в избранное: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}