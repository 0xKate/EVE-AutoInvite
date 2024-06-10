using System.Collections.ObjectModel;
using System.Windows;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;

namespace EVEAutoInvite
{
    public partial class App : Application
    {
        public ObservableCollection<ESIAuthenticatedCharacter> AuthenticatedCharacters { get; private set; }
        public readonly string AuthBackupFile = "Saved-Characters.json";

        public async Task<bool> LoadCharactersAsync()
        {
            if (File.Exists(this.AuthBackupFile))
            {
                using (StreamReader reader = new StreamReader(this.AuthBackupFile))
                {
                    string json = await reader.ReadToEndAsync();
                    if (json.Length > 2)
                    {
                        this.AuthenticatedCharacters = JsonSerializer.Deserialize<ObservableCollection<ESIAuthenticatedCharacter>>(json);
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task SaveCharactersAsync()
        {
            string json = JsonSerializer.Serialize(this.AuthenticatedCharacters);
            using (StreamWriter writer = new StreamWriter(this.AuthBackupFile))
            {
                await writer.WriteAsync(json);
            }
        }


        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Load the Authenticated Characters from file
            await this.LoadCharactersAsync();            

            // Create and show the main window
            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await this.SaveCharactersAsync();

            base.OnExit(e);
        }
    }
}
