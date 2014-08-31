using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.NullStore
{
    public class AccountStore : Abstract.AccountStore
    {
        public AccountStore(string reference, Dictionary settings) : base("") { }

        public override void Accounts(Dictionary<string, string> query, AccountsResult handler) { handler(new List<Abstract.Account>()); }

        public override Abstract.Account Account(Hash identityHash) { return new Account(this, identityHash); }
    }
}
