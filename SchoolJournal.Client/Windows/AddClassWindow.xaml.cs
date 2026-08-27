using System;
using System.Windows;

namespace SchoolJournal.Client.Windows
{
    public partial class AddClassWindow : Window
    {
        public AddClassWindow()
        {
            InitializeComponent();
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string className = ClassNameBox.Text.Trim();
            if (string.IsNullOrEmpty(className))
            {
                MessageBox.Show("Введите название класса!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(YearBox.Text, out int year))
            {
                MessageBox.Show("Введите корректный год!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var response = await TcpClientHelper.SendRequestAsync($"CREATE_CLASS|{className}|{year}");
                if (response.StartsWith("SUCCESS"))
                {
                    MessageBox.Show("Класс добавлен!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"Ошибка: {response}", "Ошибка",
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