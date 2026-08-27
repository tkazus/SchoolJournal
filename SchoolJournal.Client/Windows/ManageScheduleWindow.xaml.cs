using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SchoolJournal.Client.Windows
{
    public partial class ManageScheduleWindow : Window
    {
        public ManageScheduleWindow()
        {
            InitializeComponent();
            Loaded += ManageScheduleWindow_Loaded;
        }

        private async void ManageScheduleWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSchedule();
        }

        private async System.Threading.Tasks.Task LoadSchedule()
        {
            try
            {
                var response = await TcpClientHelper.SendRequestAsync("GET_SCHEDULE");
                if (response.StartsWith("ERROR"))
                {
                    MessageBox.Show(response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var days = new[] { "", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
                var items = new List<ScheduleAdminItem>();

                foreach (var s in response.Split(';'))
                {
                    var parts = s.Split('|');
                    if (parts.Length >= 8)
                    {
                        int day = int.Parse(parts[4]);
                        items.Add(new ScheduleAdminItem
                        {
                            ScheduleId = int.Parse(parts[0]),
                            ClassName = parts[1],
                            SubjectName = parts[2],
                            TeacherName = parts[3],
                            DayName = days[day],
                            Time = $"{parts[5]}"
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

        private async void AddScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция добавления занятия в разработке", "Информация");
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int scheduleId)
            {
                var result = MessageBox.Show("Удалить занятие?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var response = await TcpClientHelper.SendRequestAsync($"DELETE_SCHEDULE|{scheduleId}");
                    if (response == "SUCCESS")
                    {
                        MessageBox.Show("Занятие удалено!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadSchedule();
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка: {response}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}