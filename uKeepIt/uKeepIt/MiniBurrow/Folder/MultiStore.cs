using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uKeepIt.MiniBurrow.Folder
{
    // NOT IN USE
    class MultiStore
    {
        public static MultiStore For(IEnumerable<Store> stores)
        {
            if (stores== null) return null;
            var array = stores.ToArray();
            if (array.Length == 0) return null;
            return new MultiStore(array);
        }

        // The (immutable) list of stores.
        private Store[] stores;

        public MultiStore(Store[] stores)
        {
            this.stores = stores;
        }

        public IEnumerable<Root> ListSpaces()
        {
            var roots = new ImmutableStack<Root>();
            foreach (string subFolder in MiniBurrow.Static.DirectoryEnumerateDirectories(Folder + "\\spaces"))
                roots = roots.With(new Root(Folder + "\\spaces\\" + subFolder));
            return roots;
        }
    }
}
