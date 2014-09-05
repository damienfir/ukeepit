using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.Backend.Null
{
    public class AccountStore : Backend.AccountStore
    {
        public AccountStore() : base("") { }

        public override void Accounts(Dictionary<string, string> query, AccountsResult handler) { handler(new List<Backend.Account>()); }

        public override Backend.Account Account(Hash identityHash) { return new Account(this, identityHash); }
    }
}
