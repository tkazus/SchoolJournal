using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SchoolJournal.Client.Windows
{
    public partial class AddGradeWindow : Window
    {
        private int _teacherId;
        private List<StudentDisplayItem> _students;
        private List<SubjectDisplayItem> _subjects = new List<SubjectDisplayItem>();

        public AddGradeWindow(int teacherId, List<StudentDisplayItem> students)
        {
            InitializeComponent();
            _teacherId = teacherId;
            _students = students;
            Loaded += AddGradeWindow_Loaded;
        }

        private async void AddGradeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSubjects();
            LoadStudents();
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

        private void LoadStudents()
        {
            StudentCombo.ItemsSource = _students;
            StudentCombo.DisplayMemberPath = "Name";
            StudentCombo.SelectedValuePath = "StudentId";
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (StudentCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите ученика!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SubjectCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите предмет!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (GradeCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите оценку!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int studentId = (int)StudentCombo.SelectedValue;
                int subjectId = (int)SubjectCombo.SelectedValue;
                int gradeValue = int.Parse((GradeCombo.SelectedItem as ComboBoxItem).Content.ToString());
                string comment = CommentBox.Text.Trim();

                string request = $"ADD_GRADE|{studentId}|{_teacherId}|{subjectId}|{gradeValue}|{comment}";
                var response = await TcpClientHelper.SendRequestAsync(request);

                if (response == "SUCCESS")
                {
                    MessageBox.Show(" Оценка успешно сохранена!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show($" Ошибка: {response}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}