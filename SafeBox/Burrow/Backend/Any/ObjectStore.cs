using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.Backend.Any
{
    class ObjectStore : Backend.ObjectStore
    {
        public static ObjectStore For(IEnumerable<Backend.ObjectStore> objectStores)
        {
            if (objectStores == null) return null;

            var stores = objectStores.ToArray();
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

            return new ObjectStore(url, minPriority, stores);
        }

        // The (immutable) list of stores.
        private Backend.ObjectStore[] stores;

        // This keeps state in a lenient way - it's no problem if different threads see a different version of this value.
        // Assuming that int assignment is atomic, we do not need to synchronize this any further.
        private int bestToRead = 0;

        // This keeps state in a lenient way - it's no problem if different threads see a different version of this value.
        // Assuming that int assignment is atomic, we do not need to synchronize this any further.
        private int lastWritten = -1;

        public ObjectStore(string url, int priority, Backend.ObjectStore[] stores)
            : base(url, priority)
        {
            this.stores = stores;
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
            var last = lastWritten;   // Make a local copy, since another thread might change that value
            for (var i = 0; i < stores.Length; i++)
            {
                last += 1;
                if (last >= stores.Length) last = 0;
                var hash = stores[last].Put(serializedObject, identity);
                if (hash != null) { 
                    lastWritten = last; // This may conflict with other threads, but is not a problem here. Statistically, we are still spreading the data over all available stores.
                    return hash; 
                }
            }

            return null;
        }
    }
}
