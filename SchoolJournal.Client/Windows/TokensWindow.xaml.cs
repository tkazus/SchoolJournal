using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SchoolJournal.Client.Windows
{
    public partial class TokensWindow : Window
    {
        private int _studentId;
        private string _studentName;

        public TokensWindow(int studentId, string studentName)
        {
            InitializeComponent();
            _studentId = studentId;
            _studentName = studentName;
            Loaded += TokensWindow_Loaded;
        }

        private async void TokensWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTokens();
        }

        private async System.Threading.Tasks.Task LoadTokens()
        {
            try
            {
                StudentInfoText.Text = $"Ученик: {_studentName}";

                var statsResponse = await TcpClientHelper.SendRequestAsync($"GET_STATISTICS|{_studentId}");
                if (!statsResponse.StartsWith("ERROR"))
                {
                    var stats = statsResponse.Split('|');
                    if (stats.Length >= 6)
                    {
                        BalanceText.Text = stats[4];
                    }
                }

                var response = await TcpClientHelper.SendRequestAsync($"GET_TOKEN_TRANSACTIONS|{_studentId}");
                if (response.StartsWith("ERROR"))
                {
                    MessageBox.Show(response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var items = new List<TokenTransactionDisplayItem>();
                foreach (var t in response.Split(';'))
                {
                    var parts = t.Split('|');
                    if (parts.Length >= 6)
                    {
                        int amount = int.Parse(parts[1]);
                        bool isPositive = amount > 0;

                        items.Add(new TokenTransactionDisplayItem
                        {
                            Amount = amount,
                            AmountText = isPositive ? $"+{amount}" : amount.ToString(),
                            Type = parts[2],
                            Reason = parts[3],
                            Date = DateTime.Parse(parts[4]),
                            Balance = int.Parse(parts[5]),
                            AmountColor = isPositive ?
                                new SolidColorBrush(Color.FromRgb(184, 224, 200)) :
                                new SolidColorBrush(Color.FromRgb(213, 168, 168)),
                            AmountTextColor = isPositive ?
                                new SolidColorBrush(Color.FromRgb(42, 106, 74)) :
                                new SolidColorBrush(Color.FromRgb(122, 42, 42))
                        });
                    }
                }

                TransactionsItemsControl.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}