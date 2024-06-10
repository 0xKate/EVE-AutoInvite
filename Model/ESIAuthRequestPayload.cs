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
