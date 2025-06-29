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
using System.Windows.Shapes;

namespace kursach.Windows
{
    public partial class SelectVacancyDialog : Window
    {
        private readonly vacancyEntities db = new vacancyEntities();
        public Vacancies SelectedVacancy { get; private set; }

        public SelectVacancyDialog()
        {
            InitializeComponent();
            LoadVacancies();
        }

        private void LoadVacancies()
        {
            try
            {
                // Загружаем вакансии текущего пользователя (или все активные вакансии)
                var vacancies = db.Vacancies
                    .Where(v => v.IsActive) // Пример фильтрации активных вакансий
                    .OrderBy(v => v.Title)
                    .ToList();

                VacanciesComboBox.ItemsSource = vacancies;
                VacanciesComboBox.DisplayMemberPath = "Title"; // Отображаем название вакансии
                VacanciesComboBox.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки вакансий: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (VacanciesComboBox.SelectedItem is Vacancies selectedVacancy)
            {
                SelectedVacancy = selectedVacancy;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите вакансию", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}