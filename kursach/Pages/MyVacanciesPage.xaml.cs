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

namespace kursach.Pages
{
    public partial class MyVacanciesPage : Page
    {
        private readonly vacancyEntities db = new vacancyEntities();

        public MyVacanciesPage()
        {
            InitializeComponent();
            LoadVacancies();
        }

        private void LoadVacancies()
        {
            try
            {
                var vacancies = db.Vacancies
                    .Include(v => v.Cities)
                    .Include(v => v.EmploymentTypes)
                    .Include(v => v.Companies)
                    .Where(v => v.CompanyId == CurrentUser.Id && v.IsActive)
                    .OrderByDescending(v => v.UpdatedDate)
                    .ToList();

                VacanciesItemsControl.ItemsSource = vacancies;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки вакансий: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateVacancyButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditVacancyPage());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void EditVacancyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int vacancyId)
            {
                NavigationService.Navigate(new EditVacancyPage(vacancyId));
            }
        }

        private void DeleteVacancyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int vacancyId)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту вакансию?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var vacancy = db.Vacancies.Find(vacancyId);
                        if (vacancy != null)
                        {
                            vacancy.IsActive = false;
                            vacancy.UpdatedDate = DateTime.Now;
                            db.SaveChanges();
                            LoadVacancies();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении вакансии: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}