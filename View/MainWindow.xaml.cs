using System.Windows;
using System.Windows.Controls;

namespace EVEAutoInvite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HelpWindow _helpWindow;
        private AccountsWindow _accountsWindow;


        public MainWindow()
        {
            InitializeComponent();

            CharSelector.IsEnabled = false;
            ChatSelector.IsEnabled = false;
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = false;

            // Generate List for CharSelector
            // -> List
            //      List{ValidAuthChar1, ValidAuthChar2, ValidAuthChar3}
            // Generate List for ChatSelector
            // -> 
            //      only the channel name of the latest log with the listener as selected character 
            //      List{ChannelName1(ChannelName1, ChannelName2, ChannelName3}

            // If last selected character exists
            // then ->
            //      If character has auth token
            //      then ->
            //          Check if auth token is valid
            //          then ->
            //              set CharSelector = ValidAuthedCharacter
            //              set CharSelector.IsEnabled = true
            //              Check if last chat channel selected exists
            //              then ->
            //                  set ChatSelector = latest log based on saved chat channel name
            //                  set ChatSelector.IsEnabled = true
            //                  set StartButton.IsEnabled = true
            //                  set StopButton.IsEnabled = true
            //
            // 

        }

        private void Accounts_Click(object sender, RoutedEventArgs e)
        {
            if (_accountsWindow == null || !_accountsWindow.IsVisible)
            {
                _accountsWindow = new AccountsWindow
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                _accountsWindow.Closed += (s, args) => _accountsWindow = null;
                _accountsWindow.Show();
            }
            else
            {
                _accountsWindow.Focus();
            }
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            if (_helpWindow == null || !_helpWindow.IsVisible)
            {
                _helpWindow = new HelpWindow
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                _helpWindow.Closed += (s, args) => _helpWindow = null;
                _helpWindow.Show();
            }
            else
            {
                _helpWindow.Focus();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CharSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // When the character changes, we need to rebuild the Chat list with all logs
            // that have the new character as a listener
            // enable the chat selector if we found any
        }

        private void ChatSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // When the selected chat changes, we need to create the log listener for the selected file
            // setup the event handler for when a new line is added
            // enable the start and stop buttons
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            // Enables all event handling
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            // disables all event handling
        }
    }
}
