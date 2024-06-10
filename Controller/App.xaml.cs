using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Windows;

namespace EVEAutoInvite
{
    public partial class App : Application
    {
        public ESIAuthManager AuthManager { get; private set; }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AuthManager = new ESIAuthManager();
            AuthManager.LoadCharacters();


            // Create and show the main window
            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            AuthManager.SaveCharacters();

            base.OnExit(e);
        }
    }
}
