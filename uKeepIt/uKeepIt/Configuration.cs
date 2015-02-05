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
        public readonly string _location;

        private Context _context;
        private byte[] _key;
        private Dictionary<string, Store> _stores;
        private Dictionary<string, Space> _spaces;

        private readonly string _store_section = "store";
        private readonly string _space_section = "space";
        private readonly string _key_section = "key";
        private readonly string _path_key = "path";
        private readonly string _default_folder = "";

        public Configuration(Context context)
        {
            _context = context;
            //var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            //Folder = appDataFolder + "\\ukeepit";
            _location = @"C:\Users\damien\Documents\ukeepit\test\config\configuration";

            _stores = new Dictionary<string, Store>();
            _spaces = new Dictionary<string, Space>();

            readConfig();
            reloadContext();
        }

        public void addStore(string name, string location)
        {
            _stores.Add(name, new Store(location));
        }

        public void removeStore(string name) { }

        public void addSpace(string name, string location)
        {
            _spaces.Add(name, new Space(name, location));
        }

        public void removeSpace(string name) { }

        public void readConfig()
        {
            IniFile config = IniFile.From(MiniBurrow.Static.FileBytes(_location));

            foreach (var sectionPair in config.SectionsByName)
            {
                if (sectionPair.Key.StartsWith(_store_section))
                {
                    var storeName = sectionPair.Key.Substring(_store_section.Length + 1);
                    var storeFolder = sectionPair.Value.Get(_path_key);
                    addStore(storeName, storeFolder);
                }
                else if (sectionPair.Key.StartsWith(_space_section))
                {
                    var spaceName = sectionPair.Key.Substring(_space_section.Length + 1);
                    var spaceFolder = sectionPair.Value.Get(_path_key, _default_folder);
                    addSpace(spaceName, spaceFolder);
                }
            }

            foreach (var store in _stores)
            {
                foreach (var checkedin in store.Value.ListSpaces())
                {
                    if (_spaces.ContainsKey(checkedin)) continue;
                    addSpace(checkedin, _default_folder);
                }
            }

            _key = new byte[32];
            for (int i = 0; i < _key.Length; i++) _key[i] = 0x01;
        }

        public void reloadContext()
        {
            _context.reloadKey(_key);
            _context.reloadObjectStore(_stores);
            _context.reloadSpaces(_spaces);
        }

        public void writeConfig()
        {
            IniFile config = new IniFile();

            // write
        }
    }
}