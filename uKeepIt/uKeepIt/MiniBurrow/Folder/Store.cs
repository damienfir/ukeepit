using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uKeepIt.MiniBurrow.Folder
{
    public class Store
    {
        public readonly string Folder;
        public readonly Root MasterRoot;

        public Store(string folder)
        {
            this.Folder = folder;
            this.MasterRoot = new Root(folder + "\\master");
        }

        public IEnumerable<string> ListSpaces()
        {
            return MiniBurrow.Static.DirectoryEnumerateDirectories(Folder + "\\spaces");
        }

        public Root SpaceRoot(string name)
        {
            return new Root(Folder + "\\spaces\\" + name);
        }
    }
}
