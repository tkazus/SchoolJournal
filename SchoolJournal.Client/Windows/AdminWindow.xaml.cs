using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SchoolJournal.Client.Windows
{
    public partial class AdminWindow : Window
    {
        private int _userId;
        private string _userName;
        private string _roleName;
        private List<UserDisplayItem> _users = new List<UserDisplayItem>();

        public AdminWindow(int userId, string userName, string roleName)
        {
            InitializeComponent();
            _userId = userId;
            _userName = userName;
            _roleName = roleName;
            Loaded += AdminWindow_Loaded;
        }

        private async void AdminWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AdminNameText.Text = $"{_roleName} • {_userName}";
            await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            try
            {
                var response = await TcpClientHelper.SendRequestAsync("GET_USERS");
                if (response.StartsWith("ERROR"))
                {
                    MessageBox.Show(response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _users.Clear();
                foreach (var u in response.Split(';'))
                {
                    var parts = u.Split('|');
                    if (parts.Length >= 7)
                    {
                        _users.Add(new UserDisplayItem
                        {
                            Id = int.Parse(parts[0]),
                            Login = parts[1],
                            FullName = parts[2],
                            Role = parts[3],
                            IsActive = bool.Parse(parts[4]),
                            Class = parts[5],
                            Specialty = parts[6]
                        });
                    }
                }
                DataItemsControl.ItemsSource = _users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async void ClassesButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new ManageClassesWindow();
            window.Owner = this;
            window.ShowDialog();
            await LoadData();
        }

        private async void StudentsButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new ManageStudentsWindow();
            window.Owner = this;
            window.ShowDialog();
            await LoadData();
        }

        private async void TeachersButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new ManageTeachersWindow();
            window.Owner = this;
            window.ShowDialog();
            await LoadData();
        }

        private async void SubjectsButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new ManageSubjectsWindow();
            window.Owner = this;
            window.ShowDialog();
            await LoadData();
        }

        private async void ScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new ManageScheduleWindow();
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