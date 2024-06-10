using System.Runtime.Serialization;


/*
"CharacterID":95465499,
"CharacterName":"CCP Bartender",
"ExpiresOn":"2017-07-05T14:34:16.5857101",
"Scopes":"esi-characters.read_standings.v1",
"TokenType":"Character",
"CharacterOwnerHash":"lots_of_letters_and_numbers",
"IntellectualProperty":"EVE"}
*/

namespace EVEAutoInvite
{
    [DataContract]
    public struct ESICharacterInfo
    {
        [DataMember]
        public string CharacterID { get; set; }

        [DataMember]
        public string CharacterName { get; set; }

        [DataMember]
        public string ExpiresOn { get; set; }

        [DataMember]
        public string Scopes { get; set; }

        [DataMember]
        public string TokenType { get; set; }

        [DataMember]
        public string CharacterOwnerHash { get; set; }

        [DataMember]
        public string IntellectualProperty { get; set; }
    }
}
