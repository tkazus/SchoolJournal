using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SchoolJournal.Client.Windows
{
    public partial class ManageClassesWindow : Window
    {
        public ManageClassesWindow()
        {
            InitializeComponent();
            Loaded += ManageClassesWindow_Loaded;
        }

        private async void ManageClassesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadClasses();
        }

        private async System.Threading.Tasks.Task LoadClasses()
        {
            try
            {
                var response = await TcpClientHelper.SendRequestAsync("GET_CLASSES");
                if (response.StartsWith("ERROR"))
                {
                    MessageBox.Show(response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var items = new List<ClassDisplayItem>();
                foreach (var c in response.Split(';'))
                {
                    var parts = c.Split('|');
                    if (parts.Length >= 3)
                    {
                        items.Add(new ClassDisplayItem
                        {
                            ClassId = int.Parse(parts[0]),
                            ClassName = parts[1],
                            YearOfStudy = int.Parse(parts[2])
                        });
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

        private async void AddClassButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddClassWindow();
            window.Owner = this;
            window.ShowDialog();
            await LoadClasses();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int classId)
            {
                var result = MessageBox.Show("Удалить класс?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var response = await TcpClientHelper.SendRequestAsync($"DELETE_CLASS|{classId}");
                    if (response == "SUCCESS")
                    {
                        MessageBox.Show("Класс удален!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadClasses();
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