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

using EVEAutoInvite.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace EVEAutoInvite
{
    public class ESIAuthManager
    {
        public static ObservableCollection<ESIAuthenticatedCharacter> Characters { get; private set; }
        public static event EventHandler<ESIAuthenticatedCharacter?> OnActiveCharacterChanged;
        private static ESIAuthenticatedCharacter? _activeCharacter;
        public static ESIAuthenticatedCharacter? ActiveCharacter
        {
            get => _activeCharacter;
            set
            {
                _activeCharacter = value;
                OnActiveCharacterChanged?.Invoke(null, value);
            }
        }
        public static HttpClient WebClient { get; private set; }
        static ESIAuthManager()
        {
            Characters = new ObservableCollection<ESIAuthenticatedCharacter>();
            WebClient = new HttpClient();
            WebClient.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.UserAgent);
        }
        public static void SetAuthToken(ESIAuthToken token)
        {
            WebClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        }
        public static async Task<HttpResponseMessage> ESIAuthenticatedRequest(string url, ESIAuthToken authToken, CancellationToken cancellationToken = default)
        {
            try
            {
                SetAuthToken(authToken);
                var response = await WebClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();
                return response;
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    MessageBox.Show("Operation was canceled.", "Request Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}", "Error during SSO Auth!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return null;
        }
        public static bool LoadCharacters()
        {
            if (File.Exists(Constants.AuthBackupFile))
            {
                try
                {
                    var characters = JsonHelper.DeserializeFromFile<ObservableCollection<ESIAuthenticatedCharacter>>(Constants.AuthBackupFile);
                    Characters = characters;

                    if (ActiveCharacter == null)
                    {
                        foreach (var character in Characters)
                        {
                            if (character.Active)
                            {
                                ActiveCharacter = character;
                                break;
                            }
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}", "Error during LoadCharacters!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return false;
        }
        public static void SaveCharacters()
        {
            if (Characters.Count > 0)
            {
                try
                {
                    JsonHelper.SerializeToFile(Characters, Constants.AuthBackupFile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}", "Error during SaveCharacters!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public static bool TryGetCharacter(string characterName, out ESIAuthenticatedCharacter? character, out int? index)
        {
            character = null;
            index = null;

            for (int i = 0; i < Characters.Count; i++)
            {
                var c = Characters[i];
                if (c.CharacterInfo.CharacterName == characterName)
                {
                    character = Characters[i];
                    index = i;
                    return true;
                }
            }
            return false;
        }
        public static bool UpdateCharacter(ESIAuthenticatedCharacter newCharacter)
        {
            if (TryGetCharacter(newCharacter.CharacterInfo.CharacterName, out ESIAuthenticatedCharacter? _, out int? i))
            {
                Characters[i.Value] = newCharacter;
                return true;
            }
            return false;
        }
        public static void OpenWebBrowser(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        public static ESIAuthChallenge GenerateNewAuthChallenge()
        {
            string codeVerifier;
            string codeChallenge;

            byte[] randomBytes = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            codeVerifier = JsonHelper.Base64UrlEncode(randomBytes);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                codeChallenge = JsonHelper.Base64UrlEncode(hashBytes);
            }

            return new ESIAuthChallenge()
            {
                CodeChallenge = codeChallenge,
                CodeVerifier = codeVerifier,
                RequestState = Guid.NewGuid().ToString()
            };
        }
        public static async Task<ESIAuthenticatedCharacter?> RequestNewSSOAuth(CancellationToken cancellationToken = default)
        {
            ESIAuthChallenge esiAuthChallenge = GenerateNewAuthChallenge();
            ESICharacterInfo esiCharacterInfo;
            ESIAuthToken esiAuthToken;

            try
            {
                var requestPayload = new ESIAuthRequestPayload()
                {
                    ResponseType = "code",
                    RedirectURI = HttpUtility.UrlEncode(Constants.EndpointCallbackURL),
                    ClientID = Constants.ClientID,
                    Scope = HttpUtility.UrlEncode(Constants.ESIScopes),
                    CodeChallenge = esiAuthChallenge.CodeChallenge,
                    CodeChallengeMethod = "S256",
                    RequestState = esiAuthChallenge.RequestState,
                };

                var server = new WebServer(Constants.EndpointCallbackURL);
                Task<HttpListenerRequest> listenerTask = server.StartAsync(cancellationToken);
                OpenWebBrowser(requestPayload.BuildURL());

                HttpListenerRequest request = await listenerTask.WithCancellation(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                NameValueCollection queryParameters = HttpUtility.ParseQueryString(request.Url.Query);

                var code = queryParameters.Get("code");
                var state = queryParameters.Get("state");

                if (state == esiAuthChallenge.RequestState)
                {
                    var payload = new Dictionary<string, string>
                    {
                        {"grant_type", "authorization_code"},
                        {"code", code},
                        {"client_id", Constants.ClientID },
                        {"code_verifier", esiAuthChallenge.CodeVerifier},
                    };

                    // Make post request 
                    HttpResponseMessage authTokenRaw = await WebClient.PostAsync(
                        Constants.EndpointOAuthToken,
                        new FormUrlEncodedContent(payload),
                        cancellationToken);
                    authTokenRaw.EnsureSuccessStatusCode();

                    // Deserialize the json payload
                    esiAuthToken = await JsonHelper.Deserialize<ESIAuthToken>(authTokenRaw.Content, cancellationToken);

                    // Make a request to the ESI /verify as an authenticated user
                    var characterInfoRaw = await ESIAuthenticatedRequest(Constants.EndpointOAuthVerify, esiAuthToken, cancellationToken);

                    // Deserialize the json payload
                    esiCharacterInfo = await JsonHelper.Deserialize<ESICharacterInfo>(characterInfoRaw.Content, cancellationToken);


                    return new ESIAuthenticatedCharacter()
                    {
                        AuthToken = esiAuthToken,
                        CharacterInfo = esiCharacterInfo,
                        Active = false
                    };
                }
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    MessageBox.Show("Operation was canceled.", "SSO Auth Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}", "Error during SSO Auth!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return null;
        }

    }
    public static class TaskExtensions
    {
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task))
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }
            return await task;
        }
    }
}