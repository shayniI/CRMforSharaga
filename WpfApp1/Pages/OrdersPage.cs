using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using WpfApp1.Models;
using System.Threading.Tasks;
using System;

namespace WpfApp1.Pages
{
    public class OrdersPage : Page
    {
        private DataGrid _grid;
        private ObservableCollection<Order> _orders = new();

        public OrdersPage()
        {
            Title = "Заказы";
            var sp = new StackPanel { Margin = new Thickness(12) };
            sp.Children.Add(new TextBlock { Text = "Заказы", FontSize = 18, FontWeight = FontWeights.SemiBold });

            var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,8,0,8) };
            var addBtn = new Button { Content = "Новый заказ", Width = 200, Margin = new Thickness(0,0,8,0) };
            addBtn.Click += async (s, e) => await AddOrderAsync();
            toolbar.Children.Add(addBtn);
            sp.Children.Add(toolbar);

            _grid = new DataGrid { AutoGenerateColumns = false, Height = 420, IsReadOnly = true };
            _grid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new System.Windows.Data.Binding("Id") });
            _grid.Columns.Add(new DataGridTextColumn { Header = "Клиент ID", Binding = new System.Windows.Data.Binding("ClientId") });
            _grid.Columns.Add(new DataGridTextColumn { Header = "Статус", Binding = new System.Windows.Data.Binding("Status") });
            var totalColumn = new DataGridTextColumn { Header = "Сумма" };
            var totalBinding = new System.Windows.Data.Binding("Total") { StringFormat = "C" };
            totalColumn.Binding = totalBinding;
            _grid.Columns.Add(totalColumn);
            _grid.Columns.Add(new DataGridTextColumn { Header = "Создан", Binding = new System.Windows.Data.Binding("CreatedAt") });
            sp.Children.Add(_grid);

            Content = sp;
            Loaded += OrdersPage_Loaded;
        }

        private async void OrdersPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadOrdersAsync();
        }

        private async Task LoadOrdersAsync()
        {
            try
            {
                var arr = await SupabaseController.GetOrdersAsync();
                _orders.Clear();
                if (arr != null)
                {
                    foreach (var o in arr) _orders.Add(o);
                }
                _grid.ItemsSource = _orders;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заказов: " + ex.Message);
            }
        }

        private async Task AddOrderAsync()
        {
            MessageBox.Show("Создание заказа — реализовать по сценарию (выбор клиента, услуг, позиций). Плейсхолдер.");
        }
    }
}