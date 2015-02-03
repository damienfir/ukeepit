using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uKeepIt.MiniBurrow.Folder;
using uKeepIt.MiniBurrow.Serialization;
using uKeepIt.MiniBurrow;

namespace uKeepIt
{
    public class Configuration
    {
        public readonly string Location;

        public Context context;
        public Store[] Stores;
        public ArraySegment<byte> key;
        public List<SynchronizedFolder> synced_folders;

        private readonly string _store_section = "store";
        private readonly string _space_section = "space";
        private readonly string _key_section = "key";
        private readonly string _path_key = "path";
        private readonly string _default_folder = "";

        public Configuration(Context context)
        {
            //var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            //Folder = appDataFolder + "\\ukeepit";
            Location = @"C:\Users\damien\Documents\ukeepit\test\config";
            
        }

        public void addStore(string name, string location) { }
        public void removeStore(string name) { }

        public void addSpace(string name, string location) { }
        public void removeSpace(string name) { }

        public void readConfig()
        {
            IniFile config = IniFile.From(MiniBurrow.Static.FileBytes(Location));

            var stores = new List<Store>();
            var spaces = new List<Tuple<string, string>>();

            foreach (var sectionPair in config.SectionsByName)
            {
                if (sectionPair.Key.StartsWith(_store_section))
                {
                    var storeFolder = sectionPair.Value.Get(_path_key);
                    stores.Add(new Store(storeFolder));
                }
                else if (sectionPair.Key.StartsWith(_space_section))
                {
                    var spaceName = sectionPair.Key.Substring(_space_section.Length + 1);
                    var spaceFolder = sectionPair.Value.Get(_path_key, _default_folder);
                    spaces.Add(Tuple.Create(spaceName, spaceFolder));
                }
            }

            foreach (var store in stores)
            {
                foreach (var checkedin in store.ListSpaces())
                {
                    if (spaces.Find(x => x.Item1.Equals(checkedin)) != null) continue;
                    spaces.Add(Tuple.Create(checkedin, _default_folder));
                }
            }

            byte[] keybytes = new byte[32];
            for (int i = 0; i < keybytes.Length; i++) keybytes[i] = 0x01;
            key = new ArraySegment<byte>(keybytes);

            context.reloadStores(stores);
            context.reloadSpaces(stores, spaces);
        }

        public void writeConfig()
        {
            IniFile config = new IniFile();

            
        }
    }
}