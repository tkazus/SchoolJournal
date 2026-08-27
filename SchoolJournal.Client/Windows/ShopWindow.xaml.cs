using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SchoolJournal.Client.Windows
{
    public partial class ShopWindow : Window
    {
        private int _studentId;
        private string _studentName;
        private List<ShopItemDisplay> _items = new List<ShopItemDisplay>();

        public ShopWindow(int studentId, string studentName)
        {
            InitializeComponent();
            _studentId = studentId;
            _studentName = studentName;
            Loaded += ShopWindow_Loaded;
        }

        private async void ShopWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadShopData();
        }

        private async System.Threading.Tasks.Task LoadShopData()
        {
            try
            {
                var statsResponse = await TcpClientHelper.SendRequestAsync($"GET_STATISTICS|{_studentId}");
                if (!statsResponse.StartsWith("ERROR"))
                {
                    var stats = statsResponse.Split('|');
                    if (stats.Length >= 6)
                    {
                        BalanceText.Text = stats[4];
                    }
                }

                var response = await TcpClientHelper.SendRequestAsync($"GET_SHOP_ITEMS");
                if (response.StartsWith("ERROR"))
                {
                    MessageBox.Show(response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _items.Clear();
                foreach (var s in response.Split(';'))
                {
                    var parts = s.Split('|');
                    if (parts.Length >= 5)
                    {
                        _items.Add(new ShopItemDisplay
                        {
                            ItemId = int.Parse(parts[0]),
                            Name = parts[1],
                            Description = parts[2],
                            Price = int.Parse(parts[3]),
                            Quantity = int.Parse(parts[4])
                        });
                    }
                }
                ShopItemsControl.ItemsSource = _items;

                var purchasesResponse = await TcpClientHelper.SendRequestAsync($"GET_PURCHASES|{_studentId}");
                if (!purchasesResponse.StartsWith("ERROR"))
                {
                    var purchases = new List<PurchaseDisplayItem>();
                    foreach (var p in purchasesResponse.Split(';'))
                    {
                        var parts = p.Split('|');
                        if (parts.Length >= 5)
                        {
                            purchases.Add(new PurchaseDisplayItem
                            {
                                ItemName = parts[1],
                                Date = DateTime.Parse(parts[2]),
                                Quantity = int.Parse(parts[3]),
                                SpentTokens = int.Parse(parts[4])
                            });
                        }
                    }
                    PurchasesItemsControl.ItemsSource = purchases;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки магазина: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is ShopItemDisplay item)
            {
                if (item.Quantity < 1)
                {
                    MessageBox.Show("Товар закончился!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"🛒 Подтверждение покупки:\n\n" +
                    $"Товар: {item.Name}\n" +
                    $"Цена: {item.Price} 🪙\n\n" +
                    $"Купить товар?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var response = await TcpClientHelper.SendRequestAsync($"PURCHASE_ITEM|{_studentId}|{item.ItemId}|1");

                    if (response.StartsWith("SUCCESS"))
                    {
                        var parts = response.Split('|');
                        MessageBox.Show($" {item.Name} куплен!\n\nОстаток жетонов: {parts[1]}",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadShopData();
                    }
                    else
                    {
                        MessageBox.Show($" Ошибка покупки:\n{response}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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