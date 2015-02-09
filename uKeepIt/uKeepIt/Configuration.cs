﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uKeepIt.MiniBurrow.Folder;
using uKeepIt.MiniBurrow.Serialization;
using uKeepIt.MiniBurrow;
using System.IO;

namespace uKeepIt
{
    public class Configuration
    {
        public readonly string _location;

        private Context _context;
        public byte[] key;
        public Dictionary<string, Store> stores;
        public Dictionary<string, Space> spaces;

        private readonly string _store_section = "store";
        private readonly string _space_section = "space";
        private readonly string _key_section = "key";
        private readonly string _path_key = "path";
        public readonly string _default_folder = "--";

        public Configuration(Context context)
        {
            _context = context;
            //var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            //Folder = appDataFolder + "\\ukeepit";
            _location = @"C:\Users\damien\Documents\ukeepit\test\config\configuration";

            stores = new Dictionary<string, Store>();
            spaces = new Dictionary<string, Space>();

            readConfig();
            reloadContext();
        }

        public void reloadContext()
        {
            _context.reloadKey(key);
            _context.reloadObjectStore(stores);
            _context.reloadSpaces(spaces.Where(x => x.Value.folder != _default_folder).ToDictionary(e => e.Key, e => e.Value));
        }

        public bool addStore(string name, string location)
        {
            if (stores.ContainsKey(name))
                return false;

            location += @"\spaces";
            if (!Directory.Exists(location))
            {
                if (!MiniBurrow.Static.DirectoryCreate(location))
                    return false;
            }
            stores.Add(name, new Store(location));
            return true;
        }

        public bool removeStore(string name)
        {
            return stores.Remove(name);
        }

        public bool addSpace(string name, string location)
        {
            if (spaces.ContainsKey(name))
                return false;

            spaces.Add(name, new Space(name, location));
            return true;
        }

        public bool removeSpace(string name, bool delete_folder = false)
        {
            if (delete_folder)
            {
                MiniBurrow.Static.DirectoryDelete(spaces[name].folder);
            }
            return spaces.Remove(name);
        }

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

            foreach (var store in stores)
            {
                foreach (var checkedin in store.Value.ListSpaces())
                {
                    if (spaces.ContainsKey(checkedin)) continue;
                    addSpace(checkedin, _default_folder);
                }
            }

            key = new byte[32];
            for (int i = 0; i < key.Length; i++) key[i] = 0x01;
        }

        public void writeConfig()
        {
            IniFile config = new IniFile();
            string tpl = "{0} {1}";

            foreach (var store in stores)
            {
                IniFileSection section = config.Section(String.Format(tpl, _store_section, store.Key));
                section.Set(_path_key, store.Value.Folder);
            }

            foreach (var space in spaces) {
                IniFileSection section = config.Section(String.Format(tpl, _space_section, space.Key));
                section.Set(_path_key, space.Value.folder);
            }

            IniFileSection sec = config.Section(_key_section);
            sec.Set(_path_key, "--");

            MiniBurrow.Static.WriteFile(_location, config.ToBytes());
        }
    }
}