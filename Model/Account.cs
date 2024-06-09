using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVEAutoInvite
{
    public struct Account
    {
        public ESIAuthToken AuthToken { get; set; }
        public ESICharacterInfo CharacterInfo { get; set; }
    }
}
