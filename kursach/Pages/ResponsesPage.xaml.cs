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
    }
}