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
        public ArraySegment<byte> key;

        public Context()
        {
            stores = new List<Store>();
            folders = new List<SynchronizedFolder>();
            key = new ArraySegment<byte>();
        }

        public void watchFolders()
        {
            folders.ForEach(x => x.watch());
        }

        public void reloadKey(byte[] keybytes)
        {
            key = new ArraySegment<byte>(keybytes);
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
    }
}
