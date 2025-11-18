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
                Binding = new System.Windows.Data.Binding("Cost")
            });
            _grid.Columns.Add(new DataGridTextColumn
            {
                Header = "Продажная",
                Binding = new System.Windows.Data.Binding("Price")
            });
            _grid.MouseDoubleClick += async (s, e) => { if (_grid.SelectedItem is Product product) await EditProductAsync(product); };
            sp.Children.Add(_grid);

            var actionToolbar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 0) };
            var editBtn = new Button { Content = "Редактировать", Width = 150, Margin = new Thickness(0, 0, 8, 0) };
            editBtn.Click += async (s, e) => { if (_grid.SelectedItem is Product product) await EditProductAsync(product); };
            var deleteBtn = new Button { Content = "Удалить", Width = 150 };
            deleteBtn.Click += async (s, e) => { if (_grid.SelectedItem is Product product) await DeleteProductAsync(product); };
            actionToolbar.Children.Add(editBtn);
            actionToolbar.Children.Add(deleteBtn);
            sp.Children.Add(actionToolbar);

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
            var dlg = new Window { Title = "Новый товар", Width = 420, Height = 380, WindowStartupLocation = WindowStartupLocation.CenterScreen };
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
                try
                {
                    if (string.IsNullOrWhiteSpace(name.Text) || name.Text == "Наименование")
                    {
                        MessageBox.Show("Укажите наименование товара.");
                        return;
                    }

                    if (!decimal.TryParse(cost.Text == "Закупочная цена" ? "0" : cost.Text, out var c)) c = 0;
                    if (!decimal.TryParse(price.Text == "Продажная цена" ? "0" : price.Text, out var p)) p = 0;
                    if (!int.TryParse(stock.Text == "Остаток" ? "0" : stock.Text, out var st)) st = 0;
                    
                    var product = new Product 
                    { 
                        Id = Guid.NewGuid().ToString(),
                        Sku = sku.Text == "Артикул" ? null : sku.Text, 
                        Name = name.Text, 
                        Stock = st, 
                        Cost = c, 
                        Price = p,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    var created = await SupabaseController.CreateProductAsync(product);
                    if (created == null)
                    {
                        MessageBox.Show("Ошибка создания товара.");
                        return;
                    }
                    
                    dlg.Close();
                    await LoadProductsAsync();
                }
                catch (Exception ex)
                {
                    var errorMsg = ex.Message;
                    if (ex.InnerException != null)
                        errorMsg += "\n" + ex.InnerException.Message;
                    MessageBox.Show("Ошибка при создании товара: " + errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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

        private async Task EditProductAsync(Product product)
        {
            var dlg = new Window { Title = "Редактировать товар", Width = 420, Height = 380, WindowStartupLocation = WindowStartupLocation.CenterScreen };
            var sp = new StackPanel { Margin = new Thickness(12) };
            var sku = new TextBox { Text = product.Sku ?? "", Margin = new Thickness(0, 0, 0, 8) };
            var name = new TextBox { Text = product.Name ?? "", Margin = new Thickness(0, 0, 0, 8) };
            var stock = new TextBox { Text = product.Stock.ToString(), Margin = new Thickness(0, 0, 0, 8) };
            var cost = new TextBox { Text = product.Cost.ToString(), Margin = new Thickness(0, 0, 0, 8) };
            var price = new TextBox { Text = product.Price.ToString(), Margin = new Thickness(0, 0, 0, 8) };

            var save = new Button { Content = "Сохранить", Width = 100 };
            save.Click += async (s, e) =>
            {
                if (!decimal.TryParse(cost.Text, out var c)) c = product.Cost;
                if (!decimal.TryParse(price.Text, out var p)) p = product.Price;
                if (!int.TryParse(stock.Text, out var st)) st = product.Stock;
                product.Sku = sku.Text;
                product.Name = name.Text;
                product.Stock = st;
                product.Cost = c;
                product.Price = p;
                await SupabaseController.UpdateProductAsync(product);
                dlg.Close();
                await LoadProductsAsync();
            };
            sp.Children.Add(new TextBlock { Text = "Артикул", Margin = new Thickness(0, 0, 0, 4) });
            sp.Children.Add(sku);
            sp.Children.Add(new TextBlock { Text = "Наименование", Margin = new Thickness(0, 4, 0, 4) });
            sp.Children.Add(name);
            sp.Children.Add(new TextBlock { Text = "Остаток", Margin = new Thickness(0, 4, 0, 4) });
            sp.Children.Add(stock);
            sp.Children.Add(new TextBlock { Text = "Закупочная цена", Margin = new Thickness(0, 4, 0, 4) });
            sp.Children.Add(cost);
            sp.Children.Add(new TextBlock { Text = "Продажная цена", Margin = new Thickness(0, 4, 0, 4) });
            sp.Children.Add(price);
            sp.Children.Add(save);
            dlg.Content = sp;
            dlg.ShowDialog();
        }

        private async Task DeleteProductAsync(Product product)
        {
            var result = MessageBox.Show($"Вы уверены, что хотите удалить товар {product.Name}?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await SupabaseController.DeleteProductAsync(product.Id);
                    await LoadProductsAsync();
                    MessageBox.Show("Товар удалён.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении: " + ex.Message);
                }
            }
        }
    }
}