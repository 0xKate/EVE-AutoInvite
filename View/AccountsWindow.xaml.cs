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
                    App.AuthManager.SaveCharacters();
                }
                else
                {
                    e.Cancel = true;
                }                
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
                _cancellationTokenSource = new CancellationTokenSource();
                ESIAuthenticatedCharacter? character = await App.AuthManager.RequestNewSSOAuth(_cancellationTokenSource.Token);

                if (character.HasValue)
                {
                    if (!App.AuthManager.UpdateCharacter(character.Value))
                    {
                        App.AuthManager.Characters.Add(character.Value);
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
            App.AuthManager.Characters.Remove(character); // Assuming Characters is your ObservableCollection or similar collection
        }

    }
}
