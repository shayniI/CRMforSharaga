using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var session = await App._Supabase!.Auth.SignIn(EmailTextBox.Text!, PasswordBox.Password!);

                // Явно проверяем результат аутентификации до доступа к свойствам
                if (session == null || session.User == null)
                {
                    MessageBox.Show("Не удалось войти. Проверьте email/пароль.");
                    return;
                }

                UserData.SetSession(session);

                // Надёжно получаем uid из установленной сессии
                var uid = session.User.Id;
                var profile = await SupabaseController.GetProfileAsync(uid);
                UserData.SetProfile(profile);

                var window = new MainWindow();
                window.Show();
                Application.Current.MainWindow.Close();
            }
            catch (Exception ex)
            {
                // Показываем реальную ошибку для диагностики, не пряча её общим сообщением
                MessageBox.Show("Ошибка авторизации: " + ex.Message);
            }
        }
    }
}
