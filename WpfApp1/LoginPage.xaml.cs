using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
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
                UserData.SetSession(await App._Supabase!.Auth.SignIn(EmailTextBox.Text!, PasswordBox.Password!));
                if (UserData.GetSession().User != null) MessageBox.Show("Отлично");
            }
            catch
            {
                MessageBox.Show("Неверные данные");
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
