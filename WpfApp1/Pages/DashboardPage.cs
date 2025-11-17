using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace WpfApp1.Pages
{
    public class DashboardPage : Page
    {
        public DashboardPage()
        {
            Title = "Дашборд";
            var sp = new StackPanel { Margin = new Thickness(16) };
            sp.Children.Add(new TextBlock
            {
                Text = "Дашборд — быстрые показатели",
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8)
            });

            sp.Children.Add(new TextBlock { Text = "Здесь будут отображаться: ежедневная выручка, текущие заказы, популярные товары." });
            Content = sp;
        }
    }
}