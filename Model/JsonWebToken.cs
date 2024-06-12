using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;


namespace EVEAutoInvite
{
    [DataContract]
    public class JsonWebToken
    {
        [DataMember(Name = "scp")] // ["esi-skills.read_skills.v1", "esi-skills.read_skillqueue.v1"]
        public string[] Scopes { get; set; }

        [DataMember(Name = "jti")] // "998e12c7-3241-43c5-8355-2c48822e0a1b"
        public string Jti { get; set; }

        [DataMember(Name = "kid")] // "JWT-Signature-Key",
        public string Kid { get; set; }

        [DataMember(Name = "sub")] // "CHARACTER:EVE:123123"
        public string Sub { get; set; }

        [DataMember(Name = "azp")] // "my3rdpartyclientid"
        public string Azp { get; set; }

        [DataMember(Name = "tenant")] // "tranquility"
        public string Tenant { get; set; }

        [DataMember(Name = "tier")] // "live"
        public string Tier { get; set; }

        [DataMember(Name = "region")] // "world"
        public string Region { get; set; }

        /// <summary>
        /// The aud claim contains the audience and must included both the client_id and the string value: "EVE Online"
        /// </summary>
        [DataMember(Name = "aud")] // ["my3rdpartyclientid", "EVE Online"]
        public string[] Audience { get; set; }

        [DataMember(Name = "name")] // "Some Bloke"
        public string Name { get; set; }

        [DataMember(Name = "owner")] // "8PmzCeTKb4VFUDrHLc/AeZXDSWM=",
        public string Owner { get; set; }

        /// <summary>
        /// The exp claim contains the expiry date of the token as a UNIX timestamp.
        /// You can use that to know when to refresh the token and to make sure that the token is valid.
        /// </summary>
        [DataMember(Name = "exp")] // 1648563218
        public long ExpirationDate { get; set; }

        [DataMember(Name = "iat")] // 1648562018,
        public long IssuedAt { get; set; }

        /// <summary>
        /// The issuer will either be the host name or the URI of the SSO instance you are using
        /// (e.g. "login.eveonline.com" or "https://login.eveonline.com").
        /// Your application should handle looking for both the host name and the URI in the iss claim
        /// and should reject any JWT tokens where iss does not equal the host name or URI of EVE’s SSO.
        /// </summary>
        [DataMember(Name = "iss")] // "login.eveonline.com"
        public string Issuer { get; set; }

        // Instance method to serialize to base64url safe encoded string
        public string Serialize()
        {
            using (var ms = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(JsonWebToken));
                serializer.WriteObject(ms, this);
                var jsonBytes = ms.ToArray();
                return Convert.ToBase64String(jsonBytes)
                              .Replace('+', '-')
                              .Replace('/', '_')
                              .TrimEnd('='); // Trim padding '=' characters
            }
        }

        // Static method to deserialize from base64url safe encoded string
        public static JsonWebToken Deserialize(string base64UrlSafeString)
        {
            // Add padding '=' characters if necessary
            int paddingNeeded = 4 - (base64UrlSafeString.Length % 4);
            if (paddingNeeded != 4)
            {
                base64UrlSafeString += new string('=', paddingNeeded);
            }

            // Replace '-' with '+' and '_' with '/' to make it standard base64
            base64UrlSafeString = base64UrlSafeString.Replace('-', '+').Replace('_', '/');

            byte[] jsonBytes = Convert.FromBase64String(base64UrlSafeString);

            using (var ms = new MemoryStream(jsonBytes))
            {
                var serializer = new DataContractJsonSerializer(typeof(JsonWebToken));
                return (JsonWebToken)serializer.ReadObject(ms);
            }
        }

        public void Validate()
        {
            // Validate the JWT signature - Not implemented as it requires external library for RS256 validation

            // Validate the issuer
            if (Issuer != Constants.JWKIssuersHost && Issuer != Constants.JWKIssuersURI)
            {
                throw new Exception("Invalid issuer");
            }

            // Validate the expiry date
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (ExpirationDate < currentTimestamp)
            {
                throw new Exception("Token has expired");
            }

            // Validate the audience claim
            bool clientIdFound = false;
            foreach (var aud in this.Audience)
            {
                if (aud == Constants.ClientID)
                {
                    clientIdFound = true;
                    break;
                }
            }
            if (!clientIdFound)
            {
                throw new Exception("Invalid audience");
            }
        }
    }
}
