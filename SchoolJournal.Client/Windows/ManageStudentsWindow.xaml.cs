using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SchoolJournal.Client.Windows
{
    public partial class ManageStudentsWindow : Window
    {
        public ManageStudentsWindow()
        {
            InitializeComponent();
            Loaded += ManageStudentsWindow_Loaded;
        }

        private async void ManageStudentsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadStudents();
        }

        private async System.Threading.Tasks.Task LoadStudents()
        {
            try
            {
                var response = await TcpClientHelper.SendRequestAsync("GET_STUDENTS_LIST");
                if (response.StartsWith("ERROR"))
                {
                    MessageBox.Show(response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var items = new List<StudentAdminItem>();
                foreach (var s in response.Split(';'))
                {
                    var parts = s.Split('|');
                    if (parts.Length >= 5)
                    {
                        items.Add(new StudentAdminItem
                        {
                            StudentId = int.Parse(parts[0]),
                            FullName = parts[1],
                            ClassName = parts[2],
                            BirthDate = parts[3],
                            Login = parts[4]
                        });
                    }
                }
                StudentsItemsControl.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки учеников: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddStudentButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция добавления ученика в разработке", "Информация");
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int studentId)
            {
                var result = MessageBox.Show("Удалить ученика?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var response = await TcpClientHelper.SendRequestAsync($"DELETE_USER|{studentId}");
                    if (response == "SUCCESS")
                    {
                        MessageBox.Show("Ученик удален!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadStudents();
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