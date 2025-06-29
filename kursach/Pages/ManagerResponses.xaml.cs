using kursach.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
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
    public partial class ManagerResponses : Page
    {
        private readonly vacancyEntities db = new vacancyEntities();

        public ManagerResponses()
        {
            InitializeComponent();
            LoadResponses();
        }

        private void LoadResponses()
        {
            try
            {
                var responses = db.VacancyResponses
                    .Include(r => r.Vacancies)
                    .Include(r => r.Resumes)
                    .Include(r => r.ResponseStatuses)
                    .Where(r => r.Vacancies.CompanyId == CurrentUser.Id)
                    .OrderByDescending(r => r.ResponseDate)
                    .ToList();

                ResponsesItemsControl.ItemsSource = responses;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки откликов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void ViewResumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int responseId)
            {
                var response = db.VacancyResponses
                    .Include(r => r.Resumes)
                    .Include(r => r.Resumes.Users)
                    .FirstOrDefault(r => r.Id == responseId);

                if (response != null && response.Resumes != null)
                {
                    // Здесь код для открытия резюме
                    MessageBox.Show($"Просмотр резюме пользователя {response.Resumes.Users.Id}",
                        "Резюме", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Резюме не найдено", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void ChangeStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int responseId)
            {
                // Здесь код для изменения статуса отклика
                /*NavigationService.Navigate(new ChangeResponseStatusPage(responseId));*/
            }
        }
    }
}