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
    /// <summary>
    /// Логика взаимодействия для UserPage.xaml
    /// </summary>
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
            SetupFilters();
        }

        private void LoadData()
        {
            try
            {
                // Загрузка данных из базы
                Vacancies = db.Vacancies
                    .Include(v => v.Companies)
                    .Include(v => v.Cities)
                    .Include(v => v.EmploymentTypes)
                    .Where(v => v.IsActive)
                    .ToList();

                // Загрузка городов для фильтра
                CityFilterComboBox.ItemsSource = db.Cities.ToList();
                CityFilterComboBox.DisplayMemberPath = "Name";
                CityFilterComboBox.SelectedIndex = -1;

                UpdateVacanciesList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void SetupFilters()
        {
            // Обработчики событий для фильтров
            SearchBox.TextChanged += (s, e) => UpdateVacanciesList();
            CityFilterComboBox.SelectionChanged += (s, e) => UpdateVacanciesList();
            FullTimeCheckBox.Checked += (s, e) => UpdateVacanciesList();
            FullTimeCheckBox.Unchecked += (s, e) => UpdateVacanciesList();
            PartTimeCheckBox.Checked += (s, e) => UpdateVacanciesList();
            PartTimeCheckBox.Unchecked += (s, e) => UpdateVacanciesList();
            RemoteCheckBox.Checked += (s, e) => UpdateVacanciesList();
            RemoteCheckBox.Unchecked += (s, e) => UpdateVacanciesList();
            ProjectCheckBox.Checked += (s, e) => UpdateVacanciesList();
            ProjectCheckBox.Unchecked += (s, e) => UpdateVacanciesList();
            SalaryFromTextBox.TextChanged += (s, e) => UpdateVacanciesList();
            SalaryToTextBox.TextChanged += (s, e) => UpdateVacanciesList();
            SortByDate.Checked += (s, e) => UpdateVacanciesList();
            SortBySalary.Checked += (s, e) => UpdateVacanciesList();
            SortByRelevance.Checked += (s, e) => UpdateVacanciesList();

            ResetFiltersButton.Click += (s, e) => ResetFilters();
        }

        private void UpdateVacanciesList()
        {
            try
            {
                var filtered = allVacancies.AsQueryable();

                // Поиск
                if (!string.IsNullOrWhiteSpace(SearchBox.Text))
                {
                    var search = SearchBox.Text.ToLower();
                    filtered = filtered.Where(v =>
                        v.Title.ToLower().Contains(search) ||
                        v.Companies.Name.ToLower().Contains(search));
                }

                // Фильтр по городу
                if (CityFilterComboBox.SelectedItem is Cities city)
                {
                    filtered = filtered.Where(v => v.CityId == city.Id);
                }

                // Фильтр по типу занятости
                var types = new List<int?>();
                if (FullTimeCheckBox.IsChecked == true) types.Add(1);
                if (PartTimeCheckBox.IsChecked == true) types.Add(2);
                if (RemoteCheckBox.IsChecked == true) types.Add(3);
                if (ProjectCheckBox.IsChecked == true) types.Add(4);

                if (types.Count > 0)
                {
                    filtered = filtered.Where(v => types.Contains(v.EmploymentTypeId));
                }
                  
                // Фильтр по зарплате
                if (decimal.TryParse(SalaryFromTextBox.Text, out decimal min))
                {
                    filtered = filtered.Where(v => v.SalaryFrom >= min);
                }

                if (decimal.TryParse(SalaryToTextBox.Text, out decimal max))
                {
                    filtered = filtered.Where(v => v.SalaryFrom <= max);
                }

                // Сортировка
                if (SortBySalary.IsChecked == true)
                {
                    filtered = filtered.OrderByDescending(v => v.SalaryFrom);
                }
                else if (SortByDate.IsChecked == true)
                {
                    filtered = filtered.OrderByDescending(v => v.CreatedDate);
                }
                else
                {
                    filtered = filtered.OrderByDescending(v => v.ViewsCount);
                }

                // Отображение
                VacanciesListView.ItemsSource = filtered.ToList().Select(v => new
                {
                    v.Id,
                    Position = v.Title,
                    Company = v.Companies.Name,
                    Salary = v.SalaryFrom.HasValue ? $"{v.SalaryFrom:N0} руб." : "Договорная",
                    City = v.Cities?.Name ?? "Не указан",
                    EmploymentType = v.EmploymentTypes?.Name ?? "Не указан",
                    IsFavorite = db.FavoriteVacancies.Any(f => f.UserId == currentUserId && f.VacancyId == v.Id)
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка фильтрации: " + ex.Message);
            }
        }

        private void ResetFilters()
        {
            SearchBox.Text = "";
            CityFilterComboBox.SelectedIndex = -1;
            FullTimeCheckBox.IsChecked = false;
            PartTimeCheckBox.IsChecked = false;
            RemoteCheckBox.IsChecked = false;
            ProjectCheckBox.IsChecked = false;
            SalaryFromTextBox.Text = "";
            SalaryToTextBox.Text = "";
            SortByRelevance.IsChecked = true;
        }

        private void ToggleFavorite_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is dynamic vacancy)
            {
                try
                {
                    if (vacancy.IsFavorite)
                    {
                        var fav = db.FavoriteVacancies
                            .FirstOrDefault(f => f.UserId == currentUserId && f.VacancyId == vacancy.Id);
                        if (fav != null)
                        {
                            db.FavoriteVacancies.Remove(fav);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        db.FavoriteVacancies.Add(new FavoriteVacancies
                        {
                            UserId = currentUserId,
                            VacancyId = vacancy.Id,
                            AddedDate = DateTime.Now
                        });
                        db.SaveChanges();
                    }
                    UpdateVacanciesList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }

        private void NavigateToProfile_Click(object sender, RoutedEventArgs e)
        {
            // NavigationService.Navigate(new ProfilePage(currentUserId));
        }
    }
}