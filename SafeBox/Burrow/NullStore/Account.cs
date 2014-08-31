using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.NullStore
{
    public class Account : Abstract.Account
    {
        public Account(AccountStore store, Hash identityHash) : base(store, identityHash) { }

        public override Abstract.Root Root(string name)
        {
            return new Root(this, name);
        }

        public override void Delete(DeleteResult handler) { handler(true); }
    }
}
