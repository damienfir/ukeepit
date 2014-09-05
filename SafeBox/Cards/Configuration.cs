using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow;
using System.Threading;
using System.IO;

namespace SafeBox.Cards
{
    public class Configuration
    {
        // Current Burrow configuration snapshot
        public readonly string ConfigurationFolder ;
        private Burrow.Configuration.Snapshot burrowSnapshot;
        
        // Cached passwords and keys
        public static ImmutableDictionary<Hash, PrivateKey> PrivateKeys = new ImmutableDictionary<Hash, PrivateKey>();
        public static ImmutableStack<ArraySegment<byte>> Passwords = new ImmutableStack<ArraySegment<byte>>();
        public static ImmutableStack<ArraySegment<byte>> Keys = new ImmutableStack<ArraySegment<byte>>();

        // Fired on the main thread whenever the configuration has been reloaded
        public delegate void ReloadedHandler(Burrow.Configuration.Snapshot burrowSnapshot);
        public event EventHandler<EventArgs> Reloaded;

        // 
        public readonly Dictionary<string, SharedSpacesList> SharedSpacesListByHash = new Dictionary<string, SharedSpacesList>();

        public Configuration()
        {
            // Determine the configuration folder
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            ConfigurationFolder  = appDataFolder + "\\burrow-data.org";

            // Create the default identity if necessary
            CreateDefaultIdentity(new ImmutableDictionary<string, string>(), new ImmutableDictionary<string, string>());

            // Load the configuration
            this.burrowSnapshot = Burrow.Configuration.FromFolder.Load(ConfigurationFolder , new ImmutableDictionary<Hash, PrivateKey>(), new ImmutableStack<ArraySegment<byte>>(), new ImmutableStack<ArraySegment<byte>>());
        }

        public Burrow.Configuration.Snapshot BurrowSnapshot { get { return burrowSnapshot; } }

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
        }

        public void Reload(ReloadedHandler handler = null) { ThreadPool.QueueUserWorkItem(new WaitCallback(obj => ReloadAsync(handler))); }

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

        /*
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
         * */

        // This creates an identity "default" unless it exists
        public void CreateDefaultIdentity(ImmutableDictionary<string, string> privateInformation, ImmutableDictionary<string, string> publicInformation)
        {
            // Create the folder
            var folder =             ConfigurationFolder +"\\identities\\default";
            if (Directory.Exists(folder) && File.Exists(folder+"\\public-key")) return;
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
            Burrow.Static.WriteFile(folder+"\\encrypted-private-rsa-key", encryptedPrivateKey);

            // Write the configuration file
            var configuration = new Burrow.Configuration.IniFile();
            var privateSection= configuration.Section("private");
            foreach (var pair in privateInformation) privateSection.Set(pair.Key, pair.Value);
            var publicSection= configuration.Section("public");
            foreach (var pair in publicInformation) publicSection.Set(pair.Key, pair.Value);
            var privateKeyInfoSection= configuration.Section("private key");
            privateKeyInfoSection.Set("aes iv", aesIv);
            Burrow.Static.WriteFile(folder+"\\configuration", configuration.ToBytes());
        }
    }
}
