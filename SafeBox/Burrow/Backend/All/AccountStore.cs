using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.Backend.All
{
    class AccountStore : Backend.AccountStore
    {
        public static AccountStore For(IEnumerable<Backend.AccountStore> accountStores, int minSuccessful)
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

            return new AccountStore(url, minPriority, stores, minSuccessful);
        }

        // The (immutable) list of stores.
        private readonly Backend.AccountStore[] stores;

        // The minimum number of stores to succeed for a modify operation to be considered successful.
        private readonly int minSuccessful = -1;

        public AccountStore(string url, int priority, Backend.AccountStore[] stores, int minSuccessful)
            : base(url, priority)
        {
            this.stores = stores;
            this.minSuccessful = minSuccessful;
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
            var successful = 0;
            for (var i = 0; i < stores.Length; i++)
                if (stores[i].Modify(identityHash, rootLabel, add, remove, identity)) successful += 1;
            return successful > minSuccessful;
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

        public override TaskGroup.Future<bool> Modify(Hash identityHash, string rootLabel, IEnumerable<ObjectUrl> add, IEnumerable<Hash> remove, Configuration.PrivateIdentity identity, TaskGroup taskGroup)
        {
            var group = new TaskGroup();
            var futures = new TaskGroup.Future<bool>[stores.Length];
            for (var i = 0; i < stores.Length; i++)
                futures[i] = stores[i].Modify(identityHash, rootLabel, add, remove, identity, group);

            var future = taskGroup.WaitForMe<bool>();
            group.WhenDone(() =>
            {
                var successful = 0;
                for (var i = 0; i < futures.Length; i++)
                    if (futures[i].Result) successful += 1;

                future.Done(successful >= minSuccessful);
            });

            return future;
        }
    }
}
