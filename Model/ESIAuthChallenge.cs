using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVEAutoInvite
{
    public struct ESIAuthChallenge
    {
        public string CodeVerifier { get; set; }
        public string CodeChallenge { get; set; }
        public string RequestState { get; set; }
    }
}
