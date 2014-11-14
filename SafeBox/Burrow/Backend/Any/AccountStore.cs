using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.Backend.Any
{
    class AccountStore : Backend.AccountStore
    {
        public static AccountStore For(IEnumerable<Backend.AccountStore> accountStores)
        {
            if (accountStores == null) return null;

            var stores = accountStores.ToArray();
            if (stores.Length == 0) return null;
            Array.Sort(stores);

            var url = "Any(\n";
            var minPriority = 100;
            foreach (var store in stores)
            {
                url += store.Url + "\n";
                minPriority = Math.Min(minPriority, store.Priority);
            }
            url += ")\n";

            return new AccountStore(url, minPriority, stores);
        }

        // The (immutable) list of stores.
        private Backend.AccountStore[] stores;

        // This keeps state in a lenient way - it's no problem if different threads see a different version of this value.
        // Assuming that int assignment is atomic, we do not need to synchronize this any further.
        private int lastWritten = -1;

        public AccountStore(string url, int priority, Backend.AccountStore[] stores)
            : base(url, priority)
        {
            this.stores = stores;
        }

        public override IEnumerable<Hash> Accounts(Dictionary<string, string> query)
        {
            var accounts = new ImmutableStack<Hash>();
            for (var i = 0; i < stores.Length; i++)
                accounts = accounts.With(stores[i].Accounts(query));
            return accounts;
        }

        public override bool Delete(Hash identityHash)
        {
            var success = true;
            for (var i = 0; i < stores.Length; i++)
                success &= stores[i].Delete(identityHash);
            return success;
        }

        public override IEnumerable<ObjectUrl> List(Hash identityHash, string rootLabel)
        {
            var list = new ImmutableStack<ObjectUrl>();
            for (var i = 0; i < stores.Length; i++)
                list = list.With(stores[i].List(identityHash, rootLabel));
            return list;
        }

        public override bool Modify(Hash identityHash, string rootLabel, IEnumerable<ObjectUrl> add, IEnumerable<Hash> remove, Configuration.PrivateIdentity identity)
        {
            var last = lastWritten;
            for (var i = 0; i < stores.Length; i++)
            {
                last += 1;
                if (last >= stores.Length) last = 0;
                if (stores[last].Modify(identityHash, rootLabel, add, remove, identity))
                {
                    lastWritten = last;
                    return true;
                }
            }

            return false;
        }

        // *** Asynchronous versions ***

        public override TaskGroup.Future<IEnumerable<ObjectUrl>> List(Hash identityHash, string rootLabel, TaskGroup taskGroup)
        {
            var group = new TaskGroup();
            var futures = new TaskGroup.Future<IEnumerable<ObjectUrl>>[stores.Length];
            for (var i = 0; i < stores.Length; i++)
                futures[i] = stores[i].List(identityHash, rootLabel, group);

            var future = taskGroup.WaitForMe<IEnumerable<ObjectUrl>>();
            group.WhenDone(() =>
            {
                var list = new ImmutableStack<ObjectUrl>();
                for (var i = 0; i < futures.Length; i++)
                    list = list.With(futures[i].Result);
                future.Done(list);
            });

            return future;
        }
    }
}
