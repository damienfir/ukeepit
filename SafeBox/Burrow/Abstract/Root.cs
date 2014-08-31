using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.Abstract
{
    public abstract class Root
    {
        public readonly Account Account;
        public readonly string Name;

        public Root(Account account, string name)
        {
            this.Account = account;
            this.Name = name;
        }

        // Returns a list of current object urls. Returns an empty list if the root is empty, or if an error occurred. Note that this function must never return null.
        public delegate void ListResult(IEnumerable<ObjectUrl> result) ;
        public abstract void List(ListResult handler);

        // Modifies the list of object urls. Returns false if one or more hashes could not be added, and true otherwise.
        public delegate void PostResult(bool result);
        public abstract void Post(IEnumerable<ObjectUrl> add, IEnumerable<Hash> remove, UnlockedPrivateIdentity identity, PostResult handler);
    }
}
