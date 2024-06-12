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

namespace EVEAutoInvite
{
    public struct LogHeader
    {
        public string ChannelID { get; set; }
        public string ChannelName { get; set; }
        /// <summary>
        /// AKA CharacterName
        /// </summary>
        public string Listener { get; set; }
        public DateTime SessionStarted { get; set; }
        public string LogPath { get; set; }
        public override string ToString()
        {
            return $"File: {LogPath}\n - Channel ID: {ChannelID}\n - Channel Name: {ChannelName}\n - Listener: {Listener}\n - Session started: {SessionStarted}";
        }
    }
}
