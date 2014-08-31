using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using SafeBox.Burrow.Abstract;
using SafeBox.Burrow;

namespace SafeBox.Burrow.Configuration
{
    public class Configuration
    {
        // *** Static ***

        public static Configuration CreateDefaultConfiguration()
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configurationFolder = appDataFolder + "\\burrow-data.org";
            return new Configuration(configurationFolder);
        }

        // *** Object ***

        public delegate void ReloadedHandler(ConfigurationSnapshot snapshot);
        public readonly string Folder;
        public ConfigurationSnapshot Snapshot = null;
        public event EventHandler<EventArgs> Reloaded;

        public Configuration(string folder)
        {
            this.Folder = folder;
            this.Snapshot = new ConfigurationSnapshot(this, new ImmutableStack<ObjectStore>(), new ImmutableStack<PrivateIdentity>());
        }

        public void Reload(ReloadedHandler handler = null) { ThreadPool.QueueUserWorkItem(new WaitCallback(obj => ReloadAsync(handler))); }

        private void ReloadAsync(ReloadedHandler handler)
        {
            // Create the folders if necessary
            Static.DirectoryCreate(Folder);

            // Load the stores
            var storesFolder = Folder + "\\stores";
            Static.DirectoryCreate(storesFolder);

            // Read all stores
            var stores = new ImmutableStack<ObjectStore>();
            foreach (var file in Static.File(storesFolder))
            {
                // Read the store information
                var settingsBytes = Static.ReadFile(file, new byte[0]);
                var settings = Dictionary.From(settingsBytes);

                // Create a new store
                var store = Static.ObjectStoreForUrl(settings, file);
                stores = stores.With(store);
            }

            // If no stores are configured, create a main store
            if (stores.Length == 0)
            {
                var settings = new Dictionary();
                settings.Set("url", Folder + "\\MainStore");
                settings.Set("permanent", true);
                settings.Set("create if necessary", true);
                Static.WriteFile(Folder + "\\stores\\main", new ArraySegment<byte>(settings.ToBytes()));
                stores = stores.With(Static.StoreForSettingsAsync(settings, "Newly created main store"));
            }

            // Load the identities
            var identitiesFolder = Folder + "\\identities";
            Static.DirectoryCreate(identitiesFolder);

            // Read all identities
            var identities = new ImmutableStack<PrivateIdentity>();
            foreach (var folder in Static.DirectoryEnumerateDirectories(identitiesFolder))
            {
                // Read the public information
                var publicInformationBytes = Static.ReadFile(folder + "\\public-information", new byte[0]);
                var publicInformation = Dictionary.From(publicInformationBytes);

                // Read and parse the public key
                var publicKeyBytes = Static.ReadFile(folder + "\\public-key", null);
                var publicKey = PublicKey.FromBytes(publicKeyBytes);
                if (publicKey == null) continue;
                var publicIdentity = new PublicIdentity(publicKey, publicInformation, ImmutableStack.With(publicKey));

                // Read the private key (but don't decrypt it yet)
                var privateInformationBytes = Static.ReadFile(folder + "\\private-information", null);
                var privateInformation = Dictionary.From(privateInformationBytes);
                if (privateInformation == null) continue;

                // Create a new identity, and set the public information
                var privateIdentity = new PrivateIdentity(publicIdentity, new ArraySegment<byte>(publicKeyBytes), privateInformation);
                identities = identities.With(privateIdentity);
            }

            // Create a new snapshot, and notify the listeners
            var snapshot = new ConfigurationSnapshot(this, stores, identities);
            Static.SynchronizationContext.Post(new SendOrPostCallback(obj => { 
                Snapshot = snapshot; 
                if (handler != null) handler(snapshot); 
                if (Reloaded != null) Reloaded(this, new EventArgs()); 
            }), null);
        }

        public IdentityFolderForHash IdentityFolderForHash(Hash hash)
        {
            // Look through all identity folders
            foreach (var identityFolder in Static.DirectoryEnumerateDirectories(Folder + "\\identities"))
            {
                var info = new IdentityFolderForHash(identityFolder);
                if (info.IdentityHash.Equals(hash)) return info;
            }

            // Not found
            return null;
        }

        /*
        public ConfiguredStore Store(string name)
        {
            ConfiguredStore store = null;
            Stores.TryGetValue(name, out store);
            return store;
            return null;
        }
        */

