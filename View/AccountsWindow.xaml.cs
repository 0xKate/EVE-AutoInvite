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

using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace EVEAutoInvite
{
    public partial class AccountsWindow : Window
    {

        private CancellationTokenSource _cancellationTokenSource;
        private bool _processing = false;

        public AccountsWindow()
        {
            InitializeComponent();
        }

        public void OnClose(object sender, CancelEventArgs e)
        {
            if (_processing)
            {
                var choice = MessageBox.Show("Please finish your current SSO Authentication. (Check web browser)\n\n You may press cancel to cancel the current operation.", "Operation in Progress", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (choice == MessageBoxResult.Cancel)
                {
                    _cancellationTokenSource.Cancel();
                    ESIAuthManager.SaveCharacters();
                }
                else
                {
                    e.Cancel = true;
                }                
            }
            else
            {
                ESIAuthManager.SaveCharacters();
            }
        }

        private async void AddCharacter_Click(object sender, RoutedEventArgs e)
        {
            
            if (_processing == false)
            {
                _processing = true;
                _cancellationTokenSource = new CancellationTokenSource();
                ESIAuthenticatedCharacter? character = await ESIAuthManager.RequestNewSSOAuth(_cancellationTokenSource.Token);

                if (character.HasValue)
                {
                    if (!ESIAuthManager.UpdateCharacter(character.Value))
                    {
                        ESIAuthManager.Characters.Add(character.Value);
                    }
                }
                _processing = false;
            }
            else
            {
                var choice = MessageBox.Show("Please finish your current SSO Authentication. (Check web browser)\n\n You may press cancel to cancel the current operation.", "Operation in Progress", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (choice == MessageBoxResult.Cancel)
                {
                    _cancellationTokenSource.Cancel();
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the button that was clicked
            var button = (System.Windows.Controls.Button)sender;

            // Get the corresponding data item (the row's DataContext)
            var character = (ESIAuthenticatedCharacter)button.DataContext;

            // Now you can remove the character from your data source
            ESIAuthManager.Characters.Remove(character); // Assuming Characters is your ObservableCollection or similar collection
        }

    }
}
