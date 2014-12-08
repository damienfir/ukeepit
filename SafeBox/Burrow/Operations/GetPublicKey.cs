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
        public readonly Hash Hash;
        public readonly Cache Cache;
        public readonly ObjectStore ObjectStore;
        public readonly TaskGroup.Future<PublicKey> Future;

        public GetPublicKey(Hash hash, Cache cache, ObjectStore objectStore, TaskGroup taskGroup)
        {
            this.Future = taskGroup.WaitForMe<PublicKey>();
            this.Cache = cache;
            this.ObjectStore = objectStore;
            this.Hash = hash;

            // Cache lookup
            var publicKey = null as PublicKey;
            if (cache.PublicKeysByHash.TryGetValue(hash, out publicKey)) { Future.Done(publicKey); return; }

            // Retrieve from the store
            Asynchronous.Run(() => objectStore.Get(hash), GetDone);
        }

        private void GetDone(BurrowObject obj)
        {
            if (obj == null) { Future.Done(null); return; }
            var publicKey = PublicKey.From(obj);
            Cache.PublicKeysByHash[Hash] = publicKey;
            Future.Done(publicKey);
        }
    }
}