        /*
        public void AddStore(string name, Dictionary settings)
        {
            if (!Utility.IsValidName(name)) return;
            if (Stores.ContainsKey(name)) return;
            var settingsFile = Folder + "\\stores\\" + name;
            var store = ConfiguredStore.Create(name, settingsFile, settings);
            Stores[name] = store;
            return store;
        }

        public bool RemoveStore(string name)
        {
            ConfiguredStore store = null;
            if (!Stores.TryGetValue(name, out store)) return true;
            Stores.Remove(name);
            File.Delete(store.SettingsFile);
            return true;
        }
        */



        /*
        public void readSettings()
        {
            readSettingsFile(settingsFolder + @"\settings");
        }

        void readSettingsFile(string file)
        {
            // Check if the backend configuration exists
            try
            {
                var fh = File.OpenText(file);
                while (true)
                {
                    var line = fh.ReadLine();
                    if (line == null) break;

                    var match = Regex.Match(line, @"^\s*local\s+folder\s+(.*?)\s*$");
                    if (match.Success)
                    {
                        var item = new Burrow.FolderStore(match.Groups[1].Value);
                        if (item.isValid()) localFolderObjectStores.Add(item);
                    }

                    match = Regex.Match(line, @"^\s*forest\s+server\s+(.*?)\s*$");
                    if (match.Success)
                    {
                        var item = new Burrow.HTTPStore(match.Groups[1].Value);
                        if (item.isValid()) forestServerObjectStores.Add(item);
                    }

                }
                fh.Close();
            }
            catch
            {
                // Unable to read the settings - perhaps they don't exist yet
            }
        }

        public bool WriteSettings()
        {
            try
            {
                var fh = File.CreateText(settingsFolder + @"\settings");
                fh.WriteLine("# This configuration file is written automatically by SafeBox.");
                fh.WriteLine("# You should quit SafeBox before modifying it. Invalid lines are silently ignored.");
                foreach (var item in localFolderObjectStores) fh.WriteLine("local folder " + item.folder);
                foreach (var item in forestServerObjectStores) fh.WriteLine("forest server " + item.host);
                fh.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public ConfiguredIdentity CreateIdentity(string name)
        {
            return new ConfiguredIdentity(name);
        } */
    }

    public class ConfigurationSnapshot {
        public readonly Configuration Configuration;
        public readonly ImmutableStack<ObjectStore> Stores;
        //public readonly ImmutableStack<UnlockedPrivateIdentity> UnlockedIdentities;
        public readonly ImmutableStack<PrivateIdentity> Identities;
        public readonly ImmutableStack<ObjectStore> PermanentStores;
        public readonly DateTime UtcCreation = DateTime.UtcNow;

        public ConfigurationSnapshot(Configuration configuration, ImmutableStack<ObjectStore> objectStores, ImmutableStack<PrivateIdentity> identities)
        {
            this.Configuration = configuration;
            this.Stores = objectStores;
            this.Identities = identities;
            this.PermanentStores = objectStores; // ImmutableStack.From(Stores.Where(store => store.IsPermanent));
        }

        public PrivateIdentity IdentityByHash(Hash hash)
        {
            foreach (var identity in Identities)
                if (identity.PublicIdentity.PublicKey.Hash.Equals(hash)) return identity;
            return null;
        }

        /*
        public List<AbstractAccount> AccountsForIdentityHash(Hash identityHash)
        {
            var accounts = new List<AbstractAccount>();
            foreach (var store in Stores)
            {
                var account = store.Account(identityHash);
                if (account == null) continue;
                accounts.Add(account);
            }
            return accounts;
        }

        public Dictionary<string, SerializedObject> PublicKeysForIdentityHash(Hash identityHash)
        {
            var publicKeys = new Dictionary<string, SerializedObject>();
            foreach (var account in AccountsForIdentityHash(identityHash))
            {
                var accountIdentity = AccountIdentity(account);
                if (accountIdentity == null) continue;
                foreach (var hash in accountIdentity.PublicKeyHashes())
                {
                    if (publicKeys.ContainsKey(hash.Hex())) continue;
                    publicKeys[hash.Hex()] = GetObject(hash);
                }
            }
            return publicKeys;
        }*/
    }

    public class IdentityFolderForHash
    {
        public readonly string Folder;
        public readonly Hash IdentityHash;
        public readonly byte[] PublicKeyBytes;

        public IdentityFolderForHash(string identityFolder)
        {
            this.Folder = identityFolder;
            this.PublicKeyBytes = Static.ReadFile(identityFolder + "\\public-key", null);
            this.IdentityHash = PublicKey.IdentityHashForPublicKeyBytes(PublicKeyBytes);
        }
    }
}
