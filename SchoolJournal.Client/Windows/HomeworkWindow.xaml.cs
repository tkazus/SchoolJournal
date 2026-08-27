using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SchoolJournal.Client.Windows
{
    public partial class HomeworkWindow : Window
    {
        private int _classId;
        private string _studentName;

        public HomeworkWindow(int classId, string studentName)
        {
            InitializeComponent();
            _classId = classId;
            _studentName = studentName;
            Loaded += HomeworkWindow_Loaded;
        }

        private async void HomeworkWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadHomeworks();
        }

        private async System.Threading.Tasks.Task LoadHomeworks()
        {
            try
            {
                ClassInfoText.Text = $"Класс: {_classId}";

                var response = await TcpClientHelper.SendRequestAsync($"GET_ALL_HOMEWORKS|{_classId}");
                if (response.StartsWith("ERROR"))
                {
                    MessageBox.Show(response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var items = new List<HomeworkDisplayFullItem>();
                foreach (var h in response.Split(';'))
                {
                    var parts = h.Split('|');
                    if (parts.Length >= 6)
                    {
                        DateTime dueDate = DateTime.Parse(parts[4]);
                        bool isOverdue = dueDate < DateTime.Now;

                        items.Add(new HomeworkDisplayFullItem
                        {
                            Subject = parts[1],
                            Description = parts[2],
                            IssueDate = DateTime.Parse(parts[3]),
                            DueDate = dueDate,
                            Teacher = parts[5],
                            DueDateColor = isOverdue ?
                                new SolidColorBrush(Color.FromRgb(213, 168, 168)) :
                                new SolidColorBrush(Color.FromRgb(184, 224, 200)),
                            DueDateTextColor = isOverdue ?
                                new SolidColorBrush(Color.FromRgb(122, 42, 42)) :
                                new SolidColorBrush(Color.FromRgb(42, 106, 74))
                        });
                    }
                }

                HomeworkItemsControl.ItemsSource = items;
                CountText.Text = $"Всего заданий: {items.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ДЗ: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}