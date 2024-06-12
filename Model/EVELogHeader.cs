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
