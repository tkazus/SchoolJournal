using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SchoolJournal.Client.Windows
{
    public partial class StatisticsWindow : Window
    {
        private int _teacherId;
        private int _classId;
        private string _className;
        private List<StudentStatItem> _students = new List<StudentStatItem>();

        public StatisticsWindow(int teacherId, int classId, string className)
        {
            InitializeComponent();
            _teacherId = teacherId;
            _classId = classId;
            _className = className;
            ClassInfoText.Text = $"Класс: {_className}";
            Loaded += StatisticsWindow_Loaded;
        }

        private async void StatisticsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadStatistics();
        }

        private async System.Threading.Tasks.Task LoadStatistics()
        {
            try
            {
                var response = await TcpClientHelper.SendRequestAsync($"GET_STUDENTS_BY_CLASS|{_classId}|{_teacherId}");
                if (response.StartsWith("ERROR"))
                {
                    MessageBox.Show(response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _students.Clear();
                double totalAvg = 0;
                int successCount = 0;
                int excellentCount = 0;

                foreach (var s in response.Split(';'))
                {
                    var parts = s.Split('|');
                    if (parts.Length >= 5)
                    {
                        double avg = double.Parse(parts[2]);
                        double attendance = double.Parse(parts[3]);
                        int tokens = int.Parse(parts[4]);

                        var student = new StudentStatItem
                        {
                            StudentId = int.Parse(parts[0]),
                            Name = parts[1],
                            AverageGrade = avg,
                            Attendance = attendance,
                            Tokens = tokens,
                            RecentGrades = new List<GradeStatItem>()
                        };

                        var gradesResponse = await TcpClientHelper.SendRequestAsync($"GET_RECENT_GRADES|{student.StudentId}|3");
                        if (!gradesResponse.StartsWith("ERROR"))
                        {
                            foreach (var g in gradesResponse.Split(';'))
                            {
                                var gParts = g.Split('|');
                                if (gParts.Length >= 6)
                                {
                                    int value = int.Parse(gParts[2]);
                                    student.RecentGrades.Add(new GradeStatItem
                                    {
                                        Value = value,
                                        GradeColor = GetGradeColor(value),
                                        GradeTextColor = GetGradeTextColor(value)
                                    });
                                }
                            }
                        }

                        _students.Add(student);
                        totalAvg += avg;
                        if (avg >= 3) successCount++;
                        if (avg >= 4.5) excellentCount++;
                    }
                }

                TotalStudentsText.Text = _students.Count.ToString();
                AvgGradeText.Text = _students.Count > 0 ? (totalAvg / _students.Count).ToString("F1") : "0.0";
                SuccessRateText.Text = _students.Count > 0 ? ((double)successCount / _students.Count * 100).ToString("F0") + "%" : "0%";
                ExcellentStudentsText.Text = excellentCount.ToString();

                StudentsItemsControl.ItemsSource = _students;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка",
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