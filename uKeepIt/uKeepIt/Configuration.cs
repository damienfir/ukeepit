using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uKeepIt.MiniBurrow.Folder;
using uKeepIt.MiniBurrow.Serialization;
using uKeepIt.MiniBurrow;
using System.IO;
using System.Threading;

namespace uKeepIt
{
    public class Configuration
    {
        public readonly string _location;
        public readonly string _default_key_location;
        private FileSystemWatcher watcher;
        private ConfigurationWindow onChangedCallback;

        public ConfigurationEditor editor;
        private Context _context;
        public Tuple<string, byte[]> key;
        public Dictionary<string, Store> stores;
        public Dictionary<string, Space> spaces;

        private readonly string _store_section = "store";
        private readonly string _space_section = "space";
        private readonly string _key_section = "key";
        private readonly string _path_key = "path";
        public readonly string _default_folder = "";

        private List<FileSystemWatcher> watchedFolders;


        public Configuration() : this(null) { }

        public Configuration(Context context)
        {
            _context = context;
            editor = new ConfigurationEditor(this);
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\di55erent\\uKeepIt";
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            watchedFolders = new List<FileSystemWatcher>();

            _location = appDataFolder + "\\conf.ini";
            _default_key_location = appDataFolder + "\\key.data";

            stores = new Dictionary<string, Store>();
            spaces = new Dictionary<string, Space>();
            key = Tuple.Create<string, byte[]>(_default_key_location, null);

            if (_context != null)
            {
                var directory = Path.GetDirectoryName(_location);
                var filename = Path.GetFileName(_location);
                watcher = new FileSystemWatcher(directory, filename);
                watcher.EnableRaisingEvents = true;
                watcher.Changed += watcher_Changed;
            }

            readConfig();
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            // try to read until file is available to read

            int tries = 0;
            while (true)
            {
                ++tries;
                try
                {
                    readConfig();
                    onChangedCallback.notify();
                    break;
                }
                catch (IOException)
                {
                    Thread.Sleep(100);
                }
                if (tries > 10) break;
            }
        }

        public void reloadContext()
        {
            if (_context != null)
            {
                _context.unWatchFolders();
                _context.reloadKey(key.Item2);
                _context.reloadObjectStore(stores);
                _context.reloadSpaces(spaces.Where(x => x.Value.folder != _default_folder).ToDictionary(e => e.Key, e => e.Value));
                _context.watchFolders();
                _context.synchronize();
            }
        }

        public bool addStore(string name, string location)
        {
            if (!location.Equals(_default_folder) && !Directory.Exists(location))
            {
                Utils.alert("The cloud " + name + " previously located at " + location + " cannot be found any more, please select the new location");
                return false;
            }

            if (stores.ContainsKey(name) && stores[name].Folder.Equals(location))
                return false;

            stores.Add(name, new Store(location));
            return true;
        }

        public bool addStore(string location)
        {
            string name = location.Replace(System.IO.Path.GetDirectoryName(location) + "\\", "");
            location += @"\ukeepit";
            
            var spaces_location = location + @"\spaces";
            if (!Directory.Exists(spaces_location))
            {
                if (!MiniBurrow.Static.DirectoryCreate(spaces_location))
                    return false;
            }
            return addStore(name, location);
        }

        public bool removeStore(string name)
        {
            return stores.Remove(name);
        }

        public bool removeStoreWithFiles(string name)
        {
            var store = stores[name];
            removeStore(name);
            _context.removeObjectStore(store, stores);
            try
            {
                Directory.Delete(store.Folder, true);
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return true;
        }

        public bool addSpace(string name, string location)
        {
            if (!location.Equals(_default_folder) && !Directory.Exists(location))
            {
                Utils.alert("The secured folder " + name + " previously located at " + location + " cannot be found any more, please select the new location");
                return false;
            }

            // broad check
            if (location != _default_folder && spaces.Values.Where(x => x.folder.Equals(location)).Count() > 0) return false;

            // change name if already exists
            if (spaces.ContainsKey(name))
            {
                var oldname = name;
                int i = 1;
                do {
                    name = oldname + " (" + i + ")";
                    i++;
                } while (spaces.ContainsKey(name));
            }

            spaces.Add(name, new Space(name, location));
            return true;
        }

        public bool addSpace(string folder)
        {
            string name = folder.Replace(System.IO.Path.GetDirectoryName(folder) + "\\", "");
            return addSpace(name, folder);
        }

        public bool removeSpace(string name)
        {
            return spaces.Remove(name);
        }

        public bool deleteSpace(string name)
        {
            foreach (var store in stores)
            {
                try
                {
                    Directory.Delete(store.Value.SpaceRoot(name).Folder, true);
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return spaces.Remove(name);
        }

        public void readConfig()
        {
            if (!File.Exists(_location))
            {
                return;
            }

            IniFile config = IniFile.From(File.ReadAllBytes(_location));

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

            var key_file = _default_key_location;
            if (config.SectionsByName.ContainsKey(_key_section))
            {
                key_file = config.SectionsByName[_key_section].Get(_path_key, _default_key_location);
            }
            key = Tuple.Create(key_file, MiniBurrow.Static.FileBytes(key_file, null));

            reloadContext();
        }

        public void writeConfig()
        {
            if (watcher != null)
                watcher.EnableRaisingEvents = false;

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
            sec.Set(_path_key, key.Item1);

            MiniBurrow.Static.WriteFile(_location, config.ToBytes());

            if (watcher != null)
                watcher.EnableRaisingEvents = true;
        }

        public bool invalidateKey(string pw)
        {
            byte[] keycheck = AESKey.generateKey(pw);

            if (keycheck.SequenceEqual(key.Item2))
            {
                return true;
            }
            return false;
        }

        public byte[] getKey()
        {
            return key.Item2;
        }

        internal bool changeKey(string pw)
        {
            key = Tuple.Create(key.Item1, AESKey.generateKey(pw));
            if (_context != null)
            {
                _context.reloadKey(key.Item2);
                _context.synchronizeWithNewKey();
            }
            return AESKey.storeKey(key.Item2, key.Item1);
        }

        internal void registerOnChanged(ConfigurationWindow configWindow)
        {
            onChangedCallback = configWindow;
        }

        internal bool firstStart()
        {
            if (getKey() == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class ConfigurationEditor
    {
        private Configuration _config;

        public ConfigurationEditor(Configuration config)
        {
            _config = config;
        }

        public void execute(bool success)
        {
            if (success)
            {
                _config.writeConfig();
            }
        }

        public void add_store(string folder)
        {
            execute(_config.addStore(folder));
        }

        public void remove_store(string name)
        {
            execute(_config.removeStore(name));
        }

        public void remove_store_with_files(string name)
        {
            execute(_config.removeStoreWithFiles(name));
        }

        public void add_space(string folder)
        {
            execute(_config.addSpace(folder));
        }

        public void remove_space(string name)
        {
            execute(_config.removeSpace(name));
        }

        public void checkout_space(string name, string target_location)
        {
            _config.removeSpace(name);
            execute(_config.addSpace(name, target_location));
        }

        public void change_key(string pw1)
        {
            execute(_config.changeKey(pw1));
        }
    }
}