using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Backend;
using SafeBox.Burrow.Serialization;

namespace SafeBox.Burrow.Operations
{
    public class GetFromAnyStore
    {
        public delegate void Done(BurrowObject obj, ObjectStore source);

        private readonly Done handler;
        public readonly Hash hash;
        private ImmutableStack<ObjectStore> objectStores;
        private ImmutableStack<ObjectStore> triedStores = new ImmutableStack<ObjectStore>();

        public GetFromAnyStore(Hash hash, ImmutableStack<ObjectStore> objectStores, Done handler)
        {
            this.hash = hash;
            this.objectStores = objectStores;
            this.handler = handler;

            if (objectStores.Length < 1) { handler(null, null); return; }
            objectStores.Head.GetObject(hash, GetDone);
        }

        void GetDone(Serialization.BurrowObject obj)
        {
            if (obj == null || !obj.IsValid()) { TryNextStore(); return; }
            handler(obj, objectStores.Head);
        }

        private void TryNextStore()
        {
            // Mark this as tried
            triedStores = triedStores.With(objectStores.Head);

            while (true)
            {
                // Try the next store
                objectStores = objectStores.Tail;
                if (objectStores.Length == 0)
                {
                    handler(null, null);
                    return;
                }

                // Check if we have tried this one already
                // This yields N^2 performance, but will be executed rarely and with small N
                var found = false;
                foreach (var store in triedStores)
                    if (objectStores.Head == store) { found = true; break; }
                if (found) continue;

                // Try this
                objectStores.Head.GetObject(hash, GetDone);
                return;
            }
        }
    }
}
