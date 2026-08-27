using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SchoolJournal.Client.Windows
{
    public partial class ParentWindow : Window
    {
        private int _parentId;
        private int _childId = 0;
        private int _classId = 0;
        private string _childName = "";
        private string _parentName;

        public ParentWindow(int parentId, string parentName)
        {
            InitializeComponent();
            _parentId = parentId;
            _parentName = parentName;
            Loaded += ParentWindow_Loaded;
        }

        private async void ParentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            try
            {
                var response = await TcpClientHelper.SendRequestAsync($"GET_CHILD|{_parentId}");

                if (response.StartsWith("ERROR"))
                {
                    ParentNameText.Text = "Родитель • Ребенок не найден";

                    var inputDialog = new ChildIdInputDialog();
                    if (inputDialog.ShowDialog() == true)
                    {
                        _childId = inputDialog.ChildId;
                        await LoadChildData();
                    }
                    return;
                }

                var parts = response.Split('|');
                if (parts.Length >= 6)
                {
                    _childId = int.Parse(parts[0]);
                    _childName = parts[1];
                    _classId = int.Parse(parts[3]);
                    await LoadChildData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadChildData()
        {
            try
            {
                if (_childId == 0)
                {
                    ParentNameText.Text = "Родитель • Ребенок не найден";
                    return;
                }

                var childResponse = await TcpClientHelper.SendRequestAsync($"GET_STATISTICS|{_childId}");
                if (!childResponse.StartsWith("ERROR"))
                {
                    var stats = childResponse.Split('|');
                    if (stats.Length >= 6)
                    {
                        _childName = stats[5];
                        ParentNameText.Text = "Родитель";
                        ChildNameText.Text = _childName;
                        ChildAvgGradeText.Text = stats[0];
                        ChildAttendanceText.Text = stats[1] + "%";
                        ChildTokensText.Text = stats[4];
                    }
                }

                var classResponse = await TcpClientHelper.SendRequestAsync($"GET_CLASS_ID|{_childId}");
                if (!classResponse.StartsWith("ERROR") && classResponse != "0")
                {
                    _classId = int.Parse(classResponse);
                    ChildClassText.Text = $"Класс: {_classId}";
                }

                if (_classId > 0)
                {
                    var homeworkResponse = await TcpClientHelper.SendRequestAsync($"GET_HOMEWORKS|{_classId}|3");
                    if (!homeworkResponse.StartsWith("ERROR"))
                    {
                        var homeworkItems = new List<HomeworkDisplayItem>();
                        foreach (var h in homeworkResponse.Split(';'))
                        {
                            var hParts = h.Split('|');
                            if (hParts.Length >= 6)
                            {
                                homeworkItems.Add(new HomeworkDisplayItem
                                {
                                    Subject = hParts[1],
                                    Description = hParts[2],
                                    DueDate = DateTime.Parse(hParts[4])
                                });
                            }
                        }
                        HomeworkItemsControl.ItemsSource = homeworkItems;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных ребенка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void GradesButton_Click(object sender, RoutedEventArgs e)
        {
            if (_childId == 0)
            {
                MessageBox.Show("Ребенок не найден", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new GradesWindow(_childId, _childName);
            window.Owner = this;
            window.ShowDialog();
        }

        private async void ScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            if (_classId == 0)
            {
                MessageBox.Show("Класс не найден", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new ScheduleWindow(_classId, _childName);
            window.Owner = this;
            window.ShowDialog();
        }

        private async void HomeworkButton_Click(object sender, RoutedEventArgs e)
        {
            if (_classId == 0)
            {
                MessageBox.Show("Класс не найден", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new HomeworkWindow(_classId, _childName);
            window.Owner = this;
            window.ShowDialog();
        }

        private async void AttendanceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_childId == 0)
            {
                MessageBox.Show("Ребенок не найден", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new AttendanceWindow(_childId, _childName);
            window.Owner = this;
            window.ShowDialog();
        }

        private async void TokensButton_Click(object sender, RoutedEventArgs e)
        {
            if (_childId == 0)
            {
                MessageBox.Show("Ребенок не найден", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new TokensWindow(_childId, _childName);
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