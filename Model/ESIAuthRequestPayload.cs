using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVEAutoInvite.Model
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

        public string BuildURL(string endpoint)
        {
            return $"{endpoint}?response_type={this.ResponseType}&redirect_uri={this.RedirectURI}&client_id={this.ClientID}&scope={this.Scope}&code_challenge={this.CodeChallenge}&code_challenge_method={this.CodeChallengeMethod}&state={this.RequestState}";
        }
    }
}
