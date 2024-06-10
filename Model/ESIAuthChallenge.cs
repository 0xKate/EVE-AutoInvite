namespace EVEAutoInvite
{
    public struct ESIAuthChallenge
    {
        public string CodeVerifier { get; set; }
        public string CodeChallenge { get; set; }
        public string RequestState { get; set; }
    
    }



}
