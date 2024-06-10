using System;
using System.Runtime.Serialization;

namespace EVEAutoInvite
{
    [DataContract]
    public struct ESIAuthToken
    {
        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }

        [DataMember(Name = "expires_in")]
        public int ExpiresIn { get; set; }

        [DataMember(Name = "token_type")]
        public string TokenType { get; set; }

        [DataMember(Name = "refresh_token")]
        public string RefreshToken { get; set; }

        [DataMember(Name = "expiration_date")]
        public DateTime ExpirationDate { get; set; }

        public bool Validate()
        {
            if (ExpiresIn <= 0)
                return false;

            if (ExpirationDate < DateTime.Now)
                return false;

            return true;
        }
    }
}
