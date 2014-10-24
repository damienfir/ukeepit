using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow;
using System.Threading;
using System.IO;
using SafeBox.Burrow.Configuration;
using System.Windows.Threading;

namespace SafeBox.Cards
{
    public class Configuration
    {
        // Current Burrow configuration snapshot
        public readonly string ConfigurationFolder ;
        private Burrow.Configuration.Snapshot burrowSnapshot;
        private DispatcherTimer reloadTimer;
        private delegate void DelayedReloadDelegate();
        
        // Cached passwords and keys
        public static ImmutableDictionary<Hash, PrivateKey> PrivateKeys = new ImmutableDictionary<Hash, PrivateKey>();
        public static ImmutableStack<ArraySegment<byte>> Passwords = new ImmutableStack<ArraySegment<byte>>();
        public static ImmutableStack<ArraySegment<byte>> Keys = new ImmutableStack<ArraySegment<byte>>();

        // Fired on the main thread whenever the configuration has been reloaded
        public delegate void ReloadedHandler(Burrow.Configuration.Snapshot burrowSnapshot);
        public event EventHandler<EventArgs> Reloaded;

        // Shared spaces
        //public readonly Dictionary<string, SharedSpacesList> SharedSpacesListByHash = new Dictionary<string, SharedSpacesList>();

        public Configuration()
        {
            // Field initialization
            reloadTimer = new DispatcherTimer(new TimeSpan(10000 * 200), DispatcherPriority.Normal, new EventHandler(DelayedReload), Dispatcher.CurrentDispatcher);

            // Determine the configuration folder
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            ConfigurationFolder  = appDataFolder + "\\burrow-data.org";

            // Create the default identity if no identity exists
            CreateDefaultIdentityIfNecessary();

            // Load the configuration
            this.burrowSnapshot = Burrow.Configuration.FromFolder.Load(ConfigurationFolder , new ImmutableDictionary<Hash, PrivateKey>(), new ImmutableStack<ArraySegment<byte>>(), new ImmutableStack<ArraySegment<byte>>());

            // Set up file change notification for the configuration folder
            var watcher = new System.IO.FileSystemWatcher(ConfigurationFolder);
            watcher.Changed += new FileSystemEventHandler(OnConfigurationChanged);
            watcher.Created += new FileSystemEventHandler(OnConfigurationChanged);
            watcher.Deleted += new FileSystemEventHandler(OnConfigurationChanged);
            watcher.Renamed += new RenamedEventHandler(OnConfigurationChanged);
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
        }

        public Burrow.Configuration.Snapshot BurrowSnapshot { get { return burrowSnapshot; } }

        private void OnConfigurationChanged(object source, FileSystemEventArgs e)
        {
            Burrow.Static.Log.Info("File: " + e.FullPath + " " + e.ChangeType);
            Reload();
        }

        /*
        public SharedSpacesList SharedSpacesList(Burrow.Configuration.PrivateIdentity identity)
        {
            var sharedSpacesList = null as SharedSpacesList;
            if (SharedSpacesListByHash.TryGetValue(identity.PublicKey.Hash.Hex(), out sharedSpacesList)) return sharedSpacesList;
            sharedSpacesList = new SharedSpacesList(this, identity);
            SharedSpacesListByHash[identity.PublicKey.Hash.Hex()] = sharedSpacesList;
            return sharedSpacesList;
        }

        public SharedSpace SharedSpaceForKey(string key)
        {
            // Check if we have this one
            foreach (var sharedSpacesList in SharedSpacesListByHash.Values)
            {
                var sharedSpace = sharedSpacesList.Get(key);
                if (sharedSpace != null) return sharedSpace;
            }

            return null;
        } */

        //public void ReloadNow(ReloadedHandler handler = null) { 
        //    ThreadPool.QueueUserWorkItem(new WaitCallback(obj => ReloadAsync(handler))); 
        //}

        public void Reload() {
            Console.WriteLine("scheduling reload " + reloadTimer.IsEnabled);
            reloadTimer.IsEnabled = true;
        }

        private void DelayedReload(object timer, EventArgs e)
        {
            reloadTimer.IsEnabled = false;
            Console.WriteLine("reload");
            ThreadPool.QueueUserWorkItem(new WaitCallback(none => ReloadAsync(null)));
        }

