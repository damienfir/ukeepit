using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.Backend.HTTP
{
    class AccountStore : Backend.AccountStore
    {
        public static AccountStore ForUrl(string url) { return null; }

        public override void Accounts(Dictionary<string, string> query, AccountsResult handler)
        {
            throw new NotImplementedException();
        }

        public override Backend.Account Account(Hash identityHash)
        {
            throw new NotImplementedException();
        }

    }
}
