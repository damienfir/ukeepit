using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.Backend.Null
{
    public class Account : Backend.Account
    {
        public Account(AccountStore store, Hash identityHash) : base(store, identityHash) { }

        public override Backend.Root Root(string name)
        {
            return new Root(this, name);
        }

        public override void Delete(DeleteResult handler) { handler(true); }
    }
}
