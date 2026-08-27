using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SchoolJournal.Client.Windows
{
    public partial class AddTokensWindow : Window
    {
        private List<StudentDisplayItem> _students;

        public AddTokensWindow(List<StudentDisplayItem> students)
        {
            InitializeComponent();
            _students = students;
            Loaded += AddTokensWindow_Loaded;
        }

        private void AddTokensWindow_Loaded(object sender, RoutedEventArgs e)
        {
            StudentCombo.ItemsSource = _students;
            StudentCombo.DisplayMemberPath = "Name";
            StudentCombo.SelectedValuePath = "StudentId";
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (StudentCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите ученика!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(AmountBox.Text, out int amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректное количество жетонов!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int studentId = (int)StudentCombo.SelectedValue;
                string reason = ReasonBox.Text.Trim();
                if (string.IsNullOrEmpty(reason))
                {
                    reason = "Начисление жетонов";
                }

                string request = $"ADD_TOKENS|{studentId}|{amount}|{reason}";
                var response = await TcpClientHelper.SendRequestAsync(request);

                if (response.StartsWith("SUCCESS"))
                {
                    var parts = response.Split('|');
                    MessageBox.Show($" Начислено {amount} жетонов!\n\nНовый баланс: {parts[1]} 🪙",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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