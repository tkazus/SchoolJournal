using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SchoolJournal.Client.Windows
{
    public partial class TeacherWindow : Window
    {
        private int _teacherId;
        private int _selectedClassId = 0;
        private string _teacherName;
        private string _selectedClassName = "";
        private List<StudentDisplayItem> _students = new List<StudentDisplayItem>();

        public TeacherWindow(int teacherId, string teacherName)
        {
            InitializeComponent();
            _teacherId = teacherId;
            _teacherName = teacherName;
            Loaded += TeacherWindow_Loaded;
        }

        private async void TeacherWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            try
            {
                var classesResponse = await TcpClientHelper.SendRequestAsync($"GET_TEACHER_CLASSES|{_teacherId}");
                if (!classesResponse.StartsWith("ERROR"))
                {
                    var classes = classesResponse.Split(';');
                    if (classes.Length > 0)
                    {
                        var classParts = classes[0].Split('|');
                        if (classParts.Length >= 4)
                        {
                            _selectedClassId = int.Parse(classParts[0]);
                            _selectedClassName = classParts[1];
                            int studentCount = int.Parse(classParts[3]);

                            TeacherNameText.Text = $"Учитель: {_teacherName}";
                            ClassInfoText.Text = $"Класс: {_selectedClassName}";
                            StudentsCountText.Text = studentCount.ToString();

                            await LoadStudents(_selectedClassId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadStudents(int classId)
        {
            try
            {
                var response = await TcpClientHelper.SendRequestAsync($"GET_STUDENTS_BY_CLASS|{classId}|{_teacherId}");
                if (!response.StartsWith("ERROR"))
                {
                    _students.Clear();
                    double totalAvg = 0;
                    int successCount = 0;

                    foreach (var s in response.Split(';'))
                    {
                        var parts = s.Split('|');
                        if (parts.Length >= 5)
                        {
                            double avg = double.Parse(parts[2]);
                            double attendance = double.Parse(parts[3]);

                            _students.Add(new StudentDisplayItem
                            {
                                StudentId = int.Parse(parts[0]),
                                Name = parts[1],
                                AverageGrade = avg,
                                Attendance = attendance,
                                Tokens = int.Parse(parts[4])
                            });

                            totalAvg += avg;
                            if (avg >= 3) successCount++;
                        }
                    }

                    StudentsItemsControl.ItemsSource = _students;

                    if (_students.Count > 0)
                    {
                        AvgGradeText.Text = (totalAvg / _students.Count).ToString("F1");
                        SuccessRateText.Text = ((double)successCount / _students.Count * 100).ToString("F0") + "%";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки учеников: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ClassesButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new MyClassesWindow(_teacherId, _teacherName);
            window.Owner = this;
            window.ShowDialog();
        }

        private async void AddGradeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_students.Count == 0)
            {
                MessageBox.Show("Нет учеников в классе", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new AddGradeWindow(_teacherId, _students);
            window.Owner = this;
            window.ShowDialog();
            await LoadData();
        }

        private async void AddHomeworkButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClassId == 0)
            {
                MessageBox.Show("Класс не выбран", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new AddHomeworkWindow(_teacherId, _selectedClassId);
            window.Owner = this;
            window.ShowDialog();
        }

        private async void AttendanceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_students.Count == 0)
            {
                MessageBox.Show("Нет учеников в классе", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new MarkAttendanceWindow(_teacherId, _students);
            window.Owner = this;
            window.ShowDialog();
        }

        private async void AddTokensButton_Click(object sender, RoutedEventArgs e)
        {
            if (_students.Count == 0)
            {
                MessageBox.Show("Нет учеников в классе", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new AddTokensWindow(_students);
            window.Owner = this;
            window.ShowDialog();
            await LoadData();
        }

        private async void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClassId == 0)
            {
                MessageBox.Show("Класс не выбран", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new StatisticsWindow(_teacherId, _selectedClassId, _selectedClassName);
            window.Owner = this;
            window.ShowDialog();
        }

        private async void StudentStatsButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int studentId)
            {
                var student = _students.FirstOrDefault(s => s.StudentId == studentId);
                if (student != null)
                {
                    var gradesResponse = await TcpClientHelper.SendRequestAsync($"GET_ALL_GRADES|{studentId}");
                    string gradesInfo = "";
                    if (!gradesResponse.StartsWith("ERROR"))
                    {
                        var grades = gradesResponse.Split(';');
                        foreach (var g in grades)
                        {
                            var parts = g.Split('|');
                            if (parts.Length >= 6)
                            {
                                gradesInfo += $"\n  {parts[1]}: {parts[2]} ({parts[3]})";
                            }
                        }
                    }

                    MessageBox.Show($"📊 Статистика ученика\n\n" +
                        $"ФИО: {student.Name}\n" +
                        $"Средний балл: {student.AverageGrade:F1}\n" +
                        $"Посещаемость: {student.Attendance:F0}%\n" +
                        $"Баланс жетонов: {student.Tokens}\n" +
                        $"\n📝 Оценки:{gradesInfo}\n",
                        "Статистика", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
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