using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SchoolJournal.Client.Windows
{
    public partial class StudentWindow : Window
    {
        private int _studentId;
        private int _classId = 1;
        private string _studentName;

        public StudentWindow(int studentId, string studentName)
        {
            InitializeComponent();
            _studentId = studentId;
            _studentName = studentName;
            Loaded += StudentWindow_Loaded;
        }

        private async void StudentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            try
            {
                var statsResponse = await TcpClientHelper.SendRequestAsync($"GET_STATISTICS|{_studentId}");
                if (!statsResponse.StartsWith("ERROR"))
                {
                    var stats = statsResponse.Split('|');
                    if (stats.Length >= 6)
                    {
                        _studentName = stats[5];
                        StudentNameText.Text = $"Ученик: {_studentName}";
                        AvgGradeText.Text = stats[0];
                        AttendanceText.Text = stats[1] + "%";
                        HomeworkCountText.Text = stats[2];
                        TokensText.Text = stats[4];
                    }
                }

                var classResponse = await TcpClientHelper.SendRequestAsync($"GET_CLASS_ID|{_studentId}");
                if (!classResponse.StartsWith("ERROR") && classResponse != "0")
                {
                    _classId = int.Parse(classResponse);
                    ClassInfoText.Text = $"Класс: {_classId}";
                }

                var gradesResponse = await TcpClientHelper.SendRequestAsync($"GET_RECENT_GRADES|{_studentId}|5");
                if (!gradesResponse.StartsWith("ERROR"))
                {
                    var gradeItems = new List<GradeDisplayItem>();
                    foreach (var g in gradesResponse.Split(';'))
                    {
                        var parts = g.Split('|');
                        if (parts.Length >= 6)
                        {
                            int value = int.Parse(parts[2]);
                            gradeItems.Add(new GradeDisplayItem
                            {
                                Subject = parts[1],
                                Value = value,
                                Date = DateTime.Parse(parts[3]),
                                Teacher = parts[4],
                                GradeColor = GetGradeColor(value),
                                GradeTextColor = GetGradeTextColor(value)
                            });
                        }
                    }
                    GradesItemsControl.ItemsSource = gradeItems;
                }

                if (_classId > 0)
                {
                    var scheduleResponse = await TcpClientHelper.SendRequestAsync($"GET_TODAY_SCHEDULE|{_classId}");
                    if (!scheduleResponse.StartsWith("ERROR"))
                    {
                        var scheduleItems = new List<ScheduleDisplayItem>();
                        foreach (var s in scheduleResponse.Split(';'))
                        {
                            var parts = s.Split('|');
                            if (parts.Length >= 6)
                            {
                                scheduleItems.Add(new ScheduleDisplayItem
                                {
                                    Subject = parts[1],
                                    Teacher = parts[2],
                                    Time = $"{parts[3]}",
                                    Room = parts[5] ?? "—"
                                });
                            }
                        }
                        ScheduleItemsControl.ItemsSource = scheduleItems;
                    }

                    var homeworkResponse = await TcpClientHelper.SendRequestAsync($"GET_HOMEWORKS|{_classId}|3");
                    if (!homeworkResponse.StartsWith("ERROR"))
                    {
                        var homeworkItems = new List<HomeworkDisplayItem>();
                        foreach (var h in homeworkResponse.Split(';'))
                        {
                            var parts = h.Split('|');
                            if (parts.Length >= 6)
                            {
                                homeworkItems.Add(new HomeworkDisplayItem
                                {
                                    Subject = parts[1],
                                    Description = parts[2],
                                    DueDate = DateTime.Parse(parts[4])
                                });
                            }
                        }
                        HomeworkItemsControl.ItemsSource = homeworkItems;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
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

        private async void GradesButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new GradesWindow(_studentId, _studentName);
            window.Owner = this;
            window.ShowDialog();
        }

        private async void ScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new ScheduleWindow(_classId, _studentName);
            window.Owner = this;
            window.ShowDialog();
        }

        private async void HomeworkButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new HomeworkWindow(_classId, _studentName);
            window.Owner = this;
            window.ShowDialog();
        }

        private async void AttendanceButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AttendanceWindow(_studentId, _studentName);
            window.Owner = this;
            window.ShowDialog();
        }

        private async void TokensButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new TokensWindow(_studentId, _studentName);
            window.Owner = this;
            window.ShowDialog();
            await LoadData();
        }

        private async void ShopButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new ShopWindow(_studentId, _studentName);
            window.Owner = this;
            window.ShowDialog();
            await LoadData();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
        }
    }
}