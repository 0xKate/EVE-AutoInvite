﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;

namespace EVEAutoInvite
{
    public class ESIAuthManager
    {
        public HttpClient HttpClient { get; set; }
        public ObservableCollection<ESIAuthenticatedCharacter> Characters { get; private set; }
        public ESIAuthenticatedCharacter? ActiveCharacter { get; private set; }
        public static ESIAuthManager Current { get; set; }
        public static ESIAuthManager GetAuthManager()
        {
            if (Current == null)
                Current = new ESIAuthManager();
            return Current;
        }       
        public ESIAuthManager()
        {
            this.HttpClient = new HttpClient();
            this.Characters = new ObservableCollection<ESIAuthenticatedCharacter>();
            this.HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.AuthBackupFile);
        }        
        public void SetAuthToken(ESIAuthToken token)
        {
            this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        }
        public void SetActiveCharacter(string characterName)
        {
            foreach (var character in this.Characters)
            {
                if (character.CharacterInfo.CharacterName == characterName)
                {
                    this.ActiveCharacter = character;
                    break;
                }
            }
        }
        public void SetActiveCharacter(ESIAuthenticatedCharacter character)
        {
            this.ActiveCharacter = character;
        }
        public async Task<HttpResponseMessage> ESIAuthenticatedRequest(string url, ESIAuthToken authToken)
        {
            try
            {
                this.SetAuthToken(authToken);
                var response = await this.HttpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return response;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}", "Error during SSO Auth!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }
        public bool LoadCharacters()
        {
            if (File.Exists(Constants.AuthBackupFile))
            {
                try
                {
                    using (FileStream fileStream = new FileStream(Constants.AuthBackupFile, FileMode.Open))
                    {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ObservableCollection<ESIAuthenticatedCharacter>));
                        this.Characters = (ObservableCollection<ESIAuthenticatedCharacter>) serializer.ReadObject(fileStream);

                        if (this.ActiveCharacter == null)
                        {
                            foreach (var character in this.Characters)
                            {
                                if (character.Active)
                                {
                                    this.ActiveCharacter = character;
                                    break;
                                }
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
        public void SaveCharacters()
        {
            if (this.Characters.Count > 0)
            {
                try
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ObservableCollection<ESIAuthenticatedCharacter>));
                        serializer.WriteObject(stream, Characters);
                        File.WriteAllBytes(Constants.AuthBackupFile, stream.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}", "Error during SaveCharacters!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public bool TryGetCharacter(string characterName, out ESIAuthenticatedCharacter? character, out int? index)
        {
            character = null;
            index = null;

            for (int i = 0; i < this.Characters.Count; i++)
            {
                var c = this.Characters[i];
                if (c.CharacterInfo.CharacterName == characterName)
                {
                    character = this.Characters[i];
                    index = i;
                    return true;
                }
            }
            return false;            
        }
        public bool UpdateCharacter(ESIAuthenticatedCharacter newCharacter)
        {
            if (this.TryGetCharacter(newCharacter.CharacterInfo.CharacterName, out ESIAuthenticatedCharacter? _, out int? i))
            {
                this.Characters[i.Value] = newCharacter;
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
        public static string Base64UrlEncode(byte[] input)
        {
            string base64 = Convert.ToBase64String(input);
            string base64Url = base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
            return base64Url;
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

            codeVerifier = Base64UrlEncode(randomBytes);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                codeChallenge = Base64UrlEncode(hashBytes);
            }

            return new ESIAuthChallenge()
            {
                CodeChallenge = codeChallenge,
                CodeVerifier = codeVerifier,
                RequestState = Guid.NewGuid().ToString()
            };
        }
        public async Task<ESIAuthenticatedCharacter?> RequestNewSSOAuth()
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
                Task<HttpListenerRequest> listenerTask = server.StartAsync();
                OpenWebBrowser(requestPayload.BuildURL());

                HttpListenerRequest request = await listenerTask;
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

                    FormUrlEncodedContent content = new FormUrlEncodedContent(payload);
                    HttpResponseMessage authTokenRaw = await this.HttpClient.PostAsync(Constants.EndpointOAuthToken, content);

                    authTokenRaw.EnsureSuccessStatusCode();
                    using (var stream = await authTokenRaw.Content.ReadAsStreamAsync())
                    {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ESIAuthToken));
                        esiAuthToken = (ESIAuthToken)serializer.ReadObject(stream);
                    }

                    var characterInfoRaw = await ESIAuthenticatedRequest(Constants.EndpointOAuthVerify, esiAuthToken);
                    using (var stream = await characterInfoRaw.Content.ReadAsStreamAsync())
                    {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ESICharacterInfo));
                        esiCharacterInfo = (ESICharacterInfo)serializer.ReadObject(stream);
                    }

                    return new ESIAuthenticatedCharacter()
                    {
                        AuthToken = esiAuthToken,
                        CharacterInfo = esiCharacterInfo
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}", "Error during SSO Auth!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }
    }
}