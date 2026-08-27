using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SchoolJournal.Client.Windows
{
    public partial class ManageSubjectsWindow : Window
    {
        public ManageSubjectsWindow()
        {
            InitializeComponent();
            Loaded += ManageSubjectsWindow_Loaded;
        }

        private async void ManageSubjectsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSubjects();
        }

        private async System.Threading.Tasks.Task LoadSubjects()
        {
            try
            {
                var response = await TcpClientHelper.SendRequestAsync("GET_SUBJECTS");
                if (response.StartsWith("ERROR"))
                {
                    MessageBox.Show(response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var items = new List<SubjectAdminItem>();
                foreach (var s in response.Split(';'))
                {
                    var parts = s.Split('|');
                    if (parts.Length >= 2)
                    {
                        items.Add(new SubjectAdminItem
                        {
                            SubjectId = int.Parse(parts[0]),
                            SubjectName = parts[1]
                        });
                    }
                }
                SubjectsItemsControl.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки предметов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddSubjectButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция добавления предмета в разработке", "Информация");
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int subjectId)
            {
                var result = MessageBox.Show("Удалить предмет?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var response = await TcpClientHelper.SendRequestAsync($"DELETE_SUBJECT|{subjectId}");
                    if (response == "SUCCESS")
                    {
                        MessageBox.Show("Предмет удален!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadSubjects();
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