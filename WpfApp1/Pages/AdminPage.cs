using System.Windows.Controls;
using System.Windows;
using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using WpfApp1.Models;

namespace WpfApp1.Pages
{
    public class AdminPage : Page
    {
        private TextBox _userIdBox;
        private TextBox _emailBox;
        private PasswordBox _passwordBox;
        private TextBox _fullNameBox;
        private ComboBox _roleBox;
        private Button _applyBtn;
        private Button _createBtn;

        public AdminPage()
        {
            Title = "Администрирование";
            var sp = new StackPanel { Margin = new Thickness(12) };
            sp.Children.Add(new TextBlock { Text = "Администрирование пользователей", FontSize = 18, FontWeight = FontWeights.SemiBold });
            sp.Children.Add(new TextBlock { Text = "Назначение роли пользователю (user / admin)", Margin = new Thickness(0,8,0,8) });

            var userLabel = new TextBlock
            {
                Text = "User ID (UUID из auth.users)",
                Foreground = SystemColors.GrayTextBrush,
                Margin = new Thickness(0, 0, 0, 4)
            };
            sp.Children.Add(userLabel);

            _userIdBox = new TextBox { Margin = new Thickness(0,0,0,8) };
            sp.Children.Add(_userIdBox);

            _roleBox = new ComboBox { ItemsSource = new[] { "user", "admin" }, SelectedIndex = 0, Margin = new Thickness(0,0,0,8) };
            sp.Children.Add(_roleBox);

            _applyBtn = new Button { Content = "Применить роль", Width = 160, Margin = new Thickness(0,0,0,16) };
            _applyBtn.Click += async (s, e) => await ApplyRoleAsync();
            sp.Children.Add(_applyBtn);

            sp.Children.Add(new TextBlock { Text = "Создать нового пользователя", FontSize = 16, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0,8,0,4) });

            sp.Children.Add(new TextBlock { Text = "Email", Margin = new Thickness(0,4,0,2) });
            _emailBox = new TextBox { Margin = new Thickness(0,0,0,8) };
            sp.Children.Add(_emailBox);

            sp.Children.Add(new TextBlock { Text = "Пароль", Margin = new Thickness(0,4,0,2) });
            _passwordBox = new PasswordBox { Margin = new Thickness(0,0,0,8) };
            sp.Children.Add(_passwordBox);

            sp.Children.Add(new TextBlock { Text = "Полное имя (необязательно)", Margin = new Thickness(0,4,0,2) });
            _fullNameBox = new TextBox { Margin = new Thickness(0,0,0,8) };
            sp.Children.Add(_fullNameBox);

            sp.Children.Add(new TextBlock { Text = "Роль для нового пользователя", Margin = new Thickness(0,4,0,2) });
            var createRoleBox = new ComboBox { ItemsSource = new[] { "user", "admin" }, SelectedIndex = 0, Margin = new Thickness(0,0,0,12) };
            sp.Children.Add(createRoleBox);

            _createBtn = new Button { Content = "Создать пользователя", Width = 200 };
            _createBtn.Click += async (s, e) => await CreateUserAsync(createRoleBox);
            sp.Children.Add(_createBtn);

            Content = sp;
        }

        private async Task ApplyRoleAsync()
        {
            var userId = _userIdBox.Text?.Trim();
            var role = _roleBox.SelectedItem as string ?? "user";
            if (string.IsNullOrEmpty(userId))
            {
                MessageBox.Show("Укажите User ID.");
                return;
            }

            try
            {
                _applyBtn.IsEnabled = false;
                var ok = await SupabaseController.SetUserRoleAsync(userId, role);
                MessageBox.Show(ok ? "Роль обновлена." : "Не удалось обновить роль.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
            finally
            {
                _applyBtn.IsEnabled = true;
            }
        }

        private async Task CreateUserAsync(ComboBox roleBox)
        {
            var email = _emailBox.Text?.Trim();
            var password = _passwordBox.Password;
            var fullName = _fullNameBox.Text?.Trim();
            var role = roleBox.SelectedItem as string ?? "user";

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Укажите email и пароль для нового пользователя.");
                return;
            }

            try
            {
                _createBtn.IsEnabled = false;
                var (ok, error) = await SupabaseController.CreateUserAsync(email, password, fullName, role);
                if (ok)
                {
                    MessageBox.Show("Пользователь создан.");
                    _emailBox.Text = "";
                    _passwordBox.Password = "";
                    _fullNameBox.Text = "";
                    roleBox.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("Не удалось создать пользователя:\n" + (string.IsNullOrEmpty(error) ? "Неизвестная ошибка" : error));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка создания пользователя: " + ex.Message);
            }
            finally
            {
                _createBtn.IsEnabled = true;
            }
        }
    }
}