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
    public partial class MyResumesPage : Page
    {
        private readonly vacancyEntities db = new vacancyEntities();

        public MyResumesPage()
        {
            InitializeComponent();
            LoadResumes();
        }

        private void LoadResumes()
        {
            try
            {
                var resumes = db.Resumes
                    .Include("Cities")
                    .Include("EmploymentTypes")
                    .Where(r => r.UserId == CurrentUser.Id && r.IsActive)
                    .OrderByDescending(r => r.UpdatedDate)
                    .ToList();

                ResumesItemsControl.ItemsSource = resumes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки резюме: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateResumeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditResumePage());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) 
        {
            NavigationService.GoBack();
        }

        private void EditResumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int resumeId)
            {
                NavigationService.Navigate(new EditResumePage(resumeId));
            }
        }

        private void DeleteResumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int resumeId)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить это резюме?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var resume = db.Resumes.Find(resumeId);
                        if (resume != null)
                        {
                            resume.IsActive = false;
                            resume.UpdatedDate = DateTime.Now;
                            db.SaveChanges();
                            LoadResumes();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении резюме: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}