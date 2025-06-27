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
    public partial class UserPage : Page
    {
        private readonly vacancyEntities db = new vacancyEntities();
        private IQueryable<Vacancies> _allVacanciesQuery;
        private bool _isInitialized = false;

        public UserPage()
        {
            InitializeComponent();
            Loaded += UserPage_Loaded;
        }

        private void UserPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
            {
                LoadData();
                SetupEventHandlers();
                _isInitialized = true;
            }
        }

        private void LoadData()
        {
            try
            {
                _allVacanciesQuery = db.Vacancies
                    .Include(v => v.Companies)
                    .Include(v => v.Cities)
                    .Include(v => v.EmploymentTypes)
                    .Where(v => v.IsActive);

                CityFilterComboBox.ItemsSource = db.Cities.ToList();
                CityFilterComboBox.DisplayMemberPath = "Name";
                CityFilterComboBox.SelectedIndex = -1;

                UpdateVacanciesList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupEventHandlers()
        {
            // Обработчики фильтров
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

            SortByDate.Checked += (s, e) => UpdateVacanciesList();
            SortBySalary.Checked += (s, e) => UpdateVacanciesList();
            SortByRelevance.Checked += (s, e) => UpdateVacanciesList();

            ResetFiltersButton.Click += (s, e) => ResetFilters();

            SalaryFromTextBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
            SalaryToTextBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
            SalaryFromTextBox.TextChanged += (s, e) => UpdateVacanciesList();
            SalaryToTextBox.TextChanged += (s, e) => UpdateVacanciesList();

            // Добавляем обработчик клика по карточке вакансии
            VacanciesListView.PreviewMouseLeftButtonUp += VacanciesListView_PreviewMouseLeftButtonUp;
        }

        private void VacanciesListView_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Получаем элемент, по которому кликнули
            var item = ItemsControl.ContainerFromElement(VacanciesListView, e.OriginalSource as DependencyObject) as ListViewItem;

            if (item != null)
            {
                // Получаем данные вакансии
                dynamic selectedVacancy = item.Content;
                int vacancyId = selectedVacancy.Id;

                // Открываем страницу с деталями вакансии
                NavigationService.Navigate(new VacancyDetails(vacancyId));
            }
        }

        private void UpdateVacanciesList()
        {
            try
            {
                var filtered = _allVacanciesQuery;

                if (!string.IsNullOrWhiteSpace(SearchBox.Text))
                {
                    var searchText = SearchBox.Text.ToLower();
                    filtered = filtered.Where(v =>
                        v.Title.ToLower().Contains(searchText) ||
                        v.Companies.Name.ToLower().Contains(searchText) ||
                        v.Description.ToLower().Contains(searchText));
                }

                if (CityFilterComboBox.SelectedItem is Cities selectedCity)
                {
                    filtered = filtered.Where(v => v.CityId == selectedCity.Id);
                }

                var employmentTypes = new System.Collections.Generic.List<int>();
                if (FullTimeCheckBox.IsChecked == true) employmentTypes.Add(1);
                if (PartTimeCheckBox.IsChecked == true) employmentTypes.Add(2);
                if (RemoteCheckBox.IsChecked == true) employmentTypes.Add(5);
                if (ProjectCheckBox.IsChecked == true) employmentTypes.Add(3);

                if (employmentTypes.Count > 0)
                {
                    filtered = filtered.Where(v => employmentTypes.Contains(v.EmploymentTypeId ?? 0));
                }

                if (decimal.TryParse(SalaryFromTextBox.Text, out decimal minSalary))
                {
                    filtered = filtered.Where(v => v.SalaryFrom >= minSalary);
                }

                if (decimal.TryParse(SalaryToTextBox.Text, out decimal maxSalary))
                {
                    filtered = filtered.Where(v => v.SalaryFrom <= maxSalary);
                }

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

                VacanciesListView.ItemsSource = filtered.ToList().Select(v => new
                {
                    v.Id,
                    Position = v.Title,
                    Company = v.Companies.Name,
                    Salary = v.SalaryFrom.HasValue ? $"{v.SalaryFrom:N0} руб." : "Договорная",
                    City = v.Cities?.Name ?? "Не указан",
                    EmploymentType = v.EmploymentTypes?.Name ?? "Не указан",
                    v.ViewsCount,
                    v.CreatedDate
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetFilters()
        {
            SearchBox.Text = string.Empty;
            CityFilterComboBox.SelectedIndex = -1;

            FullTimeCheckBox.IsChecked = false;
            PartTimeCheckBox.IsChecked = false;
            RemoteCheckBox.IsChecked = false;
            ProjectCheckBox.IsChecked = false;

            SalaryFromTextBox.Text = string.Empty;
            SalaryToTextBox.Text = string.Empty;

            SortByRelevance.IsChecked = true;
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        // Остальные методы оставляем без изменений
        private void VacanciesButton_Click(object sender, RoutedEventArgs e) { }
        private void MyResumesButton_Click(object sender, RoutedEventArgs e) { }
        private void ResponsesButton_Click(object sender, RoutedEventArgs e) { }
        private void FavoritesButton_Click(object sender, RoutedEventArgs e) { }
        private void NotificationsButton_Click(object sender, RoutedEventArgs e) { }
        private void ProfileButton_Click(object sender, RoutedEventArgs e) { }
        private void VacanciesButton_Click_1(object sender, RoutedEventArgs e) { }
    }
}