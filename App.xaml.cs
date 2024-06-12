using System.Windows;

namespace EVEAutoInvite
{
    public partial class App : Application
    {
        public static ESIAuthManager AuthManager { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Load Authenticated Characters
            ESIAuthManager.LoadCharacters();

            // Parse all logs and fetch their header.
            _ = EVELogManager.RefreshLogHeadersAsync();

            // Create and show the main window
            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ESIAuthManager.SaveCharacters();

            base.OnExit(e);
        }
    }
}
