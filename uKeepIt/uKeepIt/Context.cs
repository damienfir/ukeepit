using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uKeepIt.MiniBurrow.Folder;

namespace uKeepIt
{
    public class Context
    {
        public MultiObjectStore multiobjectstore;
        public List<SynchronizedFolder> folders;
        public List<Store> stores;
        public ArraySegment<byte> previous_key;
        public ArraySegment<byte> key;

        public Context()
        {
            stores = new List<Store>();
            folders = new List<SynchronizedFolder>();
            previous_key = new ArraySegment<byte>();
            key = new ArraySegment<byte>();
        }

        public void watchFolders()
        {
            folders.ForEach(x => x.watch());
        }

        public void reloadKey(byte[] keybytes)
        {
            previous_key = key;
            if (keybytes != null)
            {
                key = new ArraySegment<byte>(keybytes);
                synchronize(previous_key, key);
            }
            else
            {
                key = new ArraySegment<byte>();
            }
        }

        public void reloadObjectStore(Dictionary<string, Store> stores_dict)
        {
            stores.Clear();

            var objectStores = new List<ObjectStore>();
            foreach (var store in stores_dict)
            {
                Console.WriteLine("adding store " + store.Key);
                stores.Add(store.Value);
                objectStores.Add(new ObjectStore(store.Value.Folder));
            }

            multiobjectstore = MultiObjectStore.For(objectStores);
        }

        public void reloadSpaces(Dictionary<string, Space> spaces_dict)
        {
            folders.ForEach(x => x.unwatch());
            folders.Clear();

            foreach (var space in spaces_dict)
            {
                folders.Add(new SynchronizedFolder(this, space.Value));
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
    }
}
