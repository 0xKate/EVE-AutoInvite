using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace EVEAutoInvite
{
    public partial class AccountsWindow : Window
    {

        private bool _processing = false;

        public AccountsWindow()
        {
            InitializeComponent();
        }

        public void OnClose(object sender, CancelEventArgs e)
        {
            if (_processing)
            {
                MessageBox.Show("Please finish your current SSO Authentication. (Check web browser)", "Operation in Progress", MessageBoxButton.OK, MessageBoxImage.Warning);
                e.Cancel = true;
            }
            else
            {
                App.AuthManager.SaveCharacters();
            }
        }

        private async void AddCharacter_Click(object sender, RoutedEventArgs e)
        {
            if (_processing == false)
            {
                _processing = true;
                ESIAuthenticatedCharacter? character = await App.AuthManager.RequestNewSSOAuth();

                if (character.HasValue)
                {
                    if (!App.AuthManager.UpdateCharacter(character.Value))
                    {
                        App.AuthManager.Characters.Add(character.Value);
                    }
                }
                _processing = false;
            }
        }
    }
}