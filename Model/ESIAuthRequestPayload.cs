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

namespace EVEAutoInvite
{
    public struct ESIAuthRequestPayload
    {
        public string ResponseType { get; set; }
        public string RedirectURI { get; set; }
        public string ClientID { get; set; }
        public string Scope { get; set; }
        public string CodeChallenge { get; set; }
        public string CodeChallengeMethod { get; set; }
        public string RequestState { get; set; } // aka. state

        public string BuildURL()
        {
            return $"{Constants.EndpointOAuthAuthorize}?response_type={this.ResponseType}&redirect_uri={this.RedirectURI}&client_id={this.ClientID}&scope={this.Scope}&code_challenge={this.CodeChallenge}&code_challenge_method={this.CodeChallengeMethod}&state={this.RequestState}";
        }
    }
}
