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
using System.Runtime.Serialization;

namespace EVEAutoInvite
{
    [DataContract]
    public struct ESIJwt
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

        public void Validate()
        {
            // Validate the JWT signature - Not implemented as it requires external library for RS256 validation

            // Validate the issuer
            if (this.Issuer != Constants.JWKIssuersHost && this.Issuer != Constants.JWKIssuersURI)
            {
                throw new Exception("Invalid issuer");
            }

            // Validate the expiry date
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (this.ExpirationDate < currentTimestamp)
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
