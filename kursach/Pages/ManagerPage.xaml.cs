using kursach.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data.Entity;
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
    public partial class ManagerPage : Page
    {
        private readonly vacancyEntities db = new vacancyEntities();
        private IQueryable<Resumes> _allResumesQuery;
        private bool _isInitialized = false;

        public ManagerPage()
        {
            InitializeComponent();
            Loaded += ManagerPage_Loaded;
        }

        private void ManagerPage_Loaded(object sender, RoutedEventArgs e)
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
                _allResumesQuery = db.Resumes
                    .Include(r => r.Users)
                    .Include(r => r.Cities)
                    .Include(r => r.EmploymentTypes)
                    .Include(r => r.Educations)
                    .Where(r => r.IsActive);

                CityFilterComboBox.ItemsSource = db.Cities.ToList();
                CityFilterComboBox.DisplayMemberPath = "Name";
                CityFilterComboBox.SelectedIndex = -1;

                EducationFilterComboBox.ItemsSource = db.Educations.ToList();
                EducationFilterComboBox.DisplayMemberPath = "Name";
                EducationFilterComboBox.SelectedIndex = -1;

                UpdateResumesList();
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
            SearchBox.TextChanged += (s, e) => UpdateResumesList();
            CityFilterComboBox.SelectionChanged += (s, e) => UpdateResumesList();
            EducationFilterComboBox.SelectionChanged += (s, e) => UpdateResumesList();

            FullTimeCheckBox.Checked += (s, e) => UpdateResumesList();
            FullTimeCheckBox.Unchecked += (s, e) => UpdateResumesList();
            PartTimeCheckBox.Checked += (s, e) => UpdateResumesList();
            PartTimeCheckBox.Unchecked += (s, e) => UpdateResumesList();
            RemoteCheckBox.Checked += (s, e) => UpdateResumesList();
            RemoteCheckBox.Unchecked += (s, e) => UpdateResumesList();
            ProjectCheckBox.Checked += (s, e) => UpdateResumesList();
            ProjectCheckBox.Unchecked += (s, e) => UpdateResumesList();

            NoExperience.Checked += (s, e) => UpdateResumesList();
            JuniorExperience.Checked += (s, e) => UpdateResumesList();
            MiddleExperience.Checked += (s, e) => UpdateResumesList();
            SeniorExperience.Checked += (s, e) => UpdateResumesList();

            SortByDate.Checked += (s, e) => UpdateResumesList();
            SortBySalary.Checked += (s, e) => UpdateResumesList();
            SortByRelevance.Checked += (s, e) => UpdateResumesList();

            ResetFiltersButton.Click += (s, e) => ResetFilters();

            SalaryFromTextBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
            SalaryToTextBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
            SalaryFromTextBox.TextChanged += (s, e) => UpdateResumesList();
            SalaryToTextBox.TextChanged += (s, e) => UpdateResumesList();

            // Добавляем обработчик клика по карточке резюме
            ResumesListView.PreviewMouseLeftButtonUp += ResumesListView_PreviewMouseLeftButtonUp;
        }

        private void ResumesListView_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(ResumesListView, e.OriginalSource as DependencyObject) as ListViewItem;

            if (item != null)
            {
                dynamic selectedResume = item.Content;
                int resumeId = selectedResume.Id;

                // Открываем страницу с деталями резюме
                NavigationService.Navigate(new ResumeDetailsPage(resumeId));
            }
        }

        private void UpdateResumesList()
        {
            try
            {
                var filtered = _allResumesQuery;

                if (!string.IsNullOrWhiteSpace(SearchBox.Text))
                {
                    var searchText = SearchBox.Text.ToLower();
                    filtered = filtered.Where(r =>
                        r.Title.ToLower().Contains(searchText) ||
                        r.Users.FirstName.ToLower().Contains(searchText) ||
                        r.Users.LastName.ToLower().Contains(searchText) ||
                        r.AboutMe.ToLower().Contains(searchText));
                }

                if (CityFilterComboBox.SelectedItem is Cities selectedCity)
                {
                    filtered = filtered.Where(r => r.CityId == selectedCity.Id);
                }

                if (EducationFilterComboBox.SelectedItem is Educations selectedEducation)
                {
                    filtered = filtered.Where(r => r.EducationLevelId == selectedEducation.Id);
                }

                var employmentTypes = new System.Collections.Generic.List<int>();
                if (FullTimeCheckBox.IsChecked == true) employmentTypes.Add(1);
                if (PartTimeCheckBox.IsChecked == true) employmentTypes.Add(2);
                if (RemoteCheckBox.IsChecked == true) employmentTypes.Add(5);
                if (ProjectCheckBox.IsChecked == true) employmentTypes.Add(3);

                if (employmentTypes.Count > 0)
                {
                    filtered = filtered.Where(r => employmentTypes.Contains(r.EmploymentTypeId ?? 0));
                }

                // Фильтрация по опыту работы
                if (NoExperience.IsChecked == true)
                {
                    filtered = filtered.Where(r => r.ExperienceYears == 0 || r.ExperienceYears == null);
                }
                else if (JuniorExperience.IsChecked == true)
                {
                    filtered = filtered.Where(r => r.ExperienceYears >= 1 && r.ExperienceYears <= 3);
                }
                else if (MiddleExperience.IsChecked == true)
                {
                    filtered = filtered.Where(r => r.ExperienceYears >= 3 && r.ExperienceYears <= 5);
                }
                else if (SeniorExperience.IsChecked == true)
                {
                    filtered = filtered.Where(r => r.ExperienceYears >= 5);
                }

                if (decimal.TryParse(SalaryFromTextBox.Text, out decimal minSalary))
                {
                    filtered = filtered.Where(r => r.SalaryExpectation >= minSalary);
                }

                if (decimal.TryParse(SalaryToTextBox.Text, out decimal maxSalary))
                {
                    filtered = filtered.Where(r => r.SalaryExpectation <= maxSalary);
                }

                if (SortBySalary.IsChecked == true)
                {
                    filtered = filtered.OrderByDescending(r => r.SalaryExpectation);
                }
                else if (SortByDate.IsChecked == true)
                {
                    filtered = filtered.OrderByDescending(r => r.CreatedDate);
                }
                else
                {
                    // Сортировка по релевантности (можно добавить свою логику)
                    filtered = filtered.OrderByDescending(r => r.ExperienceYears);
                }

                ResumesListView.ItemsSource = filtered.ToList().Select(r => new
                {
                    r.Id,
                    Position = r.Title,
                    Name = $"{r.Users.LastName} {r.Users.FirstName}",
                    Salary = r.SalaryExpectation.HasValue ? $"{r.SalaryExpectation:N0} руб." : "Не указана",
                    City = r.Cities?.Name ?? "Не указан",
                    EmploymentType = r.EmploymentTypes?.Name ?? "Не указан",
                    Education = r.Educations?.Name ?? "Не указано",
                    Experience = r.ExperienceYears.HasValue ? $"{r.ExperienceYears} лет опыта" : "Без опыта",
                    r.CreatedDate
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
            EducationFilterComboBox.SelectedIndex = -1;

            FullTimeCheckBox.IsChecked = false;
            PartTimeCheckBox.IsChecked = false;
            RemoteCheckBox.IsChecked = false;
            ProjectCheckBox.IsChecked = false;

            NoExperience.IsChecked = false;
            JuniorExperience.IsChecked = false;
            MiddleExperience.IsChecked = false;
            SeniorExperience.IsChecked = false;

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

        private void ResumesButton_Click(object sender, RoutedEventArgs e)
        {
            // Уже на этой странице
        }

        private void MyVacanciesButton_Click(object sender, RoutedEventArgs e)
        {
            /*NavigationService.Navigate(new MyVacanciesPage());*/
        }

        private void ResponsesButton_Click(object sender, RoutedEventArgs e)
        {
            /*NavigationService.Navigate(new EmployerResponses());*/
        }

        private void InterviewsButton_Click(object sender, RoutedEventArgs e)
        {
            /*NavigationService.Navigate(new InterviewsPage());*/
        }

        private void FavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new FavoritesPage());
        }

        private void NotificationsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Notification());
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new UserAccount());
        }
    }
}