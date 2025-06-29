using System;
using System.Collections.Generic;
using System.Globalization;
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
    public partial class ScheduleInterviewDialog : Window
    {
        public int SelectedResponseId { get; private set; }
        public DateTime InterviewDate { get; private set; }
        public string Location { get; private set; }
        public string OnlineMeetingLink { get; private set; }
        public string Notes { get; private set; }
        public bool IsOnline { get; private set; }

        public ScheduleInterviewDialog(dynamic availableResponses)
        {
            InitializeComponent();

            ResponseComboBox.ItemsSource = availableResponses;
            ResponseComboBox.DisplayMemberPath = "DisplayText";
            ResponseComboBox.SelectedValuePath = "Id";

            // Устанавливаем значения по умолчанию
            DatePicker.SelectedDate = DateTime.Today.AddDays(1);
            TimeTextBox.Text = "10:00";
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация выбора отклика
            if (ResponseComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите отклик для собеседования", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Валидация даты
            if (DatePicker.SelectedDate == null)
            {
                MessageBox.Show("Укажите дату собеседования", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Валидация времени
            DateTime time;
            if (!DateTime.TryParseExact(TimeTextBox.Text, "HH:mm", CultureInfo.InvariantCulture,
                                      DateTimeStyles.None, out time))
            {
                MessageBox.Show("Введите время в формате ЧЧ:MM (например, 14:30)", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Комбинируем дату и время
            InterviewDate = DatePicker.SelectedDate.Value.Date.Add(time.TimeOfDay);

            if (InterviewDate < DateTime.Now)
            {
                MessageBox.Show("Дата собеседования не может быть в прошлом", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedResponseId = (int)ResponseComboBox.SelectedValue;
            IsOnline = OnlineRadio.IsChecked == true;

            if (IsOnline)
            {
                if (string.IsNullOrWhiteSpace(MeetingLinkTextBox.Text))
                {
                    MessageBox.Show("Укажите ссылку на онлайн-встречу", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                OnlineMeetingLink = MeetingLinkTextBox.Text;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(LocationTextBox.Text))
                {
                    MessageBox.Show("Укажите место проведения собеседования", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                Location = LocationTextBox.Text;
            }

            Notes = NotesTextBox.Text;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnlineRadio_Checked(object sender, RoutedEventArgs e)
        {
            OnlinePanel.Visibility = Visibility.Visible;
            OfflinePanel.Visibility = Visibility.Collapsed;
        }

        private void OfflineRadio_Checked(object sender, RoutedEventArgs e)
        {
            OnlinePanel.Visibility = Visibility.Collapsed;
            OfflinePanel.Visibility = Visibility.Visible;
        }
    }
}