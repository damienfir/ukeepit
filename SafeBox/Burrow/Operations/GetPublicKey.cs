using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Backend;
using SafeBox.Burrow.Serialization;

namespace SafeBox.Burrow.Operations
{
    public class GetPublicKey
    {
        public delegate void Done(PublicKey publicKey);
        private readonly Done handler;
        public readonly Hash Hash;
        public readonly Cache Cache;
        public readonly ImmutableStack<ObjectStore> ObjectStores;

        public GetPublicKey(Hash hash, Cache cache, ImmutableStack<ObjectStore> objectStores, Done handler)
        {
            this.Cache = cache;
            this.ObjectStores = objectStores;
            this.Hash = hash;
            this.handler = handler;

            // Cache lookup
            var publicKey = null as PublicKey;
            if (cache.PublicKeysByHash.TryGetValue(hash, out publicKey)) { handler(publicKey); return; }

            // Retrieve from the store
            new GetFromAnyStore(hash, objectStores, GetDone);
        }

        private void GetDone(BurrowObject obj, ObjectStore store)
        {
            if (obj == null) { handler(null); return; }
            var publicKey = PublicKey.From(obj);
            Cache.PublicKeysByHash[Hash] = publicKey;
            handler(publicKey);
        }
    }
}
