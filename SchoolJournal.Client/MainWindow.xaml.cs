using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace SchoolJournal.Client
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                await CheckServerConnection();
                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.5)
                };
                this.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };
        }

        private async Task CheckServerConnection()
        {
            try
            {
                var response = await TcpClientHelper.SendRequestAsync("PING");
                if (response == "PONG")
                {
                    ServerStatusText.Text = " Подключено";
                    ServerStatusText.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    ServerStatusText.Text = " Ошибка соединения";
                    ServerStatusText.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch
            {
                ServerStatusText.Text = " Сервер не доступен";
                ServerStatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void StudentButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new Windows.LoginWindow("Ученик", "🎒", "", "Вход для ученика");
            loginWindow.ShowDialog();
        }

        private void TeacherButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new Windows.LoginWindow("Учитель", "👩‍🏫", "", "Вход для учителя");
            loginWindow.ShowDialog();
        }

        private void ParentButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new Windows.LoginWindow("Родитель", "👨‍👩‍👦", "", "Вход для родителя");
            loginWindow.ShowDialog();
        }

        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new Windows.LoginWindow("Администратор", "⚙️", "", "Вход для администратора");
            loginWindow.ShowDialog();
        }
    }
}