using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SchoolJournal.Client.Windows
{
    public partial class MarkAttendanceWindow : Window
    {
        private int _teacherId;
        private List<StudentDisplayItem> _students;
        private List<SubjectDisplayItem> _subjects = new List<SubjectDisplayItem>();

        public MarkAttendanceWindow(int teacherId, List<StudentDisplayItem> students)
        {
            InitializeComponent();
            _teacherId = teacherId;
            _students = students;
            Loaded += MarkAttendanceWindow_Loaded;
        }

        private async void MarkAttendanceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSubjects();
            StudentsItemsControl.ItemsSource = _students;
        }

        private async System.Threading.Tasks.Task LoadSubjects()
        {
            try
            {
                var response = await TcpClientHelper.SendRequestAsync($"GET_TEACHER_SUBJECTS|{_teacherId}");
                if (!response.StartsWith("ERROR"))
                {
                    _subjects.Clear();
                    foreach (var s in response.Split(';'))
                    {
                        var parts = s.Split('|');
                        if (parts.Length >= 2)
                        {
                            _subjects.Add(new SubjectDisplayItem
                            {
                                SubjectId = int.Parse(parts[0]),
                                SubjectName = parts[1]
                            });
                        }
                    }
                    SubjectCombo.ItemsSource = _subjects;
                    SubjectCombo.DisplayMemberPath = "SubjectName";
                    SubjectCombo.SelectedValuePath = "SubjectId";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки предметов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SubjectCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите предмет!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int subjectId = (int)SubjectCombo.SelectedValue;

                var items = StudentsItemsControl.ItemsSource as List<StudentDisplayItem>;
                if (items == null) return;

                foreach (var student in items)
                {
                    var container = StudentsItemsControl.ItemContainerGenerator.ContainerFromItem(student) as ContentPresenter;
                    if (container != null)
                    {
                        var statusCombo = FindVisualChild<ComboBox>(container, "StatusCombo");
                        if (statusCombo != null)
                        {
                            string status = (statusCombo.SelectedItem as ComboBoxItem)?.Content.ToString() ?? " Присутствовал";
                            status = status.Replace("✅ ", "").Replace("❌ ", "").Replace("⚕️ ", "");

                            string request = $"ADD_ATTENDANCE|{student.StudentId}|{subjectId}|{status}";
                            await TcpClientHelper.SendRequestAsync(request);
                        }
                    }
                }

                MessageBox.Show(" Посещаемость сохранена!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private T FindVisualChild<T>(DependencyObject parent, string childName) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T && (child as FrameworkElement).Name == childName)
                    return child as T;
                var result = FindVisualChild<T>(child, childName);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}