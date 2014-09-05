using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.Backend
{
    // Methods are called from the main thread, and must not block, but call the handler (on the main thread) when done.
    // More than one method may be called at the same time.
    public abstract class AccountStore
    {
        // Two stores with the same ReferenceUrl are supposed to be the same.
        public readonly string Url;

        // Returns a list of accounts.
        public delegate void AccountsResult(IEnumerable<Account> result);
        public abstract void Accounts(Dictionary<string, string> query, AccountsResult handler);

        public abstract Account Account(Hash identityHash);

        public AccountStore(string url)
        {
            this.Url = url;
        }
    }
}
