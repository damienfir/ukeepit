using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Configuration;

namespace SafeBox.Burrow.Backend
{
    // Methods are called from the main thread, and must not block, but call the handler (on the main thread) when done.
    // More than one method may be called at the same time.
    public abstract class AccountStore
    {
        // Two stores with the same url are supposed to be the same.
        public readonly string Url;

        // A lower value means that the store can be accessed more easily, or faster, and should therefore be preferred.
        public readonly int Priority;

        // Returns a list of accounts.
        public abstract IEnumerable<Hash> Accounts(Dictionary<string, string> query);

        // Returns true if the account does not exist when the function returns. The return value is informational. By the time the function returns, the account may have been created again.
        public abstract bool Delete(Hash identityHash);

        // Returns a list of current object urls. Returns an empty list if the root is empty, or if an error occurred. Note that this function must never return null.
        public abstract IEnumerable<ObjectUrl> List(Hash identityHash, string rootLabel);

        // Modifies the list of object urls. Returns false if one or more hashes could not be added, and true otherwise.
        public abstract bool Modify(Hash identityHash, string rootLabel, IEnumerable<ObjectUrl> add, IEnumerable<Hash> remove, PrivateIdentity identity);

        // Asynchronous interface
        public TaskGroup.Future<IEnumerable<Hash>> Accounts(Dictionary<string, string> query, TaskGroup whenDone) { return whenDone.Run(() => Accounts(query)); }
        public TaskGroup.Future<bool> Delete(Hash identityHash, TaskGroup whenDone) { return whenDone.Run(() => Delete(identityHash)); }
        public TaskGroup.Future<IEnumerable<ObjectUrl>> List(Hash identityHash, string rootLabel, TaskGroup whenDone) { return whenDone.Run(() => List(identityHash, rootLabel)); }
        public TaskGroup.Future<bool> Modify(Hash identityHash, string rootLabel, IEnumerable<ObjectUrl> add, IEnumerable<Hash> remove, PrivateIdentity identity, TaskGroup whenDone) { return whenDone.Run(() => Modify(identityHash, rootLabel, add, remove, identity)); }

        public AccountStore(string url, int priority)
        {
            this.Url = url;
            this.Priority = priority;
        }
    }
}
