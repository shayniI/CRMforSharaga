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
            sp.Children.Add(_grid);

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
            var dlg = new Window { Title = "Новый клиент", Width = 400, Height = 300, WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = Application.Current.MainWindow };
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
                var client = new Client { FullName = name.Text, Phone = phone.Text, Email = email.Text, Address = addr.Text };
                await SupabaseController.CreateClientAsync(client);
                dlg.Close();
                await LoadClientsAsync();
            };
            sp.Children.Add(name);
            sp.Children.Add(phone);
            sp.Children.Add(email);
            sp.Children.Add(addr);
            sp.Children.Add(save);
            dlg.Content = sp;
            dlg.ShowDialog();
        }
    }
}   