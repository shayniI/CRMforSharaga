using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace WpfApp1.Pages
{
    public class DashboardPage : Page
    {
        private TextBlock _dailyRevenue;
        private TextBlock _activeOrders;
        private TextBlock _popularProducts;
        private TextBlock _monthlyProfit;

        public DashboardPage()
        {
            Title = "Дашборд";
            var sp = new StackPanel { Margin = new Thickness(16) };
            sp.Children.Add(new TextBlock
            {
                Text = "Дашборд — быстрые показатели",
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 16)
            });

            _dailyRevenue = new TextBlock
            {
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 8),
                Text = "Ежедневная выручка: загрузка..."
            };

            _monthlyProfit = new TextBlock
            {
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 8),
                Text = "Месячная прибыль: загрузка..."
            };

            _activeOrders = new TextBlock
            {
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 8),
                Text = "Активные заказы: загрузка..."
            };

            _popularProducts = new TextBlock
            {
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 8),
                TextWrapping = TextWrapping.Wrap,
                Text = "Популярные товары: загрузка..."
            };

            sp.Children.Add(_dailyRevenue);
            sp.Children.Add(_monthlyProfit);
            sp.Children.Add(_activeOrders);
            sp.Children.Add(_popularProducts);

            Content = sp;
            Loaded += DashboardPage_Loaded;
        }

        private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDashboardDataAsync();
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                var today = DateTime.Now;
                var daily = await SupabaseController.GetDailyRevenueAsync(today);
                var monthly = await SupabaseController.GetMonthlyProfitAsync(today.Year, today.Month);
                var orders = await SupabaseController.GetOrdersAsync();
                var popular = await SupabaseController.GetPopularProductsWithNamesAsync(5);

                _dailyRevenue.Text = $"Ежедневная выручка ({today:dd.MM.yyyy}): {daily:C}";
                _monthlyProfit.Text = $"Месячная прибыль ({today:MMMM yyyy}): {monthly:C}";

                if (orders != null)
                {
                    var activeOrders = orders.Where(o => o.Status != "completed" && o.Status != "cancelled").Count();
                    var totalOrders = orders.Length;
                    _activeOrders.Text = $"Активные заказы: {activeOrders} из {totalOrders}";
                }
                else
                {
                    _activeOrders.Text = "Активные заказы: нет данных";
                }

                if (popular.Length == 0)
                {
                    _popularProducts.Text = "Популярные товары: отсутствуют данные";
                }
                else
                {
                    var items = string.Join("\n", popular.Select((p, i) => $"{i + 1}. {p.productName} — {p.count} шт."));
                    _popularProducts.Text = "Популярные товары:\n" + items;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных дашборда: " + ex.Message);
            }
        }
    }
}