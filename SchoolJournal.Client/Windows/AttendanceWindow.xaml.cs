using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SchoolJournal.Client.Windows
{
    public partial class AttendanceWindow : Window
    {
        private int _studentId;
        private string _studentName;

        public AttendanceWindow(int studentId, string studentName)
        {
            InitializeComponent();
            _studentId = studentId;
            _studentName = studentName;
            Loaded += AttendanceWindow_Loaded;
        }

        private async void AttendanceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAttendance();
        }

        private async System.Threading.Tasks.Task LoadAttendance()
        {
            try
            {
                StudentInfoText.Text = $"Ученик: {_studentName}";

                var response = await TcpClientHelper.SendRequestAsync($"GET_ATTENDANCE|{_studentId}");
                if (response.StartsWith("ERROR"))
                {
                    MessageBox.Show(response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var items = new List<AttendanceDisplayFullItem>();
                int present = 0, excused = 0, absent = 0;

                foreach (var a in response.Split(';'))
                {
                    var parts = a.Split('|');
                    if (parts.Length >= 5)
                    {
                        string status = parts[3];
                        var item = new AttendanceDisplayFullItem
                        {
                            Subject = parts[1],
                            Date = DateTime.Parse(parts[2]),
                            Status = status,
                            Comment = parts[4] ?? "—"
                        };

                        switch (status)
                        {
                            case "Присутствовал":
                                item.StatusColor = new SolidColorBrush(Color.FromRgb(184, 224, 200));
                                item.StatusTextColor = new SolidColorBrush(Color.FromRgb(42, 106, 74));
                                present++;
                                break;
                            case "Уважительная причина":
                                item.StatusColor = new SolidColorBrush(Color.FromRgb(232, 213, 160));
                                item.StatusTextColor = new SolidColorBrush(Color.FromRgb(122, 106, 42));
                                excused++;
                                break;
                            default:
                                item.StatusColor = new SolidColorBrush(Color.FromRgb(213, 168, 168));
                                item.StatusTextColor = new SolidColorBrush(Color.FromRgb(122, 42, 42));
                                absent++;
                                break;
                        }

                        items.Add(item);
                    }
                }

                AttendanceItemsControl.ItemsSource = items;
                PresentCount.Text = $" Присутствовал: {present}";
                ExcusedCount.Text = $" Уважительная: {excused}";
                AbsentCount.Text = $" Отсутствовал: {absent}";
                TotalCount.Text = $" Всего: {items.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки посещаемости: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}