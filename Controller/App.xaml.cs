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
