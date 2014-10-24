using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Backend;
using SafeBox.Burrow.Serialization;

namespace SafeBox.Burrow.Operations
{
    public class ReadRoot
    {
        public delegate bool Merge(Burrow.Serialization.Dictionary dictionary);
        public delegate void Done(ImmutableStack<Hash> mergedHashes);

        public readonly Burrow.Configuration.PrivateIdentity Identity;
        public readonly ImmutableStack<ObjectStore> ObjectStores;
        public readonly Merge MergeHandler;
        public readonly Done DoneHandler;
        private int expected = 0;
        private ImmutableStack<Hash> MergedHashes = new ImmutableStack<Hash>();

        public ReadRoot(Hash hash, string rootLabel, ImmutableStack<AccountStore> accountStores, ImmutableStack<ObjectStore> objectStores, Burrow.Configuration.PrivateIdentity identity, Merge mergeHandler, Done doneHandler)
        {
            this.ObjectStores = objectStores;
            this.Identity = identity;
            this.MergeHandler = mergeHandler;
            this.DoneHandler = doneHandler;

            expected = 1;
            foreach (var accountStore in accountStores) accountStore.Account(hash).Root(rootLabel).List(Process);
            Done(null);
        }

        private void Process(IEnumerable<ObjectUrl> objectUrls)
        {
            if (objectUrls == null) return;
            foreach (var objectUrl in objectUrls)
            {
                expected += 1;
                new ReadRootObjectUrl(this, objectUrl);
            }
        }

        internal void Done(Hash hash)
        {
            expected -= 1;
            if (hash != null) MergedHashes = MergedHashes.With(hash);
            if (expected > 0) return;
            DoneHandler(MergedHashes);
        }
    }

    internal class ReadRootObjectUrl
    {
        public readonly ReadRoot ReadRoot;
        public readonly ObjectUrl ObjectUrl;
        private HashWithAesParameters Reference = null;

        public ReadRootObjectUrl(ReadRoot readRoot, ObjectUrl objectUrl)
        {
            this.ReadRoot = readRoot;
            this.ObjectUrl = objectUrl;

            var stores = ReadRoot.ObjectStores;
            if (objectUrl.Url != null) stores = stores.With(Burrow.Static.ObjectStoreForUrl(objectUrl.Url, objectUrl.Url));
            new Burrow.Operations.GetFromAnyStore(objectUrl.Hash, stores, OpenEnvelope);
        }

        private void OpenEnvelope(BurrowObject obj, ObjectStore source)
        {
            if (obj == null) { ReadRoot.Done(null); return; }
            Reference = Burrow.Static.OpenEnvelope(obj, ReadRoot.Identity, new ImmutableStack<Burrow.PublicIdentity>());
            if (Reference == null) { ReadRoot.Done(null); return; }
            new GetFromAnyStore(Reference.Hash, ReadRoot.ObjectStores.With(source), DecryptObject);
        }

        private void DecryptObject(BurrowObject obj, ObjectStore source)
        {
            if (obj == null) {ReadRoot.Done(null); return;}
            var decryptedData = Aes.Decrypt(obj.Data, Reference.Key, Reference.Iv);
            var dictionary = Burrow.Serialization.Dictionary.From(obj, decryptedData);
            if (dictionary== null) { ReadRoot.Done(null); return; }
            var success = ReadRoot.MergeHandler(dictionary);
            ReadRoot.Done(success ? ObjectUrl.Hash : null);
        }
    }
}
