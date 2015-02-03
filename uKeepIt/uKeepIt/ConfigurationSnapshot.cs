using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uKeepIt.MiniBurrow.Folder;
using uKeepIt.MiniBurrow.Serialization;
using uKeepIt.MiniBurrow;
using System.IO;

namespace uKeepIt
{
    // This is an immutable snapshot
    public class ConfigurationSnapshot
    {
        public readonly string Folder;
        public MultiObjectStore MultiObjectStore;
        public Store[] Stores;
        public IniFile iniFile;
        public ArraySegment<byte> key;
        public List<SynchronizedFolder> synced_folders;

        private readonly string _store_section = "store";
        private readonly string _space_section = "space";
        private readonly string _key_section = "key";
        private readonly string _path_key = "path";
        private readonly string _default_folder = "";

        public ConfigurationSnapshot(string folder)
        {
            this.Folder = folder;

            // Load the configuration
            iniFile = IniFile.From(MiniBurrow.Static.FileBytes(Folder + @"\configuration"));
            
            var stores = getStores();
            var spaces = getSpaces();
            setupMultiObjectStore(stores);
            setupSpaces(stores, spaces);
            getKey();
        }

        

        public IEnumerable<string> Spaces()
        {
            var spaceNames = new HashSet<string>();
            foreach (var store in Stores)
            {
                var list = store.ListSpaces();
                foreach (var item in list) spaceNames.Add(item);
            }
            return spaceNames;
        }
    }
}