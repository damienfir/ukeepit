﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uKeepIt.MiniBurrow.Folder;
using uKeepIt.MiniBurrow.Serialization;
using uKeepIt.MiniBurrow;

namespace uKeepIt
{
    // This is an immutable snapshot
    public class ConfigurationSnapshot
    {
        public readonly string Folder;
        public readonly MultiObjectStore MultiObjectStore;
        public readonly Store[] Stores;

        public ConfigurationSnapshot(string folder)
        {
            this.Folder=folder;

            // Load the configuration
            var stores = new ImmutableStack<Store>();
            var objectStores = new ImmutableStack<ObjectStore>();
            var iniFile = IniFile.From(MiniBurrow.Static.FileBytes(Folder + "\\configuration"));
            foreach (var sectionPair in iniFile.SectionsByName)
            {
                if (sectionPair.Key.Substring(0, 6) != "store ") continue;
                var storeFolder = sectionPair.Value.Get("folder");
                stores=stores.With( new Store(storeFolder));
                objectStores = objectStores.With(new ObjectStore(storeFolder));
            }
            
            // Create the main object
            Stores = stores.ToArray();
            MultiObjectStore = MultiObjectStore.For(objectStores);
        }

        IEnumerable<string> Spaces()
        {
            var spaceNames = new HashSet<string>();
            foreach (var store in Stores)
            {
                var list = store.ListSpaces();
                foreach (var item in list) spaceNames.Add(item);
            }
            return spaceNames;
        }

        Space Space(string name)
        {
            return new Space(this, name);
        }
    }
}