        private void ReloadAsync(ReloadedHandler handler)
        {
            /*
            // If no stores are configured, create a main store
            if (stores.Length == 0)
            {
                var settings = new IniFileSection();
                settings.Set("url", Folder + "\\MainStore");
                settings.Set("permanent", true);
                settings.Set("create if necessary", true);
                Static.WriteFile(Folder + "\\stores\\main", new ArraySegment<byte>(settings.ToBytes()));
                stores = stores.With(Static.StoreForSettingsAsync(settings, "Newly created main store"));
            } */

            // Load the identities
            //var identitiesFolder = Folder + "\\identities";
            //Static.DirectoryCreate(identitiesFolder);

            // Current private keys
            var knownPrivateKeys = new ImmutableDictionary<Hash, PrivateKey>();
            foreach (var identity in burrowSnapshot.Identities)
                if (identity.Value.PrivateKey != null) knownPrivateKeys = knownPrivateKeys.With(identity.Key, identity.Value.PrivateKey);

            // Create a new snapshot, and notify the listeners
            var snapshot = Burrow.Configuration.FromFolder.Load(ConfigurationFolder , knownPrivateKeys, new ImmutableStack<ArraySegment<byte>>(), new ImmutableStack<ArraySegment<byte>>());
            Static.SynchronizationContext.Post(new SendOrPostCallback(obj =>
            {
                burrowSnapshot = snapshot;
                if (handler != null) handler(snapshot);
                if (Reloaded != null) Reloaded(this, new EventArgs());
            }), null);
        }

        public Burrow.Configuration.IniFile ReadConfiguration()
        {
            var configuration = Burrow.Configuration.IniFile.From(Burrow.Static.FileBytes(ConfigurationFolder + "\\configuration"));
            if (configuration == null) configuration = new Burrow.Configuration.IniFile();
            return configuration;
        }

        public bool WriteConfiguration(Burrow.Configuration.IniFile configuration) {
            return Burrow.Static.WriteFile(ConfigurationFolder + "\\configuration", configuration.ToBytes());
        }

        // This creates an identity "default" unless it exists
        public void CreateDefaultIdentityIfNecessary()
        {
            var windowsUser = System.DirectoryServices.AccountManagement.UserPrincipal.Current;
            var configuration = new Burrow.Configuration.IniFile();
            var privateInformation = configuration.Section("private");
            privateInformation.Set("name", windowsUser.DisplayName);
            var publicInformation = configuration.Section("public");
            publicInformation.Set("name", windowsUser.DisplayName);
            publicInformation.Set("first name", windowsUser.GivenName);
            publicInformation.Set("middle name", windowsUser.MiddleName);
            publicInformation.Set("last name", windowsUser.Name);
            CreateIdentity("default", configuration);
        }

        // Creates a new identity
        public void CreateIdentity(string id, IniFile configuration)
        {
            // Create the folder
            var folder =             ConfigurationFolder +"\\identities\\"+id;
            if (Directory.Exists(folder) && File.Exists(folder + "\\public-rsa-key")) return;
            Burrow.Static.DirectoryCreate(folder, LogLevel.Warning);

            // Create a key pair
            Static.Log.Info("Creating a key pair");
            var rsa = new System.Security.Cryptography.RSACryptoServiceProvider();
            var key = rsa.ExportParameters(true);

            // Write the public key
            var publicKey = new SafeBox.Burrow.Serialization.DictionaryConstructor();
            publicKey.Add("modulus", key.Modulus);
            publicKey.Add("exponent", key.Exponent);
            var publicKeyObject = publicKey.ToBurrowObject();
            var identityHash = publicKeyObject.Hash();
            Burrow.Static.WriteFile(folder+"\\public-rsa-key", publicKeyObject.Bytes);

            // Write the encrypted private key
            var privateKey = new Burrow.Configuration.IniFile();
            var privateKeySection = privateKey.Section("private key");
            privateKeySection.Set("modulus", key.Modulus);
            privateKeySection.Set("exponent", key.Exponent);
            privateKeySection.Set("d", key.D);
            privateKeySection.Set("p", key.P);
            privateKeySection.Set("q", key.Q);
            privateKeySection.Set("d mod p", key.DP);
            privateKeySection.Set("d mod q", key.DQ);
            privateKeySection.Set("inv(q) mod p", key.InverseQ);
            var aesKey = new byte[32];  // TODO: use key from windows credentials
            var aesIv = new byte[16];
            Burrow.Static.Random.NextBytes(aesIv);
            var encryptedPrivateKey = Burrow.Aes.Encrypt(new ArraySegment<byte>(privateKey.ToBytes()), aesKey, aesIv );
            Burrow.Static.WriteFile(folder+"\\aes-encrypted-private-rsa-key", encryptedPrivateKey);
            var privateKeyInfoSection = configuration.Section("private key");
            privateKeyInfoSection.Set("aes iv", aesIv);

            // Write the configuration file
            Burrow.Static.WriteFile(folder+"\\configuration", configuration.ToBytes());
        }
    }
}
