using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVEAutoInvite
{
    public struct ESIAuthToken
    {
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; }
        public string RefreshToken { get; set; }
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
