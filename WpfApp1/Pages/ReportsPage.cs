using System.Windows.Controls;
using System.Windows;
using System;
using System.Threading.Tasks;
using WpfApp1.Models;

namespace WpfApp1.Pages
{
    public class ReportsPage : Page
    {
        private TextBlock _dailyRevenue;
        private TextBlock _monthlyProfit;
        private TextBlock _popular;

        public ReportsPage()
        {
            Title = "Отчёты";
            var sp = new StackPanel { Margin = new Thickness(12) };
            sp.Children.Add(new TextBlock { Text = "Отчёты", FontSize = 18, FontWeight = FontWeights.SemiBold });

            _dailyRevenue = new TextBlock { Text = "Ежедневная выручка: ...", Margin = new Thickness(0,8,0,4) };
            _monthlyProfit = new TextBlock { Text = "Месячная прибыль: ...", Margin = new Thickness(0,4,0,4) };
            _popular = new TextBlock { Text = "Популярные товары: ...", Margin = new Thickness(0,4,0,4) };

            sp.Children.Add(_dailyRevenue);
            sp.Children.Add(_monthlyProfit);
            sp.Children.Add(_popular);

            Content = sp;
            Loaded += ReportsPage_Loaded;
        }

        private async void ReportsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadReportsAsync();
        }

        private async Task LoadReportsAsync()
        {
            try
            {
                var today = DateTime.Now;
                var daily = await SupabaseController.GetDailyRevenueAsync(today);
                var monthly = await SupabaseController.GetMonthlyProfitAsync(today.Year, today.Month);
                var popular = await SupabaseController.GetPopularProductsWithNamesAsync(5);

                _dailyRevenue.Text = $"Ежедневная выручка ({today:dd.MM.yyyy}): {daily:C}";
                _monthlyProfit.Text = $"Месячная прибыль ({today:MMMM yyyy}): {monthly:C}";

                if (popular.Length == 0) _popular.Text = "Популярные товары: отсутствуют данные";
                else
                {
                    var items = string.Join(", ", popular.Select(p => $"{p.productName} ({p.count} шт.)"));
                    _popular.Text = "Популярные товары: " + items;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка формирования отчётов: " + ex.Message);
            }
        }
    }
}