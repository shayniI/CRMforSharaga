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

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            var url = "https://quqvkrzznvxuiautqyin.supabase.co";
            var key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InF1cXZrcnp6bnZ4dWlhdXRxeWluIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjAyNDM1NzgsImV4cCI6MjA3NTgxOTU3OH0.w1Vc3hoQharEA9IpCCQNRLJYSB--2Gy1FRVG4d-VEYg";

            var option = new SupabaseOptions
            {
                AutoConnectRealtime = true,
            };
            _Supabase = new Client(url, key, option);
            await _Supabase.InitializeAsync();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var errorMsg = e.Exception.Message;
            if (e.Exception.InnerException != null)
                errorMsg += "\n" + e.Exception.InnerException.Message;
            MessageBox.Show("Необработанная ошибка: " + errorMsg, "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                var errorMsg = ex.Message;
                if (ex.InnerException != null)
                    errorMsg += "\n" + ex.InnerException.Message;
                MessageBox.Show("Необработанная ошибка домена: " + errorMsg, "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            var errorMsg = e.Exception.Message;
            if (e.Exception.InnerException != null)
                errorMsg += "\n" + e.Exception.InnerException.Message;
            MessageBox.Show("Необработанная ошибка задачи: " + errorMsg, "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            e.SetObserved();
        }
    }

}
