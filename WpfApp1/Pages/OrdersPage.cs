using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using WpfApp1.Models;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace WpfApp1.Pages
{
    public class OrderItemViewModel
    {
        public Product? Product { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal Total => Quantity * UnitPrice;
    }

    public class OrderDisplayItem
    {
        public string Id { get; set; } = null!;
        public string? ClientName { get; set; }
        public string Status { get; set; } = null!;
        public decimal Total { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Order OriginalOrder { get; set; } = null!;
    }

    public class OrdersPage : Page
    {
        private DataGrid _grid;
        private ObservableCollection<OrderDisplayItem> _orders = new();

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
            _grid.Columns.Add(new DataGridTextColumn { Header = "Клиент", Binding = new System.Windows.Data.Binding("ClientName") });
            _grid.Columns.Add(new DataGridTextColumn { Header = "Статус", Binding = new System.Windows.Data.Binding("Status") });
            var totalColumn = new DataGridTextColumn { Header = "Сумма" };
            var totalBinding = new System.Windows.Data.Binding("Total");
            totalColumn.Binding = totalBinding;
            _grid.Columns.Add(totalColumn);
            _grid.Columns.Add(new DataGridTextColumn { Header = "Создан", Binding = new System.Windows.Data.Binding("CreatedAt") });
            _grid.MouseDoubleClick += async (s, e) => { if (_grid.SelectedItem is OrderDisplayItem item) await ViewOrderDetailsAsync(item.OriginalOrder); };
            sp.Children.Add(_grid);

            var actionToolbar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 0) };
            var viewBtn = new Button { Content = "Просмотр", Width = 150, Margin = new Thickness(0, 0, 8, 0) };
            viewBtn.Click += async (s, e) => { if (_grid.SelectedItem is OrderDisplayItem item) await ViewOrderDetailsAsync(item.OriginalOrder); };
            var editBtn = new Button { Content = "Изменить статус", Width = 150, Margin = new Thickness(0, 0, 8, 0) };
            editBtn.Click += async (s, e) => { if (_grid.SelectedItem is OrderDisplayItem item) await ChangeOrderStatusAsync(item.OriginalOrder); };
            var deleteBtn = new Button { Content = "Удалить", Width = 150 };
            deleteBtn.Click += async (s, e) => { if (_grid.SelectedItem is OrderDisplayItem item) await DeleteOrderAsync(item.OriginalOrder); };
            actionToolbar.Children.Add(viewBtn);
            actionToolbar.Children.Add(editBtn);
            actionToolbar.Children.Add(deleteBtn);
            sp.Children.Add(actionToolbar);

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
                var clients = await SupabaseController.GetClientsAsync();
                _orders.Clear();
                if (arr != null)
                {
                    foreach (var o in arr)
                    {
                        if (string.IsNullOrEmpty(o.Id))
                            continue;

                        var displayItem = new OrderDisplayItem
                        {
                            Id = o.Id,
                            Status = o.Status,
                            Total = o.Total,
                            CreatedAt = o.CreatedAt,
                            OriginalOrder = o
                        };

                        if (!string.IsNullOrEmpty(o.ClientId) && clients != null)
                        {
                            var client = clients.FirstOrDefault(c => c.Id == o.ClientId);
                            displayItem.ClientName = client?.FullName ?? "Неизвестно";
                        }
                        else
                        {
                            displayItem.ClientName = "Не указан";
                        }

                        _orders.Add(displayItem);
                    }
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
            var clients = await SupabaseController.GetClientsAsync();
            var products = await SupabaseController.GetProductsAsync();

            if (clients == null || clients.Length == 0)
            {
                MessageBox.Show("Нет клиентов. Сначала добавьте клиента.");
                return;
            }

            if (products == null || products.Length == 0)
            {
                MessageBox.Show("Нет товаров. Сначала добавьте товары.");
                return;
            }

            var dlg = new Window
            {
                Title = "Новый заказ",
                Width = 700,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var mainSp = new StackPanel { Margin = new Thickness(12) };

            mainSp.Children.Add(new TextBlock { Text = "Клиент", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
            var clientCombo = new ComboBox
            {
                ItemsSource = clients,
                DisplayMemberPath = "FullName",
                SelectedValuePath = "Id",
                Margin = new Thickness(0, 0, 0, 12),
                SelectedIndex = 0
            };
            mainSp.Children.Add(clientCombo);

            mainSp.Children.Add(new TextBlock { Text = "Товары в заказе", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
            var orderItems = new ObservableCollection<OrderItemViewModel>();
            var itemsGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                Height = 200,
                ItemsSource = orderItems,
                CanUserAddRows = false
            };
            itemsGrid.Columns.Add(new DataGridTextColumn { Header = "Товар", Binding = new System.Windows.Data.Binding("Product.Name") });
            itemsGrid.Columns.Add(new DataGridTextColumn { Header = "Количество", Binding = new System.Windows.Data.Binding("Quantity") });
            itemsGrid.Columns.Add(new DataGridTextColumn { Header = "Цена", Binding = new System.Windows.Data.Binding("UnitPrice") });
            itemsGrid.Columns.Add(new DataGridTextColumn { Header = "Сумма", Binding = new System.Windows.Data.Binding("Total") });
            mainSp.Children.Add(itemsGrid);

            var totalText = new TextBlock
            {
                Text = "Итого: 0 ₽",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 8, 0, 12)
            };

            var itemsToolbar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 12) };
            var addItemBtn = new Button { Content = "Добавить товар", Width = 150, Margin = new Thickness(0, 0, 8, 0) };
            var removeItemBtn = new Button { Content = "Удалить товар", Width = 150 };
            itemsToolbar.Children.Add(addItemBtn);
            itemsToolbar.Children.Add(removeItemBtn);
            mainSp.Children.Add(itemsToolbar);

            addItemBtn.Click += (s, e) =>
            {
                var itemDlg = new Window
                {
                    Title = "Добавить товар",
                    Width = 400,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = dlg
                };
                var itemSp = new StackPanel { Margin = new Thickness(12) };
                var productCombo = new ComboBox
                {
                    ItemsSource = products,
                    DisplayMemberPath = "Name",
                    SelectedValuePath = "Id",
                    Margin = new Thickness(0, 0, 0, 8)
                };
                var qtyBox = new TextBox { Text = "1", Margin = new Thickness(0, 0, 0, 8) };
                itemSp.Children.Add(new TextBlock { Text = "Товар" });
                itemSp.Children.Add(productCombo);
                itemSp.Children.Add(new TextBlock { Text = "Количество" });
                itemSp.Children.Add(qtyBox);
                var saveItemBtn = new Button { Content = "Добавить", Width = 100 };
                saveItemBtn.Click += (s2, e2) =>
                {
                    if (productCombo.SelectedItem is Product product && int.TryParse(qtyBox.Text, out var qty) && qty > 0)
                    {
                        orderItems.Add(new OrderItemViewModel
                        {
                            Product = product,
                            Quantity = qty,
                            UnitPrice = product.Price
                        });
                        totalText.Text = $"Итого: {orderItems.Sum(i => i.Total):C}";
                        itemDlg.Close();
                    }
                    else
                    {
                        MessageBox.Show("Выберите товар и укажите количество больше 0.");
                    }
                };
                itemSp.Children.Add(saveItemBtn);
                itemDlg.Content = itemSp;
                itemDlg.ShowDialog();
            };

            removeItemBtn.Click += (s, e) =>
            {
                if (itemsGrid.SelectedItem is OrderItemViewModel item)
                {
                    orderItems.Remove(item);
                    totalText.Text = $"Итого: {orderItems.Sum(i => i.Total):C}";
                }
            };

            mainSp.Children.Add(totalText);

            var buttonsSp = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var saveBtn = new Button { Content = "Создать заказ", Width = 150, Margin = new Thickness(0, 0, 8, 0) };
            var cancelBtn = new Button { Content = "Отмена", Width = 100 };
            cancelBtn.Click += (s, e) => dlg.Close();
            saveBtn.Click += async (s, e) =>
            {
                try
                {
                    if (clientCombo.SelectedValue == null)
                    {
                        MessageBox.Show("Выберите клиента.");
                        return;
                    }
                    if (orderItems.Count == 0)
                    {
                        MessageBox.Show("Добавьте хотя бы один товар в заказ.");
                        return;
                    }
                    var clientIdStr = clientCombo.SelectedValue?.ToString();
                    if (string.IsNullOrEmpty(clientIdStr))
                    {
                        MessageBox.Show("Ошибка: не выбран клиент.");
                        return;
                    }

                    var order = new Order
                    {
                        ClientId = clientIdStr,
                        Status = "new",
                        Total = orderItems.Sum(i => i.Total),
                        CreatedAt = DateTime.UtcNow,
                        Items = null
                    };

                    var createdOrder = await SupabaseController.CreateOrderAsync(order);
                    if (createdOrder == null || string.IsNullOrEmpty(createdOrder.Id))
                    {
                        MessageBox.Show("Ошибка создания заказа: не получен ID заказа.");
                        return;
                    }

                    foreach (var item in orderItems)
                    {
                        if (item.Product == null || string.IsNullOrEmpty(item.Product.Id))
                        {
                            MessageBox.Show($"Ошибка: товар без ID пропущен.");
                            continue;
                        }

                        var orderItem = new OrderItem
                        {
                            OrderId = createdOrder.Id,
                            ProductId = item.Product.Id,
                            Description = item.Product.Name,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice
                        };
                        var result = await SupabaseController.CreateOrderItemAsync(orderItem);
                        var invoice = new Invoice 
                        {
                            OrderId = result?.OrderId,
                            IssuedTo = order.ClientId,
                            Amount = orderItems.Sum(i=>i.Total),
                            CreatedAt = DateTime.Now
                        };
                        await SupabaseController.CreateInvoiceAsync(invoice);
                        var newStock = item.Product.Stock - item.Quantity;
                        if (newStock >= 0)
                        {
                            await SupabaseController.UpdateProductStockAsync(item.Product.Id, newStock);
                        }
                    }

                    MessageBox.Show("Заказ создан успешно.");
                    dlg.Close();
                    await LoadOrdersAsync();
                }
                catch (Exception ex)
                {
                    var errorMsg = ex.Message;
                    if (ex.InnerException != null)
                        errorMsg += "\n" + ex.InnerException.Message;
                }
            };
            buttonsSp.Children.Add(saveBtn);
            buttonsSp.Children.Add(cancelBtn);
            mainSp.Children.Add(buttonsSp);

            dlg.Content = mainSp;
            dlg.ShowDialog();
        }

        private async Task ViewOrderDetailsAsync(Order order)
        {
            if (string.IsNullOrEmpty(order.Id))
            {
                MessageBox.Show("Ошибка: у заказа отсутствует ID.");
                return;
            }

            var items = await SupabaseController.GetOrderItemsByOrderIdAsync(order.Id);
            var clients = await SupabaseController.GetClientsAsync();
            var products = await SupabaseController.GetProductsAsync();

            var dlg = new Window
            {
                Title = $"Детали заказа {order.Id}",
                Width = 600,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var sp = new StackPanel { Margin = new Thickness(12) };
            sp.Children.Add(new TextBlock { Text = $"Заказ: {order.Id}", FontSize = 16, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 8) });

            if (clients != null && !string.IsNullOrEmpty(order.ClientId))
            {
                var client = clients.FirstOrDefault(c => c.Id == order.ClientId);
                sp.Children.Add(new TextBlock { Text = $"Клиент: {client?.FullName ?? "Неизвестно"}", Margin = new Thickness(0, 0, 0, 4) });
            }

            sp.Children.Add(new TextBlock { Text = $"Статус: {order.Status}", Margin = new Thickness(0, 0, 0, 4) });
            sp.Children.Add(new TextBlock { Text = $"Сумма: {order.Total}", Margin = new Thickness(0, 0, 0, 8) });

            sp.Children.Add(new TextBlock { Text = "Товары:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 8, 0, 4) });

            var itemsGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                Height = 250,
                IsReadOnly = true
            };
            itemsGrid.Columns.Add(new DataGridTextColumn { Header = "Товар", Binding = new System.Windows.Data.Binding("Description") });
            itemsGrid.Columns.Add(new DataGridTextColumn { Header = "Количество", Binding = new System.Windows.Data.Binding("Quantity") });
            itemsGrid.Columns.Add(new DataGridTextColumn { Header = "Цена", Binding = new System.Windows.Data.Binding("UnitPrice")});
            itemsGrid.Columns.Add(new DataGridTextColumn { Header = "Сумма", Binding = new System.Windows.Data.Binding("Total")});

            if (items != null)
            {
                var itemsList = new ObservableCollection<OrderItem>(items);
                itemsGrid.ItemsSource = itemsList;
            }

            sp.Children.Add(itemsGrid);
            dlg.Content = sp;
            dlg.ShowDialog();
        }

        private async Task ChangeOrderStatusAsync(Order order)
        {
            var dlg = new Window
            {
                Title = "Изменить статус заказа",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var sp = new StackPanel { Margin = new Thickness(12) };
            var statusCombo = new ComboBox
            {
                ItemsSource = new[] { "new", "in_progress", "completed", "cancelled" },
                SelectedItem = order.Status,
                Margin = new Thickness(0, 0, 0, 12)
            };
            sp.Children.Add(new TextBlock { Text = "Статус", Margin = new Thickness(0, 0, 0, 4) });
            sp.Children.Add(statusCombo);

            var saveBtn = new Button { Content = "Сохранить", Width = 100 };
            saveBtn.Click += async (s, e) =>
            {
                order.Status = statusCombo.SelectedItem?.ToString() ?? order.Status;
                order.UpdatedAt = DateTime.UtcNow;
                await SupabaseController.UpdateOrderAsync(order);
                dlg.Close();
                await LoadOrdersAsync();
            };
            sp.Children.Add(saveBtn);
            dlg.Content = sp;
            dlg.ShowDialog();
        }

        private async Task DeleteOrderAsync(Order order)
        {
            if (string.IsNullOrEmpty(order.Id))
            {
                MessageBox.Show("Ошибка: у заказа отсутствует ID.");
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить заказ {order.Id}?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await SupabaseController.DeleteOrderAsync(order.Id);
                    await LoadOrdersAsync();
                    MessageBox.Show("Заказ удалён.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении: " + ex.Message);
                }
            }
        }
    }
}