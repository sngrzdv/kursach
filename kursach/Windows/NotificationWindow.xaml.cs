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
    public partial class NotificationWindow : Window
    {
        private vacancyEntities _db = new vacancyEntities();

        public NotificationWindow()
        {
            InitializeComponent();
            LoadNotifications();
        }

        private void LoadNotifications()
        {
            var notifications = _db.Notifications
                .Where(n => n.UserId == CurrentUser.Id)
                .OrderByDescending(n => n.CreatedDate)
                .Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    CreatedAt = n.CreatedDate,
                    IsRead = n.IsRead,
                    Type = n.NotificationType
                })
                .ToList();

            NotificationsList.ItemsSource = notifications;

            // Помечаем уведомления как прочитанные
            var unreadNotifications = _db.Notifications
                .Where(n => n.UserId == CurrentUser.Id && !n.IsRead)
                .ToList();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            _db.SaveChanges();
        }

        private class NotificationViewModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Message { get; set; }
            public System.DateTime CreatedAt { get; set; }
            public bool IsRead { get; set; }
            public string Type { get; set; }
        }
    }
}