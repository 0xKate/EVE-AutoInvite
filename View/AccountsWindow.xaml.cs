using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace EVEAutoInvite
{
    public partial class AccountsWindow : Window
    {
        public ObservableCollection<ESIAuthenticatedCharacter> Characters { get; set; }

        public AccountsWindow()
        {
            InitializeComponent();
            Characters = new ObservableCollection<ESIAuthenticatedCharacter>();

            // Fetch list of loaded Characters from ESIManager

        }

        private void AddCharacter_Click(object sender, RoutedEventArgs e)
        {
            // Starts new SSO Auth from ESIManager
        }
    }
}