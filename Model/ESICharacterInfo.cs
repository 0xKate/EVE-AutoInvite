using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVEAutoInvite
{
    public struct ESICharacterInfo
    {
        public string CharacterID { get; set; }
        public string CharacterName { get; set; }
        public string ExpiresOn { get; set; }
        public string Scopes { get; set; }
        public string TokenType { get; set; }
        public string CharacterOwnerHash { get; set; }
        public string IntellectualProperty { get; set; }
    }
}
