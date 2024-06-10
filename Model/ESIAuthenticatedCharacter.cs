using System.Runtime.Serialization;

namespace EVEAutoInvite
{
    [DataContract]
    public struct ESIAuthenticatedCharacter
    {
        [DataMember]
        public ESIAuthToken AuthToken { get; set; }

        [DataMember]
        public ESICharacterInfo CharacterInfo { get; set; }

        [DataMember]
        public bool Active { get; set; }
    }
}
