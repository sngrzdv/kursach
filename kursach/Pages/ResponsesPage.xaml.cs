using kursach.AppData;
using System;
using System.Data.Entity;
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
using kursach.Windows;

namespace kursach.Pages
{

    public partial class ResponsesPage : Page
    {
        private readonly vacancyEntities db = new vacancyEntities();

        public ResponsesPage()
        {
            InitializeComponent();
            LoadResponses();
        }

        private void LoadResponses()
        {
            try
            {
                var responses = db.VacancyResponses
                    .Include("Vacancies")
                    .Include("Vacancies.Companies")
                    .Include("ResponseStatuses")
                    .Where(r => r.Resumes.UserId == CurrentUser.Id)
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
            // Возврат на предыдущую страницу
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                // Если нет истории навигации, переходим на главную страницу пользователя
                NavigationService.Navigate(new UserPage());
            }
        }
        private void ResponseItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1)
            {
                var border = sender as Border;
                var response = border.DataContext as VacancyResponses;

                // Проверяем, что статус "Принято" (или другой, по вашему усмотрению)
                if (response.ResponseStatuses.Name == "Принято")
                {
                    var dialog = new ScheduleInterviewDialog(response);
                    dialog.Owner = Window.GetWindow(this);
                    dialog.ShowDialog();

                    // Обновляем данные после закрытия диалога (если нужно)
                    LoadResponses();
                }
            }
        }
    }
}