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

        public readonly Burrow.Configuration.PrivateIdentity Identity;
        public readonly ObjectStore ObjectStore;
        public readonly Merge MergeHandler;
        public readonly TaskGroup.Future<ImmutableStack<Hash>> Future;
        private ImmutableStack<Hash> MergedHashes = new ImmutableStack<Hash>();

        public ReadRoot(Hash hash, string rootLabel, AccountStore accountStore, ObjectStore objectStore, Burrow.Configuration.PrivateIdentity identity, Merge mergeHandler, TaskGroup taskGroup)
        {
            this.ObjectStore = objectStore;
            this.Identity = identity;
            this.MergeHandler = mergeHandler;
            this.Future = taskGroup.WaitForMe<ImmutableStack<Hash>>();

            Asynchronous.Run(() => accountStore.List(hash, rootLabel), Process);
        }

        private void Process(IEnumerable<ObjectUrl> objectUrls)
        {
            if (objectUrls == null) return;
            var taskGroup = new TaskGroup();
            foreach (var objectUrl in objectUrls)
                new ReadRootObjectUrl(this, objectUrl, taskGroup);
            taskGroup.WhenDone(Finish);
        }
        
        internal void Finish()
        {
            Future.Done(MergedHashes);
        }
    }

    internal class ReadRootObjectUrl
    {
        public readonly ReadRoot ReadRoot;
        public readonly ObjectUrl ObjectUrl;
        private TaskGroup.Future<Hash> Future;
        private HashWithAesParameters Reference = null;

        public ReadRootObjectUrl(ReadRoot readRoot, ObjectUrl objectUrl, TaskGroup taskGroup)
        {
            this.ReadRoot = readRoot;
            this.ObjectUrl = objectUrl;
            this.Future = taskGroup.WaitForMe<Hash>();

            if (objectUrl.Url == null)
            {
                var store = Burrow.Static.ObjectStoreForUrl(objectUrl.Url, objectUrl.Url);
                Asynchronous.Run(()=> store.Get(objectUrl.Hash), GetFromMainStore);
            }
            else
            {
            }
            var stores = ReadRoot.ObjectStore;
            if (objectUrl.Url != null) stores = stores.With();
            taskGroup = new TaskGroup();
            new Burrow.Operations.GetFromAnyStore(objectUrl.Hash, stores, OpenEnvelope);
        }

        private void GetFromMainStore(BurrowObject obj)
        {
            if (obj == null)
        }

        private void OpenEnvelope(BurrowObject obj, ObjectStore source)
        {
            if (obj == null) { ReadRoot.Done(null); return; }
            Reference = Burrow.Static.OpenEnvelope(obj, ReadRoot.Identity, new ImmutableStack<Burrow.PublicIdentity>());
            if (Reference == null) { ReadRoot.Done(null); return; }
            new GetFromAnyStore(Reference.Hash, ReadRoot.ObjectStore.With(source), DecryptObject);
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
