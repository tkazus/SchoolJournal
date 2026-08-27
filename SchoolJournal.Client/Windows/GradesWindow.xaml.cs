using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SchoolJournal.Client.Windows
{
    public partial class GradesWindow : Window
    {
        private int _studentId;
        private string _studentName;

        public GradesWindow(int studentId, string studentName)
        {
            InitializeComponent();
            _studentId = studentId;
            _studentName = studentName;
            Loaded += GradesWindow_Loaded;
        }

        private async void GradesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadGrades();
        }

        private async System.Threading.Tasks.Task LoadGrades()
        {
            try
            {
                StudentInfoText.Text = $"Ученик: {_studentName}";

                var response = await TcpClientHelper.SendRequestAsync($"GET_ALL_GRADES|{_studentId}");
                if (response.StartsWith("ERROR"))
                {
                    MessageBox.Show(response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var items = new List<GradeDisplayFullItem>();
                double sum = 0;
                int count = 0;

                foreach (var g in response.Split(';'))
                {
                    var parts = g.Split('|');
                    if (parts.Length >= 6)
                    {
                        int value = int.Parse(parts[2]);
                        items.Add(new GradeDisplayFullItem
                        {
                            Subject = parts[1],
                            Value = value,
                            Date = DateTime.Parse(parts[3]),
                            Teacher = parts[4],
                            Comment = parts[5] ?? "—",
                            GradeColor = GetGradeColor(value),
                            GradeTextColor = GetGradeTextColor(value)
                        });
                        sum += value;
                        count++;
                    }
                }

                GradesItemsControl.ItemsSource = items;
                double avg = count > 0 ? sum / count : 0;
                SummaryText.Text = $"Средний балл: {avg:F1}  •  Всего оценок: {count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки оценок: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Brush GetGradeColor(int grade)
        {
            switch (grade)
            {
                case 5: return new SolidColorBrush(Color.FromRgb(184, 224, 200));
                case 4: return new SolidColorBrush(Color.FromRgb(232, 213, 160));
                case 3: return new SolidColorBrush(Color.FromRgb(242, 200, 170));
                default: return new SolidColorBrush(Color.FromRgb(240, 200, 200));
            }
        }

        private Brush GetGradeTextColor(int grade)
        {
            switch (grade)
            {
                case 5: return new SolidColorBrush(Color.FromRgb(42, 106, 74));
                case 4: return new SolidColorBrush(Color.FromRgb(122, 106, 42));
                case 3: return new SolidColorBrush(Color.FromRgb(160, 120, 60));
                default: return new SolidColorBrush(Color.FromRgb(160, 60, 60));
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}