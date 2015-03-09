using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using uKeepIt.MiniBurrow;
using uKeepIt.MiniBurrow.Folder;

namespace uKeepIt
{
    public class Context
    {
        public MultiObjectStore multiobjectstore;
        public List<ObjectStore> objectStores;
        public List<SynchronizedFolder> folders;
        public List<Store> stores;
        public ArraySegment<byte> previous_key;
        public ArraySegment<byte> key;
        private Timer gbTimer;

        public Context()
        {
            stores = new List<Store>();
            folders = new List<SynchronizedFolder>();
            previous_key = new ArraySegment<byte>();
            key = new ArraySegment<byte>();
            gbTimer = null;
        }

        public void watchFolders()
        {
            folders.ForEach(x => x.watch());
        }

        public void unWatchFolders()
        {
            folders.ForEach(x => x.unwatch());
        }

        public void reloadKey(byte[] keybytes)
        {
            previous_key = key;
            if (keybytes != null)
            {
                key = new ArraySegment<byte>(keybytes);
            }
            else
            {
                key = new ArraySegment<byte>();
            }
        }

        public void reloadObjectStore(Dictionary<string, Store> stores_dict)
        {
            stores.Clear();
            objectStores = new List<ObjectStore>();
            foreach (var store in stores_dict)
            {
                Console.WriteLine("adding store " + store.Key + ": " + store.Value.Folder);
                stores.Add(store.Value);
                objectStores.Add(new ObjectStore(store.Value.Folder));
            }

            multiobjectstore = MultiObjectStore.For(objectStores);
        }

        public void removeObjectStore(Store store, Dictionary<string, Store> stores_dict)
        {
            var os = objectStores.Find((x) => x.Folder == store.Folder);
            reloadObjectStore(stores_dict);
            var hashList = os.List();

            foreach (var item in hashList)
            {
                var relative = item.Replace(store.Folder + "\\", "");
                var hash = relative.Replace("\\", "");
                var obj = os.Get(Hash.From(hash));
                multiobjectstore.Put(obj);
            }
        }

        public void reloadSpaces(Dictionary<string, Space> spaces_dict)
        {
            folders.Clear();

            foreach (var space in spaces_dict)
            {
                folders.Add(new SynchronizedFolder(new ContextSnapshot(this), space.Value));
            }
        }

        public void synchronizeWithNewKey()
        {
            if (previous_key.Array != null)
            {
                synchronize(previous_key, key);
            }
            else
            {
                synchronize();
            }
        }

        public void synchronize()
        {
            synchronize(key, key);
        }

        public void synchronize(ArraySegment<byte> readkey, ArraySegment<byte> writekey)
        {
            foreach (var folder in folders)
            {
                folder.syncFolder(readkey, writekey);
            }
        }

        public void enableGarbageCollection()
        {
            if (gbTimer == null)
            {
                gbTimer = new System.Threading.Timer(this.garbageCollection, this, (long)(60 * 1000), (long)(3600 * 1000));
            }
        }

        public void disableGarbageCollection()
        {
            if (gbTimer != null)
            {
                gbTimer.Dispose();
                gbTimer = null;
            }
        }

        public void garbageCollection(Object context)
        {
            foreach (var folder in folders)
            {
                if (folder.isSyncing()) return;
                folder.disableSync();
            }

            Console.WriteLine("starting gb...");
            var ctx = new ContextSnapshot(context as Context);
            var gb = new GarbageCollection(ctx.stores, ctx.objectStores);
            bool success = gb.successful;
            Console.WriteLine("finished gb: " + success);

            folders.ForEach((x) => x.enableSync());
        }
    }
}