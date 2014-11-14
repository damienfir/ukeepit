using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.Backend.All
{
    class ObjectStore : Backend.ObjectStore
    {
        public static ObjectStore For(IEnumerable<Backend.ObjectStore> objectStores, int minSuccessful)
        {
            if (objectStores == null) return null;

            var stores = objectStores.ToArray();
            if (stores.Length == 0) return null;
            Array.Sort(stores);

            var url = "All(\n";
            var minPriority = 100;
            foreach (var store in stores)
            {
                url += store.Url + "\n";
                minPriority = Math.Min(minPriority, store.Priority);
            }
            url += ")\n";

            return new ObjectStore(url, minPriority, stores, minSuccessful);
        }

        // The (immutable) list of stores.
        private readonly Backend.ObjectStore[] stores;
        // The minimum number of stores to succeed for a put operation to be considered successful.
        private readonly int minSuccessful;

        // This keeps state in a lenient way - it's no problem if different threads see a different version of this value.
        // Assuming that int assignment is atomic, we do not need to synchronize this any further.
        private int bestToRead = 0;

        public ObjectStore(string url, int priority, Backend.ObjectStore[] stores, int minSuccessful)
            : base(url, priority)
        {
            this.stores = stores;
            this.minSuccessful = minSuccessful;
        }

        public override bool Has(Hash hash)
        {
            var best = bestToRead;   // Make a local copy, since another thread might change that value

            if (stores[best].Has(hash)) return true;
            for (var i = 0; i < stores.Length; i++)
                if (i != best && stores[i].Has(hash)) return true;

            return false;
        }

        public override Serialization.BurrowObject Get(Hash hash)
        {
            var best = bestToRead;   // Make a local copy, since another thread might change that value

            var obj = stores[best].Get(hash);
            if (obj != null) return obj;

            for (var i = 0; i < stores.Length; i++)
            {
                if (i == best) continue;
                obj = stores[i].Get(hash);
                if (obj != null) { bestToRead = i; return obj; }
            }

            return null;
        }

        public override Hash Put(Serialization.BurrowObject serializedObject, Configuration.PrivateIdentity identity)
        {
            var successful = 0;
            var successfulHash = null as Hash;
            for (var i = 0; i < stores.Length; i++)
            {
                var hash = stores[i].Put(serializedObject, identity);
                if (hash == null) continue;
                if (successfulHash == null) successfulHash = hash;
                else if (successfulHash != hash) return null;
                successful += 1;
            }

            return successful >= minSuccessful ? successfulHash : null;
        }

        // Asynchronous version
        public override TaskGroup.Future<Hash> Put(Serialization.BurrowObject serializedObject, Configuration.PrivateIdentity identity, TaskGroup taskGroup)
        {
            var future = taskGroup.WaitForMe<Hash>();
            var group = new TaskGroup();
            var futures = new TaskGroup.Future<Hash>[stores.Length];
            for (var i = 0; i < stores.Length; i++)
                futures[i] = stores[i].Put(serializedObject, identity, group);

            group.WhenDone(() =>
                {
                    var successful = 0;
                    var successfulHash = null as Hash;
                    for (var i = 0; i < futures.Length; i++)
                    {
                        if (futures[i].Result == null) continue;
                        if (successfulHash == null) successfulHash = futures[i].Result;
                        else if (successfulHash != futures[i].Result) { future.Done(null); return; }
                        successful += 1;
                    }
                    future.Done(successful >= minSuccessful ? successfulHash : null);
                });

            return future;
        }
    }
}
