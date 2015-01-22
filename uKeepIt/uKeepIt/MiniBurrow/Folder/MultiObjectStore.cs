using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uKeepIt.MiniBurrow.Folder
{
    public class MultiObjectStore 
    {
        public static MultiObjectStore For(IEnumerable<ObjectStore> objectStores)
        {
            if (objectStores == null) return null;
            var stores = objectStores.ToArray();
            if (stores.Length == 0) return null;
            return new MultiObjectStore(stores);
        }

        // The (immutable) list of stores.
        private ObjectStore[] stores;

        // This keeps state in a lenient way - it's no problem if different threads see a different version of this value.
        // Assuming that int assignment is atomic, we do not need to synchronize this any further.
        private int bestToRead = 0;

        // This keeps state in a lenient way - it's no problem if different threads see a different version of this value.
        // Assuming that int assignment is atomic, we do not need to synchronize this any further.
        private int lastWritten = -1;

        public MultiObjectStore(ObjectStore[] stores)
        {
            this.stores = stores;
        }

        public bool Has(Hash hash)
        {
            var best = bestToRead;   // Make a local copy, since another thread might change that value

            if (stores[best].Has(hash)) return true;
            for (var i = 0; i < stores.Length; i++)
                if (i != best && stores[i].Has(hash)) return true;

            return false;
        }

        public Serialization.BurrowObject Get(Hash hash)
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

        public Hash Put(Serialization.BurrowObject serializedObject)
        {
            var last = lastWritten;   // Make a local copy, since another thread might change that value
            for (var i = 0; i < stores.Length; i++)
            {
                last += 1;
                if (last >= stores.Length) last = 0;
                var hash = stores[last].Put(serializedObject);
                if (hash != null) { 
                    lastWritten = last; // This may conflict with other threads, but is not a problem here. Statistically, we are still spreading the data over all available stores.
                    return hash; 
                }
            }

            return null;
        }
    }
}
