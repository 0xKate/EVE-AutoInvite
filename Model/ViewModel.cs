/*
EVEAutoInvite - A small utility for EVE Online
Copyright (C) 2024 github.com/0xKate

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

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
