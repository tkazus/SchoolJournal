using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SchoolJournal.Client.Windows
{
    public partial class ManageTeachersWindow : Window
    {
        public ManageTeachersWindow()
        {
            InitializeComponent();
            Loaded += ManageTeachersWindow_Loaded;
        }

        private async void ManageTeachersWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTeachers();
        }

        private async System.Threading.Tasks.Task LoadTeachers()
        {
            try
            {
                var response = await TcpClientHelper.SendRequestAsync("GET_TEACHERS_LIST");
                if (response.StartsWith("ERROR"))
                {
                    MessageBox.Show(response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var items = new List<TeacherAdminItem>();
                foreach (var t in response.Split(';'))
                {
                    var parts = t.Split('|');
                    if (parts.Length >= 4)
                    {
                        items.Add(new TeacherAdminItem
                        {
                            TeacherId = int.Parse(parts[0]),
                            FullName = parts[1],
                            Specialty = parts[2],
                            Login = parts[3]
                        });
                    }
                }
                TeachersItemsControl.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки учителей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddTeacherButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция добавления учителя в разработке", "Информация");
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int teacherId)
            {
                var result = MessageBox.Show("Удалить учителя?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var response = await TcpClientHelper.SendRequestAsync($"DELETE_USER|{teacherId}");
                    if (response == "SUCCESS")
                    {
                        MessageBox.Show("Учитель удален!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadTeachers();
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