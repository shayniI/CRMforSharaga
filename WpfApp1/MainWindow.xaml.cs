using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfApp1.Pages;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private Brush _menuForeground;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            MainFrame.Navigate(new DashboardPage());
            try
            {
                _menuForeground = (Brush)Application.Current.Resources["MaterialDesignBody"]!;
            }
            catch
            {
                _menuForeground = Brushes.Black;
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UserData.GetSession() != null)
                {
                    var uid = UserData.GetSession().User.Id;
                    var profile = await SupabaseController.GetProfileAsync(uid);
                    if (profile != null) UserData.SetProfile(profile);
                }
            }
            catch { }

            BuildMenu();
        }

        private void BuildMenu()
        {
            MenuList.Items.Clear();
            var role = UserData.GetRole() ?? "user";
            var user = UserData.GetSession()?.User;
            UserInfoTextBlock.Text = $"Пользователь: {(user?.Email ?? "неавторизован")}   Роль: {role}";

            AddMenuItem("Дашборд", () => MainFrame.Navigate(new DashboardPage()));
            AddMenuItem("Клиенты", () => MainFrame.Navigate(new ClientsPage()));
            AddMenuItem("Товары", () => MainFrame.Navigate(new ProductsPage()));
            AddMenuItem("Заказы", () => MainFrame.Navigate(new OrdersPage()));
            AddMenuItem("Отчёты", () => MainFrame.Navigate(new ReportsPage()));

            if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
            {
                AddMenuItem("Администрирование", () => MainFrame.Navigate(new AdminPage()));
            }

            AddMenuItem("Выйти", Logout);
        }

        private void AddMenuItem(string title, Action onClick)
        {
            var btn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinHeight = 48,
                Padding = new Thickness(12, 8, 12, 8),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = _menuForeground
            };

            btn.Content = new TextBlock
            {
                Text = title,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14,
                Margin = new Thickness(4, 0, 0, 0),
                Foreground = _menuForeground
            };

            btn.Click += (s, e) => onClick();
            MenuList.Items.Add(btn);
        }

        private void Logout()
        {
            UserData.LogOut();
            var auth = new AuthForm();
            auth.Show();
            this.Close();
        }
    }
}
