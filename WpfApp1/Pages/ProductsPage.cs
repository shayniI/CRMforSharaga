using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using WpfApp1.Models;
using System.Threading.Tasks;
using System;

namespace WpfApp1.Pages
{
    public class ProductsPage : Page
    {
        private DataGrid _grid;
        private ObservableCollection<Product> _products = new();

        public ProductsPage()
        {
            Title = "Товары";
            var sp = new StackPanel { Margin = new Thickness(12) };
            sp.Children.Add(new TextBlock { Text = "Товары", FontSize = 18, FontWeight = FontWeights.SemiBold });

            var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,8,0,8) };
            var addBtn = new Button { Content = "Добавить товар", Width = 200, Margin = new Thickness(0,0,8,0) };
            addBtn.Click += async (s, e) => await AddProductAsync();
            toolbar.Children.Add(addBtn);
            sp.Children.Add(toolbar);

            _grid = new DataGrid { AutoGenerateColumns = false, Height = 420, IsReadOnly = true };
            _grid.Columns.Add(new DataGridTextColumn { Header = "Артикул", Binding = new System.Windows.Data.Binding("Sku") });
            _grid.Columns.Add(new DataGridTextColumn { Header = "Наименование", Binding = new System.Windows.Data.Binding("Name") });
            _grid.Columns.Add(new DataGridTextColumn { Header = "Остаток", Binding = new System.Windows.Data.Binding("Stock") });
            _grid.Columns.Add(new DataGridTextColumn
            {
                Header = "Закупочная",
                Binding = new System.Windows.Data.Binding("Cost") { StringFormat = "C" }
            });
            _grid.Columns.Add(new DataGridTextColumn
            {
                Header = "Продажная",
                Binding = new System.Windows.Data.Binding("Price") { StringFormat = "C" }
            });
            sp.Children.Add(_grid);

            Content = sp;
            Loaded += ProductsPage_Loaded;
        }

        private async void ProductsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                var arr = await SupabaseController.GetProductsAsync();
                _products.Clear();
                if (arr != null)
                {
                    foreach (var p in arr) _products.Add(p);
                }
                _grid.ItemsSource = _products;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки товаров: " + ex.Message);
            }
        }

        private async Task AddProductAsync()
        {
            var dlg = new Window { Title = "Новый товар", Width = 420, Height = 380, WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = Application.Current.MainWindow };
            var sp = new StackPanel { Margin = new Thickness(12) };
            var sku = new TextBox { Margin = new Thickness(0,0,0,8) };
            sku.GotFocus += (s, e) => { if (sku.Text == "Артикул") sku.Text = ""; };
            sku.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(sku.Text)) sku.Text = "Артикул"; };
            sku.Text = "Артикул";

            var name = new TextBox { Margin = new Thickness(0,0,0,8) };
            name.GotFocus += (s, e) => { if (name.Text == "Наименование") name.Text = ""; };
            name.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(name.Text)) name.Text = "Наименование"; };
            name.Text = "Наименование";

            var stock = new TextBox { Margin = new Thickness(0,0,0,8) };
            stock.GotFocus += (s, e) => { if (stock.Text == "Остаток") stock.Text = ""; };
            stock.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(stock.Text)) stock.Text = "Остаток"; };
            stock.Text = "Остаток";

            var cost = new TextBox { Margin = new Thickness(0,0,0,8) };
            cost.GotFocus += (s, e) => { if (cost.Text == "Закупочная цена") cost.Text = ""; };
            cost.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(cost.Text)) cost.Text = "Закупочная цена"; };
            cost.Text = "Закупочная цена";

            var price = new TextBox { Margin = new Thickness(0,0,0,8) };
            price.GotFocus += (s, e) => { if (price.Text == "Продажная цена") price.Text = ""; };
            price.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(price.Text)) price.Text = "Продажная цена"; };
            price.Text = "Продажная цена";

            var save = new Button { Content = "Сохранить", Width = 100 };
            save.Click += async (s, e) =>
            {
                if (!decimal.TryParse(cost.Text, out var c)) c = 0;
                if (!decimal.TryParse(price.Text, out var p)) p = 0;
                if (!int.TryParse(stock.Text, out var st)) st = 0;
                var product = new Product { Sku = sku.Text, Name = name.Text, Stock = st, Cost = c, Price = p };
                await SupabaseController.CreateProductAsync(product);
                dlg.Close();
                await LoadProductsAsync();
            };
            sp.Children.Add(sku);
            sp.Children.Add(name);
            sp.Children.Add(stock);
            sp.Children.Add(cost);
            sp.Children.Add(price);
            sp.Children.Add(save);
            dlg.Content = sp;
            dlg.ShowDialog();
        }
    }
}