using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SchoolJournal.Client.Windows
{
    public partial class MyClassesWindow : Window
    {
        private int _teacherId;

        public MyClassesWindow(int teacherId, string teacherName)
        {
            InitializeComponent();
            _teacherId = teacherId;
            TeacherNameText.Text = $"Учитель: {teacherName}";
            Loaded += MyClassesWindow_Loaded;
        }

        private async void MyClassesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadClasses();
        }

        private async System.Threading.Tasks.Task LoadClasses()
        {
            try
            {
                var response = await TcpClientHelper.SendRequestAsync($"GET_TEACHER_CLASSES|{_teacherId}");
                if (response.StartsWith("ERROR"))
                {
                    MessageBox.Show(response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var items = new List<ClassDisplayItem>();
                foreach (var c in response.Split(';'))
                {
                    var parts = c.Split('|');
                    if (parts.Length >= 4)
                    {
                        items.Add(new ClassDisplayItem
                        {
                            ClassId = int.Parse(parts[0]),
                            ClassName = parts[1],
                            YearOfStudy = int.Parse(parts[2]),
                            StudentCount = int.Parse(parts[3]),
                            AverageGrade = 0 
                        });
                    }
                }

                foreach (var cls in items)
                {
                    var studentsResponse = await TcpClientHelper.SendRequestAsync($"GET_STUDENTS_BY_CLASS|{cls.ClassId}|{_teacherId}");
                    if (!studentsResponse.StartsWith("ERROR"))
                    {
                        double totalAvg = 0;
                        int count = 0;
                        foreach (var s in studentsResponse.Split(';'))
                        {
                            var parts = s.Split('|');
                            if (parts.Length >= 5)
                            {
                                totalAvg += double.Parse(parts[2]);
                                count++;
                            }
                        }
                        cls.AverageGrade = count > 0 ? totalAvg / count : 0;
                    }
                }

                ClassesItemsControl.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки классов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}