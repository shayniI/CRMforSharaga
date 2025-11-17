using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using WpfApp1.Models;
using System.Threading.Tasks;
using System;

namespace WpfApp1.Pages
{
    public class ClientsPage : Page
    {
        private DataGrid _grid;
        private ObservableCollection<Client> _clients = new();

        public ClientsPage()
        {
            Title = "Клиенты";
            var sp = new StackPanel { Margin = new Thickness(12) };
            sp.Children.Add(new TextBlock { Text = "Клиенты", FontSize = 18, FontWeight = FontWeights.SemiBold });

            var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,8,0,8) };
            var addBtn = new Button { Content = "Добавить клиента", Width = 200, Margin = new Thickness(0,0,8,0) };
            addBtn.Click += async (s, e) => await AddClientAsync();
            toolbar.Children.Add(addBtn);
            sp.Children.Add(toolbar);

            _grid = new DataGrid { AutoGenerateColumns = false, Height = 420, IsReadOnly = true };
            _grid.Columns.Add(new DataGridTextColumn { Header = "ФИО", Binding = new System.Windows.Data.Binding("FullName") });
            _grid.Columns.Add(new DataGridTextColumn { Header = "Телефон", Binding = new System.Windows.Data.Binding("Phone") });
            _grid.Columns.Add(new DataGridTextColumn { Header = "Email", Binding = new System.Windows.Data.Binding("Email") });
            _grid.Columns.Add(new DataGridTextColumn { Header = "Адрес", Binding = new System.Windows.Data.Binding("Address") });
            _grid.MouseDoubleClick += async (s, e) => { if (_grid.SelectedItem is Client client) await EditClientAsync(client); };
            sp.Children.Add(_grid);

            var actionToolbar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 0) };
            var editBtn = new Button { Content = "Редактировать", Width = 150, Margin = new Thickness(0, 0, 8, 0) };
            editBtn.Click += async (s, e) => { if (_grid.SelectedItem is Client client) await EditClientAsync(client); };
            var deleteBtn = new Button { Content = "Удалить", Width = 150 };
            deleteBtn.Click += async (s, e) => { if (_grid.SelectedItem is Client client) await DeleteClientAsync(client); };
            actionToolbar.Children.Add(editBtn);
            actionToolbar.Children.Add(deleteBtn);
            sp.Children.Add(actionToolbar);

            Content = sp;
            Loaded += ClientsPage_Loaded;
        }

        private async void ClientsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadClientsAsync();
        }

        private async Task LoadClientsAsync()
        {
            try
            {
                var arr = await SupabaseController.GetClientsAsync();
                _clients.Clear();
                if (arr != null)
                {
                    foreach (var c in arr) _clients.Add(c);
                }
                _grid.ItemsSource = _clients;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки клиентов: " + ex.Message);
            }
        }

        private async Task AddClientAsync()
        {
            var dlg = new Window { Title = "Новый клиент", Width = 400, Height = 300, WindowStartupLocation = WindowStartupLocation.CenterScreen };
            var sp = new StackPanel { Margin = new Thickness(12) };
            var name = new TextBox { Margin = new Thickness(0,0,0,8) };
            name.GotFocus += (s, e) => { if (name.Text == "ФИО") name.Text = ""; };
            name.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(name.Text)) name.Text = "ФИО"; };
            name.Text = "ФИО";

            var phone = new TextBox { Margin = new Thickness(0,0,0,8) };
            phone.GotFocus += (s, e) => { if (phone.Text == "Телефон") phone.Text = ""; };
            phone.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(phone.Text)) phone.Text = "Телефон"; };
            phone.Text = "Телефон";

            var email = new TextBox { Margin = new Thickness(0,0,0,8) };
            email.GotFocus += (s, e) => { if (email.Text == "Email") email.Text = ""; };
            email.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(email.Text)) email.Text = "Email"; };
            email.Text = "Email";

            var addr = new TextBox { Margin = new Thickness(0,0,0,8) };
            addr.GotFocus += (s, e) => { if (addr.Text == "Адрес") addr.Text = ""; };
            addr.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(addr.Text)) addr.Text = "Адрес"; };
            addr.Text = "Адрес";

            var save = new Button { Content = "Сохранить", Width = 100 };
            save.Click += async (s, e) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(name.Text) || name.Text == "ФИО")
                    {
                        MessageBox.Show("Укажите ФИО клиента.");
                        return;
                    }

                    var client = new Client 
                    { 
                        Id = Guid.NewGuid().ToString(),
                        FullName = name.Text, 
                        Phone = phone.Text == "Телефон" ? null : phone.Text, 
                        Email = email.Text == "Email" ? null : email.Text, 
                        Address = addr.Text == "Адрес" ? null : addr.Text,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    var created = await SupabaseController.CreateClientAsync(client);
                    if (created == null)
                    {
                        MessageBox.Show("Ошибка создания клиента.");
                        return;
                    }
                    
                    dlg.Close();
                    await LoadClientsAsync();
                }
                catch (Exception ex)
                {
                    var errorMsg = ex.Message;
                    if (ex.InnerException != null)
                        errorMsg += "\n" + ex.InnerException.Message;
                    MessageBox.Show("Ошибка при создании клиента: " + errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            sp.Children.Add(name);
            sp.Children.Add(phone);
            sp.Children.Add(email);
            sp.Children.Add(addr);
            sp.Children.Add(save);
            dlg.Content = sp;
            dlg.ShowDialog();
        }

        private async Task EditClientAsync(Client client)
        {
            var dlg = new Window { Title = "Редактировать клиента", Width = 400, Height = 300, WindowStartupLocation = WindowStartupLocation.CenterScreen };
            var sp = new StackPanel { Margin = new Thickness(12) };
            var name = new TextBox { Text = client.FullName ?? "", Margin = new Thickness(0, 0, 0, 8) };
            var phone = new TextBox { Text = client.Phone ?? "", Margin = new Thickness(0, 0, 0, 8) };
            var email = new TextBox { Text = client.Email ?? "", Margin = new Thickness(0, 0, 0, 8) };
            var addr = new TextBox { Text = client.Address ?? "", Margin = new Thickness(0, 0, 0, 8) };

            var save = new Button { Content = "Сохранить", Width = 100 };
            save.Click += async (s, e) =>
            {
                client.FullName = name.Text;
                client.Phone = phone.Text;
                client.Email = email.Text;
                client.Address = addr.Text;
                await SupabaseController.UpdateClientAsync(client);
                dlg.Close();
                await LoadClientsAsync();
            };
            sp.Children.Add(new TextBlock { Text = "ФИО", Margin = new Thickness(0, 0, 0, 4) });
            sp.Children.Add(name);
            sp.Children.Add(new TextBlock { Text = "Телефон", Margin = new Thickness(0, 4, 0, 4) });
            sp.Children.Add(phone);
            sp.Children.Add(new TextBlock { Text = "Email", Margin = new Thickness(0, 4, 0, 4) });
            sp.Children.Add(email);
            sp.Children.Add(new TextBlock { Text = "Адрес", Margin = new Thickness(0, 4, 0, 4) });
            sp.Children.Add(addr);
            sp.Children.Add(save);
            dlg.Content = sp;
            dlg.ShowDialog();
        }

        private async Task DeleteClientAsync(Client client)
        {
            var result = MessageBox.Show($"Вы уверены, что хотите удалить клиента {client.FullName}?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await SupabaseController.DeleteClientAsync(client.Id);
                    await LoadClientsAsync();
                    MessageBox.Show("Клиент удалён.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении: " + ex.Message);
                }
            }
        }
    }
}   