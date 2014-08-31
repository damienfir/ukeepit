using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.HTTP
{
    class AccountStore : Abstract.AccountStore
    {
        public override void Accounts(Dictionary<string, string> query, AccountsResult handler)
        {
            throw new NotImplementedException();
        }

        public override Abstract.Account Account(Hash identityHash)
        {
            throw new NotImplementedException();
        }

    }
}
