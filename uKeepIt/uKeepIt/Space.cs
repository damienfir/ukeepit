using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uKeepIt.MiniBurrow;
using uKeepIt.MiniBurrow.Folder;

namespace uKeepIt
{
    public class Space
    {
        public readonly MultiObjectStore multiObjectStore;
        public readonly string Name;
        public readonly ImmutableStack<Root> Roots;
        public ArraySegment<byte> Key;

        public Space(Store[] stores, MultiObjectStore multiObjectStore, string name)
        {
            this.Name = name;
            this.multiObjectStore = multiObjectStore;

            var roots = new ImmutableStack<Root>();
            foreach (var store in stores)
                roots = roots.With(store.SpaceRoot(name));
            this.Roots = roots;
        }

        public SpaceEditor CreateEditor(ArraySegment<byte> key)
        {
            return new SpaceEditor(multiObjectStore, Roots, key);
        }

    }
}
