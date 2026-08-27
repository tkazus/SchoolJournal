using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SchoolJournal.Client.Windows
{
    public partial class AddHomeworkWindow : Window
    {
        private int _teacherId;
        private int _classId;
        private List<SubjectDisplayItem> _subjects = new List<SubjectDisplayItem>();

        public AddHomeworkWindow(int teacherId, int classId)
        {
            InitializeComponent();
            _teacherId = teacherId;
            _classId = classId;
            Loaded += AddHomeworkWindow_Loaded;
        }

        private async void AddHomeworkWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSubjects();
            DueDatePicker.SelectedDate = DateTime.Now.AddDays(7);
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

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (SubjectCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите предмет!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(DescriptionBox.Text))
            {
                MessageBox.Show("Введите описание задания!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DueDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату сдачи!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int subjectId = (int)SubjectCombo.SelectedValue;
                string description = DescriptionBox.Text.Trim();
                DateTime dueDate = DueDatePicker.SelectedDate.Value;

                string request = $"ADD_HOMEWORK|{subjectId}|{_teacherId}|{_classId}|{description}|{dueDate:yyyy-MM-dd}";
                var response = await TcpClientHelper.SendRequestAsync(request);

                if (response == "SUCCESS")
                {
                    MessageBox.Show(" Домашнее задание добавлено!", "Успех",
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