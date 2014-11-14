using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.Backend.Cache
{
    class ObjectStore : Backend.ObjectStore
    {
        public readonly Backend.ObjectStore Cache;
        public readonly Backend.ObjectStore Backend;

        public ObjectStore(Backend.ObjectStore cache, Backend.ObjectStore backend)
            : base("(\nCache=" + cache.Url + "\nBackend=" + backend.Url + "\n)\n", backend.Priority)
        {
            this.Cache = cache;
            this.Backend = backend;
        }

        public override bool Has(Hash hash)
        {
            if (Cache.Has(hash)) return true;
            return Backend.Has(hash);
        }

        public override Serialization.BurrowObject Get(Hash hash)
        {
            var obj = Cache.Get(hash);
            if (obj != null) return obj;
            return Backend.Get(hash);
        }

        public override Hash Put(Serialization.BurrowObject serializedObject, Configuration.PrivateIdentity identity)
        {
            var hash = Cache.Put(serializedObject, identity);
            var backendHash = Backend.Put(serializedObject, identity);
            return backendHash;
        }

        // Asynchronous version
        public override TaskGroup.Future<Hash> Put(Serialization.BurrowObject serializedObject, Configuration.PrivateIdentity identity, TaskGroup taskGroup)
        {
            var future = taskGroup.WaitForMe<Hash>();
            var group = new TaskGroup();
            var cacheFuture = Cache.Put(serializedObject, identity, group);
            var backendFuture = Backend.Put(serializedObject, identity, group);
            group.WhenDone(() => future.Done(backendFuture.Result));
            return future;
        }
    }
}
