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
            var list = new List<string>();
            foreach (var name in MiniBurrow.Static.DirectoryEnumerateDirectories(Folder + "\\spaces"))
                list.Add(name.Replace(Folder + "\\spaces\\", ""));

            return list;
        }

        public Root SpaceRoot(string name)
        {
            return new Root(Folder + "\\spaces\\" + name);
        }
    }
}
