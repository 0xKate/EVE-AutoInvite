using System.Windows;

namespace EVEAutoInvite
{
    public partial class App : Application
    {

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Create and show the main window
            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
