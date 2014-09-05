using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.Backend
{
    public abstract class Account
    {
        public readonly AccountStore Store;
        public readonly Hash IdentityHash;

        public Account(AccountStore store, Hash identityHash)
        {
            this.Store = store;
            this.IdentityHash = identityHash;
        }

        public abstract Root Root(string name);

        // Returns true if the account does not exist when the function returns. The return value is informational. By the time the function returns, the account may have been created again.
        public delegate void DeleteResult(bool result);
        public abstract void Delete(DeleteResult handler);
    }
}
