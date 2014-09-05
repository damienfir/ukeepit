using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Serialization;
using SafeBox.Burrow.Backend;

namespace SafeBox.Burrow.DataTree
{
    // This object may only be used on a single thread.
    public class Cache
    {
        public delegate void PartialTreeLoaded(PartialTree partialTree);
        internal Dictionary<string, PartialTree> partialTrees = new Dictionary<string,PartialTree>();

        internal void PartialTree(HashWithAesParameters hashWithAesParameters, ImmutableStack<ObjectStore> objectStores, PartialTreeLoaded handler)
        {
            PartialTree value;
            if (partialTrees.TryGetValue(hashWithAesParameters.Hash.Hex(), out value)) handler(value);
            else new LoadPartialTree(this, hashWithAesParameters, objectStores, handler);
        }
    }

    public class LoadPartialTree
    {
        Cache cache;
        HashWithAesParameters hashWithAesParameters;
        Cache.PartialTreeLoaded handler;

        public LoadPartialTree(Cache cache, HashWithAesParameters hashWithAesParameters, ImmutableStack<ObjectStore> objectStores, Cache.PartialTreeLoaded handler)
        {
            this.cache = cache;
            this.hashWithAesParameters = hashWithAesParameters;
            this.handler = handler;
            new Burrow.Operations.GetFromAnyStore(hashWithAesParameters.Hash, objectStores, GetObjectDone);
        }

        public void GetObjectDone(BurrowObject obj, ObjectStore source)
        {
            var decryptedData = Aes.Decrypt(obj.Data, hashWithAesParameters.Key, hashWithAesParameters.Iv);
            var partialTree = PartialTree.From(obj, decryptedData);
            cache.partialTrees[hashWithAesParameters.Hash.Hex()] = partialTree;
        }
    }
}
