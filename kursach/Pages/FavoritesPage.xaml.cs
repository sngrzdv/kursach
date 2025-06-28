using kursach.AppData;
using System;
using System.Data.Entity;
using System.Linq;
using System.Text;
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
    public partial class FavoritesPage : Page
    {
        private readonly vacancyEntities db = new vacancyEntities();

        public FavoritesPage()
        {
            InitializeComponent();
            Loaded += FavoritesPage_Loaded;
        }

        private void FavoritesPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadFavorites();
        }

        private void LoadFavorites()
        {
            try
            {
                if (CurrentUser.IsEmployer)
                {
                    // Загрузка избранных резюме для работодателя
                    PageTitle.Text = "Избранные резюме";
                    FavoritesList.ItemTemplate = (DataTemplate)Resources["ResumeCardTemplate"];

                    var favoriteResumes = db.FavoriteResumes
                        .Include(fr => fr.Resumes)
                        .Include(fr => fr.Resumes.Users)
                        .Include(fr => fr.Resumes.Cities)
                        .Where(fr => fr.UserId == CurrentUser.Id)
                        .OrderByDescending(fr => fr.AddedDate)
                        .ToList();

                    FavoritesList.ItemsSource = favoriteResumes;
                    NoFavoritesText.Visibility = favoriteResumes.Any() ? Visibility.Collapsed : Visibility.Visible;
                }
                else if (CurrentUser.IsJobSeeker)
                {
                    // Загрузка избранных вакансий для соискателя
                    PageTitle.Text = "Избранные вакансии";
                    FavoritesList.ItemTemplate = (DataTemplate)Resources["VacancyCardTemplate"];

                    var favoriteVacancies = db.FavoriteVacancies
                        .Include(fv => fv.Vacancies)
                        .Include(fv => fv.Vacancies.Companies)
                        .Include(fv => fv.Vacancies.Cities)
                        .Include(fv => fv.Vacancies.EmploymentTypes)
                        .Where(fv => fv.UserId == CurrentUser.Id)
                        .OrderByDescending(fv => fv.AddedDate)
                        .ToList();

                    FavoritesList.ItemsSource = favoriteVacancies;
                    NoFavoritesText.Visibility = favoriteVacancies.Any() ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    // Для других ролей (например, админ) или неавторизованных
                    NoFavoritesText.Text = "Функция недоступна для вашей роли";
                    NoFavoritesText.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки избранного: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void FavoriteItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!(sender is Border border) || border.DataContext == null)
                {
                    MessageBox.Show("Не удалось получить данные элемента", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (CurrentUser.IsEmployer)
                {
                    if (border.DataContext is FavoriteResumes favoriteResume)
                    {
                        NavigationService?.Navigate(new ResumeDetailsPage(favoriteResume.ResumeId));
                    }
                }
                else if (CurrentUser.IsJobSeeker)
                {
                    if (border.DataContext is FavoriteVacancies favoriteVacancy)
                    {
                        NavigationService?.Navigate(new VacancyDetails(favoriteVacancy.VacancyId));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при переходе: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}