using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.Backend.HTTP
{
    class Account : Backend.Account
    {
        public Account(AccountStore store, Hash identityHash) : base(store, identityHash) { }

        public override Backend.Root Root(string name)
        {
            throw new NotImplementedException();
        }

        public override void Delete(DeleteResult handler)
        {
            throw new NotImplementedException();
        }
    }
}
