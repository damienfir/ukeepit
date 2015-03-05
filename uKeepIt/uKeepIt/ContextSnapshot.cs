using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uKeepIt.MiniBurrow;
using uKeepIt.MiniBurrow.Folder;

namespace uKeepIt
{
    public class ContextSnapshot
    {
        public readonly ImmutableStack<Store> stores;
        public readonly ImmutableStack<ObjectStore> objectStores;
        public readonly MultiObjectStore multiObjectStore;
        public readonly ArraySegment<byte> key;

        public ContextSnapshot(Context context)
        {
            this.stores = ImmutableStack.From(context.stores);
            this.objectStores = ImmutableStack.From(context.objectStores);
            this.multiObjectStore = MultiObjectStore.For(context.objectStores);
            this.key = new ArraySegment<byte>(context.key.ToByteArray());
        }
    }
}