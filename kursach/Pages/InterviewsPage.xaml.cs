using kursach.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
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
using kursach.Windows;

namespace kursach.Pages
{
    public partial class InterviewsPage : Page
    {
        private readonly vacancyEntities db = new vacancyEntities();
        private int _userId;

        public InterviewsPage(int userId)
        {
            InitializeComponent();
            _userId = userId;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var interviews = db.Interviews
                    .Include(i => i.VacancyResponses)
                    .Include(i => i.VacancyResponses.Vacancies)
                    .Include(i => i.VacancyResponses.Resumes)
                    .Include(i => i.VacancyResponses.Resumes.Users)
                    .Include(i => i.VacancyResponses.ResponseStatuses) // Добавляем загрузку статуса
                    .Where(i => i.VacancyResponses.Vacancies.Companies.UserId == _userId)
                    .ToList();

                InterviewsListView.ItemsSource = interviews.Select(i => new
                {
                    Vacancy = i.VacancyResponses.Vacancies.Title,
                    Applicant = $"{i.VacancyResponses.Resumes.Users.LastName} {i.VacancyResponses.Resumes.Users.FirstName}",
                    Status = i.VacancyResponses.ResponseStatuses?.Name ?? "Не указан", // Берем статус из связанной сущности
                    InterviewDate = i.InterviewDate,
                    Notes = i.Notes ?? "Нет примечаний",
                    Location = string.IsNullOrEmpty(i.OnlineMeetingLink) ? i.Location : "Онлайн"
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private void ScheduleInterviewButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var availableResponses = db.VacancyResponses
                    .Include(vr => vr.Vacancies)
                    .Include(vr => vr.Resumes)
                    .Include(vr => vr.Resumes.Users)
                    .Where(vr => vr.Vacancies.Companies.UserId == _userId &&
                                vr.ResponseStatuses.Name != "Отклонено" &&
                                !db.Interviews.Any(i => i.ResponseId == vr.Id))
                    .Select(vr => new
                    {
                        Id = vr.Id,
                        DisplayText = $"{vr.Vacancies.Title} - {vr.Resumes.Users.LastName} {vr.Resumes.Users.FirstName}"
                    })
                    .ToList();

                if (!availableResponses.Any())
                {
                    MessageBox.Show("Нет доступных откликов для назначения собеседования",
                                   "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dialog = new ScheduleInterviewDialog(availableResponses);
                if (dialog.ShowDialog() == true)
                {
                    var newInterview = new Interviews
                    {
                        ResponseId = dialog.SelectedResponseId,
                        InterviewDate = dialog.InterviewDate,
                        Location = dialog.IsOnline ? null : dialog.Location,
                        OnlineMeetingLink = dialog.IsOnline ? dialog.OnlineMeetingLink : null,
                        Notes = dialog.Notes,
                        IsCompleted = false
                    };

                    db.Interviews.Add(newInterview);
                    db.SaveChanges();
                    LoadData();

                    MessageBox.Show("Собеседование успешно назначено!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при назначении собеседования: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}