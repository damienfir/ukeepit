using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using SafeBox.Burrow.Backend;
using SafeBox.Burrow;
using SafeBox.Burrow.Serialization;

namespace SafeBox.Burrow.Configuration
{
    public class FromFolder
    {
        public static Snapshot Load(string folder, ImmutableDictionary<Hash, PrivateKey> knownPrivateKeys, ImmutableStack<ArraySegment<byte>> knownPasswords, ImmutableStack<ArraySegment<byte>> knownAesKeys)
        {
            // Read all identities
            var identities = new ImmutableDictionary<Hash, PrivateIdentity>();
            var identitiesFolder = folder + "\\identities";
            foreach (var id in Static.DirectoryEnumerateDirectories(identitiesFolder))
            {
                var identity = LoadIdentity(identitiesFolder + "\\" + id, knownPrivateKeys, knownPasswords, knownAesKeys);
                if (identity != null) identities = identities.With(identity.PublicKey.Hash, identity);
            }

            // Read all object stores
            var objectStoresByName = new Dictionary<string, ObjectStore>();
            var accountStoresByName = new Dictionary<string, AccountStore>();
            var objectStores = new ImmutableStack<ObjectStore>();
            var accountStores = new ImmutableStack<AccountStore>();
            var permanentObjectStores = new ImmutableStack<ObjectStore>();
            var permanentAccountStores = new ImmutableStack<AccountStore>();
            var stores = IniFile.From(Static.FileUtf8Lines(folder + "/stores", new string[0]));
            foreach (var pair in stores.SectionsByName)
            {
                var permanent = pair.Value.Get("permanent").ToBool(false);
                var url = pair.Value.Get("url", null as string);

                // Object store
                var objectsUrl = pair.Value.Get("objects url", null as string);
                if (objectsUrl == null && url != null) objectsUrl = url + "/objects";
                if (objectsUrl != null)
                {
                    var store = Static.ObjectStoreForUrl(objectsUrl, pair.Key);
                    if (permanent) permanentObjectStores = permanentObjectStores.With(store);
                    objectStores = objectStores.With(store);
                    objectStoresByName[pair.Key] = store;
                }

                // Account store
                var accountsUrl = pair.Value.Get("accounts url", null as string);
                if (accountsUrl == null && url != null) accountsUrl = url + "/objects";
                if (accountsUrl != null)
                {
                    var store = Static.AccountStoreForUrl(accountsUrl, pair.Key);
                    if (permanent) permanentAccountStores = permanentAccountStores.With(store);
                    accountStores = accountStores.With(store);
                    accountStoresByName[pair.Key] = store;
                }
            }

            return new Snapshot(identities, permanentObjectStores, permanentAccountStores, objectStores, accountStores);
        }

        public static PrivateIdentity LoadIdentity(string identityFolder, ImmutableDictionary<Hash, PrivateKey> knownPrivateKeys, ImmutableStack<ArraySegment<byte>> knownPasswords, ImmutableStack<ArraySegment<byte>> knownAesKeys)
        {
            // Read the configuration file
            var configuration = IniFile.From(Static.FileUtf8Lines(identityFolder + "\\configuration", new string[0]));

            // Read the public key
            var publicKeyBytes = Static.FileBytes(identityFolder + "\\public-rsa-key").ToByteSegment();
            var publicKeyObject = BurrowObject.From(publicKeyBytes);
            if (publicKeyObject == null) return null;
            var publicKey = PublicKey.From(publicKeyObject);
            if (publicKey == null) return null;

            // Check if we have a private key for this
            var privateKey = knownPrivateKeys.Get(publicKey.Hash);
            if (privateKey == null) privateKey = PrivateKey.From(configuration.Section("private key"));
            if (privateKey != null) return new PrivateIdentity(configuration, publicKey, publicKeyBytes, privateKey);

            // Check if we have an AES IV to decrypt a key
            var section = configuration.Section("encrypted private key");
            var aesIv = section.Get("aes iv").ToByteArray();
            if (aesIv.Length != 16) return new PrivateIdentity(configuration, publicKey, publicKeyBytes, null);

            // Load the encrypted private key
            var encryptedPrivateKeySegment = Static.FileBytes(identityFolder + "\\aes-encrypted-private-rsa-key").ToByteSegment();
            if (encryptedPrivateKeySegment.Array == null) return new PrivateIdentity(configuration, publicKey, publicKeyBytes, null);

            // Try decrypting with all known keys
            foreach (var aesKey in knownAesKeys)
            {
                var decrypted = Aes.Decrypt(encryptedPrivateKeySegment, Static.ToByteArray( aesKey), aesIv);
                var iniFile = IniFile.From(decrypted);
                privateKey = PrivateKey.From(iniFile.Section("private key"));
                if (privateKey != null) return new PrivateIdentity(configuration, publicKey, publicKeyBytes, privateKey);
            }

            // Try decrypting with all known passwords
            var aeskdfIv = section.Get("aeskdf iv").ToByteArray(Static.EmptyByteArray);
            var aeskdfIterations = section.Get("aeskdf iterations").ToInt();
            if (aeskdfIterations == 0 || aeskdfIv.Length != 32) return new PrivateIdentity(configuration, publicKey, publicKeyBytes, null);
            foreach (var password in knownPasswords)
            {
                // Create the passwort digest
                var sha256 = new System.Security.Cryptography.SHA256Managed();
                var passwordHash = sha256.ComputeHash(Static.ToByteArray(password));

                // Apply AES iterations
                var aes = new System.Security.Cryptography.AesManaged();
                aes.Mode = System.Security.Cryptography.CipherMode.ECB;
                var iterationBuffer = new byte[32];
                Array.Copy(aeskdfIv, iterationBuffer, 32);
                var encryptor = aes.CreateEncryptor(passwordHash, new byte[16]);
                for (var i = 0; i < aeskdfIterations; i++)
                {
                    encryptor.TransformBlock(iterationBuffer, 0, 16, iterationBuffer, 0);
                    encryptor.TransformBlock(iterationBuffer, 16, 16, iterationBuffer, 16);
                }

                // Try decrypting the private key
                var decrypted = Aes.Decrypt(encryptedPrivateKeySegment, iterationBuffer, aesIv);
                var iniFile = IniFile.From(decrypted);
                privateKey = PrivateKey.From(iniFile.Section("private key"));
                if (privateKey != null) return new PrivateIdentity(configuration, publicKey, publicKeyBytes, privateKey);
            }

            // We cannot decrypt the private key
            return new PrivateIdentity(configuration, publicKey, publicKeyBytes, null);
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

        public static ImmutableStack<IdentityFolder> IdentityFolders(string folder)
        {
            var identityFolders = new ImmutableStack<IdentityFolder>();
            var identitiesFolder = folder + "\\identities";
            foreach (var id in Static.DirectoryEnumerateDirectories(identitiesFolder))
            {
                identityFolders = identityFolders.With( new IdentityFolder(identitiesFolder + "\\" + id));
            }

            return identityFolders;
        }

    }

    public class IdentityFolder
    {
        public readonly string Folder;
        public readonly Hash IdentityHash;
        public readonly byte[] PublicKeyBytes;

        public IdentityFolder(string identityFolder)
        {
            this.Folder = identityFolder;
            this.PublicKeyBytes = Static.FileBytes(identityFolder + "\\public-rsa-key", null);
            this.IdentityHash = Hash.For(PublicKeyBytes);
        }
    }
}
