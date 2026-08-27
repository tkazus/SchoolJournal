using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace SchoolJournal.Client.Windows
{
    public partial class LoginWindow : Window
    {
        private string _role;
        private string _roleIcon;
        private string _title;
        private string _subtitle;

        public LoginWindow(string role, string roleIcon, string title, string subtitle)
        {
            InitializeComponent();
            _role = role;
            _roleIcon = roleIcon;
            _title = title;
            _subtitle = subtitle;

            Loaded += LoginWindow_Loaded;
        }

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RoleIconText.Text = _roleIcon;
            TitleText.Text = _title;
            SubtitleText.Text = _subtitle;

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            this.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ShowError("Введите логин и пароль");
                return;
            }

            try
            {
                btnLogin.IsEnabled = false;
                txtError.Visibility = Visibility.Collapsed;

                var response = await TcpClientHelper.SendRequestAsync($"LOGIN|{login}|{password}");
                if (response.StartsWith("ERROR"))
                {
                    ShowError(response.Replace("ERROR|", ""));
                    btnLogin.IsEnabled = true;
                    return;
                }

                var parts = response.Split('|');
                if (parts.Length >= 4)
                {
                    int userId = int.Parse(parts[0]);
                    string fullName = parts[1];
                    string roleName = parts[2];
                    bool isActive = bool.Parse(parts[3]);

                    if (!isActive)
                    {
                        ShowError("Пользователь заблокирован!");
                        btnLogin.IsEnabled = true;
                        return;
                    }

                    if (roleName != _role)
                    {
                        ShowError($"Доступ запрещен! Вы вошли как {roleName}");
                        btnLogin.IsEnabled = true;
                        return;
                    }

                    Window mainWindow = null;
                    switch (roleName)
                    {
                        case "Ученик":
                            var studentIdResponse = await TcpClientHelper.SendRequestAsync($"GET_STUDENT_ID|{userId}");
                            if (studentIdResponse != "0")
                            {
                                mainWindow = new StudentWindow(int.Parse(studentIdResponse), fullName);
                            }
                            else
                            {
                                ShowError("Ученик не найден в системе");
                                btnLogin.IsEnabled = true;
                                return;
                            }
                            break;

                        case "Учитель":
                            var teacherIdResponse = await TcpClientHelper.SendRequestAsync($"GET_TEACHER_ID|{userId}");
                            if (teacherIdResponse != "0")
                            {
                                mainWindow = new TeacherWindow(int.Parse(teacherIdResponse), fullName);
                            }
                            else
                            {
                                ShowError("Учитель не найден в системе");
                                btnLogin.IsEnabled = true;
                                return;
                            }
                            break;

                        case "Родитель":
                            mainWindow = new ParentWindow(userId, fullName);
                            break;

                        case "Администратор":
                            mainWindow = new AdminWindow(userId, fullName, roleName);
                            break;

                        default:
                            ShowError($"Неизвестная роль: {roleName}");
                            btnLogin.IsEnabled = true;
                            return;
                    }

                    if (mainWindow != null)
                    {
                        mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        mainWindow.Show();
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
                btnLogin.IsEnabled = true;
            }
        }

        private void ShowError(string message)
        {
            txtError.Text = "❌ " + message;
            txtError.Visibility = Visibility.Visible;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}