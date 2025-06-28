using kursach.AppData;
using System;
using System.Data.Entity;
using System.Globalization;
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
    public partial class ResumeDetailsPage : Page
    {
        private readonly vacancyEntities db = new vacancyEntities();
        private int _resumeId;
        private Resumes _resume;

        public ResumeDetailsPage(int resumeId)
        {
            InitializeComponent();
            _resumeId = resumeId;
            Loaded += ResumeDetailsPage_Loaded;
        }

        private void ResumeDetailsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadResumeData();
        }

        private void LoadResumeData()
        {
            try
            {
                _resume = db.Resumes
                    .Include(r => r.Users)
                    .Include(r => r.Cities)
                    .Include(r => r.EmploymentTypes)
                    .Include(r => r.Educations)
                    .Include(r => r.Skills)
                    .Include(r => r.ResumeEducations.Select(re => re.Educations))
                    .Include(r => r.WorkExperiences)
                    .FirstOrDefault(r => r.Id == _resumeId);

                if (_resume == null)
                {
                    MessageBox.Show("Резюме не найдено", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService.GoBack();
                    return;
                }

                // Основная информация
                ResumeTitle.Text = _resume.Title;
                UserName.Text = $"{_resume.Users.LastName} {_resume.Users.FirstName}";
                SalaryText.Text = _resume.SalaryExpectation.HasValue ?
                    $"{_resume.SalaryExpectation:N0} руб." : "Зарплата не указана";
                CityText.Text = _resume.Cities?.Name ?? "Город не указан";
                EmploymentTypeText.Text = _resume.EmploymentTypes?.Name ?? "Тип занятости не указан";
                EducationText.Text = _resume.Educations?.Name ?? "Образование не указано";
                ExperienceText.Text = _resume.ExperienceYears.HasValue ?
                    $"{_resume.ExperienceYears} лет опыта" : "Опыт не указан";

                // Фото
                if (!string.IsNullOrEmpty(_resume.PhotoPath))
                {
                    try
                    {
                        var bitmap = new BitmapImage(new Uri(_resume.PhotoPath, UriKind.RelativeOrAbsolute));
                        UserPhoto.Source = bitmap;
                    }
                    catch
                    {
                        // Если не удалось загрузить фото, оставляем placeholder
                    }
                }

                // О себе
                AboutMeText.Text = _resume.AboutMe ?? "Информация о себе не указана";

                // Навыки
                SkillsItemsControl.ItemsSource = _resume.Skills.ToList();

                // Образование
                EducationItemsControl.ItemsSource = _resume.ResumeEducations.Select(re => new
                {
                    Institution = re.Institution,
                    Specialization = re.Specialization,
                    EducationLevelName = re.Educations?.Name ?? "Не указано",
                    YearCompleted = re.YearCompleted
                }).ToList();

                // Опыт работы
                ExperienceItemsControl.ItemsSource = _resume.WorkExperiences.Select(we => new
                {
                    we.CompanyName,
                    we.Position,
                    we.StartDate,
                    we.EndDate,
                    we.IsCurrent,
                    we.Description
                }).ToList();

                // Контакты
                EmailText.Text = _resume.Users.Email;
                PhoneText.Text = _resume.Users.Phone ?? "Не указан";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных резюме: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void RespondButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем авторизацию
            if (!CurrentUser.IsAuthenticated)
            {
                MessageBox.Show("Для отправки отклика необходимо авторизоваться",
                    "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверяем, что пользователь - работодатель
            if (!CurrentUser.IsEmployer)
            {
                MessageBox.Show("Только работодатели могут отправлять отклики на резюме",
                    "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Логика отправки отклика на резюме
            try
            {
                using (var db = new vacancyEntities())
                {
                    // Проверяем, не отправлял ли уже работодатель отклик на это резюме
                    var existingResponse = db.FavoriteResumes
                        .FirstOrDefault(fr => fr.UserId == AppData.CurrentUser.Id && fr.ResumeId == _resumeId);

                    if (existingResponse != null)
                    {
                        MessageBox.Show("Вы уже отправляли отклик на это резюме",
                            "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // Создаем новый отклик
                    var newFavorite = new FavoriteResumes
                    {
                        UserId = AppData.CurrentUser.Id,
                        ResumeId = _resumeId,
                        AddedDate = DateTime.Now
                    };

                    db.FavoriteResumes.Add(newFavorite);
                    db.SaveChanges();

                    MessageBox.Show("Отклик успешно отправлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке отклика: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CurrentUser.IsAuthenticated)
            {
                MessageBox.Show("Для добавления в избранное необходимо авторизоваться",
                    "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Логика добавления в избранное
            try
            {
                using (var db = new vacancyEntities())
                {
                    // Проверяем, не добавлено ли уже резюме в избранное
                    var existingFavorite = db.FavoriteResumes
                        .FirstOrDefault(fr => fr.UserId == AppData.CurrentUser.Id && fr.ResumeId == _resumeId);

                    if (existingFavorite != null)
                    {
                        MessageBox.Show("Это резюме уже в вашем избранном",
                            "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // Добавляем в избранное
                    var newFavorite = new FavoriteResumes
                    {
                        UserId = AppData.CurrentUser.Id,
                        ResumeId = _resumeId,
                        AddedDate = DateTime.Now
                    };

                    db.FavoriteResumes.Add(newFavorite);
                    db.SaveChanges();

                    MessageBox.Show("Резюме добавлено в избранное", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении в избранное: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}