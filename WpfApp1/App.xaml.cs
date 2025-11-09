using System.Configuration;
using System.Data;
using System.Windows;
using Supabase;

namespace WpfApp1
{
    public partial class App : Application
    {
        public static Client? _Supabase {  get; private set; }
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var url = "https://quqvkrzznvxuiautqyin.supabase.co";
            var key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InF1cXZrcnp6bnZ4dWlhdXRxeWluIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjAyNDM1NzgsImV4cCI6MjA3NTgxOTU3OH0.w1Vc3hoQharEA9IpCCQNRLJYSB--2Gy1FRVG4d-VEYg";

            var option = new SupabaseOptions
            {
                AutoConnectRealtime = true,
            };
            _Supabase = new Client(url, key, option);
            await _Supabase.InitializeAsync();
        }
    }

}
