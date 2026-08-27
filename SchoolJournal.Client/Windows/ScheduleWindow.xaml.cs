using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SchoolJournal.Client.Windows
{
    public partial class ScheduleWindow : Window
    {
        private int _classId;
        private string _studentName;
        private string[] _dayNames = { "", "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" };

        public ScheduleWindow(int classId, string studentName)
        {
            InitializeComponent();
            _classId = classId;
            _studentName = studentName;
            Loaded += ScheduleWindow_Loaded;
        }

        private async void ScheduleWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSchedule();
        }

        private async System.Threading.Tasks.Task LoadSchedule()
        {
            try
            {
                ClassInfoText.Text = $"Класс: {_classId}";

                var response = await TcpClientHelper.SendRequestAsync($"GET_WEEKLY_SCHEDULE|{_classId}");
                if (response.StartsWith("ERROR"))
                {
                    MessageBox.Show(response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var items = new List<ScheduleDisplayFullItem>();
                foreach (var s in response.Split(';'))
                {
                    var parts = s.Split('|');
                    if (parts.Length >= 7)
                    {
                        int day = int.Parse(parts[6]);
                        items.Add(new ScheduleDisplayFullItem
                        {
                            DayName = _dayNames[day],
                            Subject = parts[1],
                            Teacher = parts[2],
                            Time = $"{parts[3]}",
                            Room = parts[5] ?? "—"
                        });
                    }
                }

                ScheduleItemsControl.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки расписания: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}