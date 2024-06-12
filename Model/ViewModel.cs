using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;

namespace EVEAutoInvite
{
    public class ViewModel
    {
        public static ObservableCollection<ESIAuthenticatedCharacter> Characters => ESIAuthManager.Characters;
        public static ObservableCollection<LogHeader> Logs => EVELogManager.Logs;

        private static ICollectionView _filteredLogs;

        static ViewModel()
        {
            // Subscribe to the ActiveCharacterChanged event
            ESIAuthManager.OnActiveCharacterChanged += OnActiveCharacterChanged;

            // Initialize the filtered logs collection view
            _filteredLogs = CollectionViewSource.GetDefaultView(Logs);
            _filteredLogs.Filter = FilterLogs;
        }

        public static ICollectionView CharacterLogs => _filteredLogs;

        private static async void OnActiveCharacterChanged(object sender, ESIAuthenticatedCharacter? e)
        {
            Debug.WriteLine($"Active character changed: {ESIAuthManager.ActiveCharacter?.CharacterInfo.CharacterName}");
            await EVELogManager.RefreshLogHeadersAsync();
            RefreshView();
        }

        private static void RefreshView()
        {
            Debug.WriteLine("Refreshing view...");
            foreach (var log in Logs)
            {
                Debug.WriteLine($"LOG: Listener={log.Listener}, ChannelName={log.ChannelName}");
            }
            _filteredLogs.Refresh();
        }

        private static bool FilterLogs(object item)
        {
            if (item is LogHeader log)
            {
                var activeCharacter = ESIAuthManager.ActiveCharacter;
                if (activeCharacter.HasValue)
                {
                    string characterName = activeCharacter.Value.CharacterInfo.CharacterName;
                    bool isMatch = log.Listener == characterName;
                    
                    Debug.WriteLine($"Filtering log: '{log.Listener}' == '{characterName}' => {isMatch}");
                    return isMatch;
                }
            }
            return false;
        }
    }
}
