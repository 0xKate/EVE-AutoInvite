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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace EVEAutoInvite
{
    internal class EVELogManager
    {
        public static ObservableCollection<LogHeader> Logs { get; set; }
        public static event EventHandler<string> OnChatLogReceived;
        public static event EventHandler<bool> OnChatLogsRefreshed;
        private static FileSystemWatcher CurrentFile { get; set; }
        static EVELogManager()
        {
            Logs = new ObservableCollection<LogHeader>();
        }
        public static string GetChatLogsDirectory()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string eveLogsPath = Path.Combine(documentsPath, @"EVE\logs\Chatlogs");
            return eveLogsPath;
        }



        private static string ExtractValue(string line)
        {
            return line.Substring(line.IndexOf(':') + 1).Trim();
        }
        public static async Task<LogHeader> ReadLogHeaderAsync(string filePath)
        {
            LogHeader logHeader = new LogHeader();
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (line.Contains("Channel ID:"))
                        {
                            logHeader.ChannelID = ExtractValue(line);
                        }
                        else if (line.Contains("Channel Name:"))
                        {
                            logHeader.ChannelName = ExtractValue(line);
                        }
                        else if (line.Contains("Listener:"))
                        {
                            logHeader.Listener = ExtractValue(line);
                        }
                        else if (line.Contains("Session started:"))
                        {
                            logHeader.SessionStarted = DateTime.Parse(ExtractValue(line));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading log file: {ex.Message}");
            }
            return logHeader;
        }
        private static void OnLogFileUpdate(object sender, FileSystemEventArgs e)
        {
            // Read the new lines from the file
            using (FileStream fs = new FileStream(CurrentFile.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                fs.Seek(0, SeekOrigin.End);  // Move to the end of the file
                using (StreamReader reader = new StreamReader(fs, Encoding.UTF8))
                {
                    while (!reader.EndOfStream)
                    {
                        string newLine = reader.ReadLine();
                        if (!string.IsNullOrEmpty(newLine))
                        {
                            OnChatLogReceived?.Invoke(null, newLine);
                        }
                    }
                }
            }
        }
        public static void StartWatchLogFile(string filePath)
        {
            if (CurrentFile != null)
            {
                CurrentFile.Changed -= OnLogFileUpdate;
            }
            
            CurrentFile = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(filePath),
                Filter = Path.GetFileName(filePath),
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            CurrentFile.Changed += OnLogFileUpdate;
        }
        public static void StopWatchLogFile()
        {
            if (CurrentFile != null)
                CurrentFile.Changed -= OnLogFileUpdate; 
        }
        public static async Task RefreshLogHeadersAsync()
        {
            // Nested dictionary: { Listener: { ChannelName: LogHeader } }
            Dictionary<string, Dictionary<string, LogHeader>> latestLogsByUser = new Dictionary<string, Dictionary<string, LogHeader>>();

            foreach (string file in Directory.GetFiles(GetChatLogsDirectory()))
            {
                if (File.GetLastWriteTime(file) > DateTime.Now.AddHours(-24))
                {
                    LogHeader logHeader = await ReadLogHeaderAsync(file);
                    Debug.WriteLine($"Read Log: {logHeader.Listener}, Channel: {logHeader.ChannelName}, SessionStarted: {logHeader.SessionStarted}");

                    // Ensure we have a dictionary for the listener
                    if (!latestLogsByUser.TryGetValue(logHeader.Listener, out var userLogs))
                    {
                        userLogs = new Dictionary<string, LogHeader>();
                        latestLogsByUser[logHeader.Listener] = userLogs;
                    }

                    // Check if the dictionary already contains a log for the same channel
                    if (userLogs.TryGetValue(logHeader.ChannelName, out var existingLog))
                    {
                        // Compare the session start time of the existing log with the new log
                        if (logHeader.SessionStarted > existingLog.SessionStarted)
                        {
                            // Replace the existing log with the new log
                            userLogs[logHeader.ChannelName] = logHeader;
                            Debug.WriteLine($"Replaced Log for Channel: {logHeader.ChannelName}, Listener: {logHeader.Listener}");
                        }
                    }
                    else
                    {
                        // Add the new log if it's the first log for this channel
                        userLogs[logHeader.ChannelName] = logHeader;
                        Debug.WriteLine($"Added Log for Channel: {logHeader.ChannelName}, Listener: {logHeader.Listener}");
                    }
                }
            }

            Debug.WriteLine($"Total Logs in latestLogsByUser: {latestLogsByUser.Count}");

            // Update the collection with the latest logs
            Logs.Clear();
            foreach (var userLogs in latestLogsByUser.Values)
            {
                foreach (var logHeader in userLogs.Values)
                {
                    Debug.WriteLine($"Adding to ObservableCollection: {logHeader.Listener}, Channel: {logHeader.ChannelName}");
                    Logs.Add(logHeader);
                }
            }

            OnChatLogsRefreshed?.Invoke(null, true);
        }

    }
}
