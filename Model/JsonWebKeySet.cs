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
using System.Runtime.Serialization;

namespace EVEAutoInvite
{

    [DataContract]
    public struct JsonWebKeySet
    {
        [DataMember(Name = "keys")]
        public JsonWebKey[] Keys { get; set; }

        [DataMember(Name = "SkipUnresolvedJsonWebKeys")]
        public bool SkipUnresolvedJsonWebKeys { get; set; }
    }

    [DataContract]
    public struct JsonWebKey
    {
        [DataMember(Name = "alg")]
        public string Alg { get; set; }

        [DataMember(Name = "e", IsRequired = false)]
        public string E { get; set; }

        [DataMember(Name = "kid")]
        public string Kid { get; set; }

        [DataMember(Name = "kty")]
        public string Kty { get; set; }

        [DataMember(Name = "n", IsRequired = false)]
        public string N { get; set; }

        [DataMember(Name = "use")]
        public string Use { get; set; }

        [DataMember(Name = "crv", IsRequired = false)]
        public string Crv { get; set; }

        [DataMember(Name = "x", IsRequired = false)]
        public string X { get; set; }

        [DataMember(Name = "y", IsRequired = false)]
        public string Y { get; set; }
    }
}
