using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using SafeBox.Burrow.Serialization;
using SafeBox.Burrow.Abstract;

namespace SafeBox.Burrow
{
    class Static
    {
        // For simplicity, the following variables are allocated statically (global variables).
        // While an application may work with multiple configurations (unlikely, but thinkable), it makes sense that they all share these objects.
        public static readonly Log Log = new Log();
        public static System.Threading.SynchronizationContext SynchronizationContext;
        public static Random Random = new Random();

        // Conversion from byte array to hex string, and vice versa
        public static string HexChars = "0123456789abcdef";
        public static char ToHexChar(int value) { return HexChars[value & 0xf]; }

        public static int FromHexChar(char c)
        {
            var value = (int)c;
            if (value >= 48 && value <= 57) return value - 48;
            if (value >= 65 && value <= 70) return value - 65 + 10;
            if (value >= 97 && value <= 102) return value - 97 + 10;
            return 0;
        }

        public static string BytesToHexString(byte[] bytes) { return BytesToHexString(bytes, 0, bytes.Length); }

        public static string BytesToHexString(byte[] bytes, int offset, int count)
        {
            char[] hex = new char[count * 2];
            for (var i = 0; i < count; i++)
            {
                hex[i * 2] = ToHexChar(bytes[offset + i] >> 4);
                hex[i * 2 + 1] = ToHexChar(bytes[offset + i]);
            }
            return new string(hex);
        }

        public static byte[] HexStringToBytes(string hex)
        {
            byte[] bytes = new byte[hex.Length >> 1];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)((FromHexChar(hex[i * 2]) << 4) | FromHexChar(hex[i * 2 + 1]));
            }
            return bytes;
        }

        public static string RandomHex(int length)
        {
            var chars = new char[length];
            for (var i = 0; i < length; i++) chars[i] = ToHexChar(Random.Next());
            return new string(chars);
        }

        // Safe names for use on filesystems and elsewhere
        /* public static bool IsValidName(string name)
        {
            if (name.Length < 1) return false;

            var f = name[0];
            if (f != '_' && f != '#' && f != '=' && f != '@' && f != '+' && f != '-' && (f < 'a' || f > 'z') && (f < '0' || f > '9'))
                return false;

            foreach (var c in name)
                if (c != '_' && c != '#' && c != '=' && c != '@' && c != '+' && c != '-' && c != '.' && c != '!' && (c < 'a' || c > 'z') && (c < '0' || c > '9'))
                    return false;

            return true;
        } */

        // Object store creation
        public static Abstract.ObjectStore ObjectStoreForUrl(string url, string logName)
        {
            if (url == null) return null;

            // Show the URL to all available backends
            var store = Folder.ObjectStore.ForUrl(url);
            if (store != null) return store;

            Static.Log.Error("Store '" + logName + "': Invalid or unsupported URL '" + url + "'.");
            return null;
        }

        // Size formatting
        static Regex NumberWithUnit = new Regex(@"^\s*([0-9\.]+)\s*([A-Za-z]*)");

        public static long ParseByteSize(string text)
        {
            var match = NumberWithUnit.Match(text);
            if (!match.Success) return -1;
            var value = 0.0;
            Double.TryParse(match.Groups[1].Value, out value);
            var unit = match.Groups[2].Value.ToLower();
            if (unit == "kib") return (long)(value * 1024);
            if (unit == "mib") return (long)(value * 1024 * 1024);
            if (unit == "gib") return (long)(value * 1024 * 1024 * 1024);
            if (unit == "tib") return (long)(value * 1024 * 1024 * 1024 * 1024);
            if (unit == "kb") return (long)(value * 1000);
            if (unit == "mb") return (long)(value * 1000 * 1000);
            if (unit == "gb") return (long)(value * 1000 * 1000 * 1000);
            if (unit == "tb") return (long)(value * 1000 * 1000 * 1000 * 1000);
            return (long)value;
        }

        // File and directory operations
        public static byte[] ReadFile(string file, byte[] defaultBytes)
        {
            try { return File.ReadAllBytes(file); }
            catch (Exception ex)
            {
                Static.Log.Error("Failed to read file '" + file + "'. " + ex.ToString());
                return defaultBytes;
            }
        }

        public static byte[] ReadFileSilent(string file, byte[] defaultBytes)
        {
            try { return File.ReadAllBytes(file); }
            catch (Exception) { return defaultBytes; }
        }

        public static string ReadFileSilent(string file, string defaultText)
        {
            try { return System.IO.File.ReadAllText(file, Encoding.UTF8); }
            catch (Exception) { return defaultText; }
        }

        public static bool WriteFile(string file, ArraySegment<byte> bytes) { return WriteFile(file, bytes.Array, bytes.Offset, bytes.Count); }
        public static bool WriteFile(string file, byte[] bytes) { return WriteFile(file, bytes, 0, bytes.Length); }
        public static bool WriteFile(string file, byte[] bytes, int offset, int count)
        {
            FileStream stream = null;
            try
            {
                stream = File.Create(file);
                stream.Write(bytes, offset, count);
                return true;
            }
            catch (Exception ex)
            {
                Static.Log.Error("Failed to write file '" + file + "'. " + ex.ToString());
                return false;
            }
            finally
            {
                if (stream != null) stream.Close();
            }
        }

        public static bool FileMove(string source, string destination)
        {
            try
            {
                File.Move(source, destination);
                return true;
            }
            catch (Exception ex)
            {
                Static.Log.Error("Failed to move file '" + source + "' to '" + destination + "'. " + ex.ToString());
                return false;
            }
        }

        public static bool FileMoveSilent(string source, string destination)
        {
            try
            {
                File.Move(source, destination);
                return true;
            }
            catch (Exception) { return false; }
        }

        public static bool FileDelete(string file)
        {
            try
            {
                File.Delete(file);
                return true;
            }
            catch (Exception ex)
            {
                Static.Log.Error("Failed to delete file '" + file + "'." + ex.ToString());
                return false;
            }
        }

        public static bool FileDeleteSilent(string file)
        {
            try
            {
                File.Delete(file);
                return true;
            }
            catch (Exception) { return false; }
        }

        public static bool DirectoryMove(string source, string destination)
        {
            try
            {
                Directory.Move(source, destination);
                return true;
            }
            catch (Exception ex)
            {
                Static.Log.Error("Failed to move '" + source + "' to '" + destination + "'. " + ex.ToString());
                return false;
            }
        }

        public static bool DirectoryDelete(string directory)
        {
            try
            {
                Directory.Delete(directory);
                return true;
            }
            catch (Exception ex)
            {
                Static.Log.Error("Failed to remove '" + directory + "'. " + ex.ToString());
                return false;
            }
        }

        public static void DirectoryCreateSilent(string folder)
        {
            try { Directory.CreateDirectory(folder); }
            catch (Exception) { }
        }

        public static bool DirectoryCreate(string folder)
        {
            try
            {
                Directory.CreateDirectory(folder);
                return true;
            }
            catch (Exception ex)
            {
                Static.Log.Error("Failed to create folder '" + folder + "'. " + ex.ToString());
                return false;
            }
        }

        public static IEnumerable<string> DirectoryEnumerateFilesSilent(string folder)
        {
            try { return Directory.EnumerateFiles(folder); }
            catch (Exception) { return new string[0]; }
        }

        public static IEnumerable<string> DirectoryEnumerateFiles(string folder)
        {
            try { return Directory.EnumerateFiles(folder); }
            catch (Exception ex)
            {
                Static.Log.Error("Failed to enumerate files in folder '" + folder + "'. " + ex.ToString());
                return new string[0];
            }
        }

        public static IEnumerable<string> DirectoryEnumerateDirectories(string folder)
        {
            try { return Directory.EnumerateDirectories(folder); }
            catch (Exception ex)
            {
                Static.Log.Error("Failed to enumerate directories in folder '" + folder + "'. " + ex.ToString());
                return new string[0];
            }
        }

        public static IEnumerable<string> DirectoryEnumerateDirectoriesSilent(string folder)
        {
            try { return Directory.EnumerateDirectories(folder); }
            catch (Exception) { return new string[0]; }
        }


        public static BurrowObject CreateEnvelope(HashWithAesParameters hashWithAesParameters, UnlockedPrivateIdentity sender, IEnumerable<PublicIdentity> receiver)
        {
            // Create serialization structures
            var hashCollector = new HashCollector();

            // Create an envelope
            var envelope = new DictionaryConstructor();
            envelope.Add("content", hashWithAesParameters.Hash);

            // Set sender and signature
            var senderHash = sender.PrivateIdentity.PublicIdentity.PublicKey.Hash;
            envelope.Add("sender", senderHash);
            envelope.Add("signature", sender.PrivateKey.Sign(hashWithAesParameters.Hash));
            
            // Add encrypted AES keys
            var encryptedAesKeysDictionary = new DictionaryConstructor();
            encryptedAesKeysDictionary.Add(senderHash.Bytes(), sender.PrivateIdentity.PublicIdentity.PublicKey.Encrypt(hashWithAesParameters.Key));
            envelope.Add("encrypted aes key", encryptedAesKeysDictionary.Serialize(hashCollector));
            envelope.Add("aes iv", hashWithAesParameters.Iv);

            // Add the encrypted content keys
            foreach (var publicIdentity in receiver)
            {
                var hashHex = publicIdentity.PublicKey.Hash.Hex();
                if (senderHash !=publicIdentity.PublicKey.Hash)
                    encryptedAesKeysDictionary.Add(hashHex, publicIdentity.PublicKey.Encrypt(hashWithAesParameters.Key));
                foreach (var publicKey in publicIdentity.PublicKeys)
                    if (publicKey.Hash != publicIdentity.PublicKey.Hash)
                        encryptedAesKeysDictionary.Add(publicKey.Hash.Hex(), publicKey.Encrypt(hashWithAesParameters.Key));
            }

            // TODO: add AES keys for symmetric keys (shared secrets)
            return BurrowObject.For(hashCollector, envelope.Serialize(hashCollector));
        }

        public static HashWithAesParameters OpenEnvelope(BurrowObject obj, UnlockedPrivateIdentity identity, IEnumerable<PublicIdentity> publicIdentities)
        {
            var envelope = Dictionary.From(obj);

            // Get the content hash
            var contentHash = envelope.GetHash("content");
            if (contentHash == null) return null;

            // Find the signer
            var senderHash = envelope.GetHash("sender");
            if (senderHash == null) return null;

            PublicIdentity signer = null;
            foreach (var publicIdentity in publicIdentities)
            {
                if (!publicIdentity.PublicKey.Hash.Equals(senderHash)) continue;
                signer = publicIdentity;
                break;
            }
            //if (signer == null) return null;

            // Check the signature (TODO: check always, not just if the signer is known)
            if (signer != null)
            {
                var signature = envelope.Get("signature");
                if (signature == null) return null;
                if (!signer.PublicKey.VerifySignature(contentHash, signature)) return null;
            }

            // Decrypt the AES key
            var encryptedAESKeys = Dictionary.From(obj, envelope.Get("encrypted aes key"));
            var encryptedAESKey = encryptedAESKeys.Get(identity.PrivateIdentity.PublicIdentity.PublicKey.Hash.Bytes());
            if (encryptedAESKey == null) return null;
            var aesKey = identity.PrivateKey.Decrypt(encryptedAESKey);
            if (aesKey == null) return null;
            var aesIv = envelope.Get("aes iv");
            if (aesIv.Array == null) return null;

            // Returns the hash with the AES parameters (TODO: return the hash of the signer, since some content will only be allowed from some signers)
            return new HashWithAesParameters(contentHash, new ArraySegment<byte>(aesKey), aesIv);
        }

        private ImmutableStack<ObjectStore> CreateObjectStoreList(IEnumerable<string> urls, ImmutableStack<ObjectStore> knownObjectStores)
        {
            var objectStores = new ImmutableStack<ObjectStore>();

            // Add all stores indicated by the object urls
            foreach (var url in urls)
            {
                if (url == null) continue;

                // Check if this store is in the list already
                var found = false;
                foreach (var store in objectStores)
                    if (store.Url == url) { found = true; break; }
                if (found) continue;

                // Try adding a known object store
                foreach (var store in knownObjectStores)
                    if (store.Url == url) { found = true; objectStores = objectStores.With(store); break; }
                if (found) continue;

                // Create and add a new store
                objectStores = objectStores.With(Static.ObjectStoreForUrl(url, url));
            }

            // Add all known object stores
            foreach (var store in knownObjectStores)
            {
                if (objectStores.Contains(store)) continue;
                objectStores = objectStores.With(store);
            }

            // TODO: order the stores by expected availability, to minimize the delay

            // We are done
            return objectStores;
        }

        public static string Utf8BytesToText(ArraySegment<byte> bytes)
        {
            try { return Encoding.UTF8.GetString(bytes.Array, bytes.Offset, bytes.Count); }
            catch (Exception) { return null; }
        }

        public static string Utf8BytesToText(byte[] bytes, int offset, int count)
        {
            try { return Encoding.UTF8.GetString(bytes, offset, count); }
            catch (Exception) { return null; }
        }

        public static string Utf8BytesToText(byte[] bytes)
        {
            try { return Encoding.UTF8.GetString(bytes); }
            catch (Exception) { return null; }
        }

        public static byte[] TextToUtf8Bytes(string text)
        {
            try { return Encoding.UTF8.GetBytes(text); }
            catch (Exception) { return null; }
        }

        public static string UtcDateToText(DateTime utcDate) { return utcDate.ToString("yyyyMMddTHHmmssZ"); }

        public static DateTime TextToUtcDate(string text, DateTime defaultUtcDate)
        {
            text = text.Trim();
            if (text.Length != 16) return defaultUtcDate;
            if (text[8] != 'T') return defaultUtcDate;
            if (text[15] != 'Z') return defaultUtcDate;

            var year = DatePart(text, 0, 4, 1, 9999);
            if (year < 0) return defaultUtcDate;

            var month = DatePart(text, 4, 2, 1, 12);
            if (month < 0) return defaultUtcDate;

            var day = DatePart(text, 6, 2, 1, 31);
            if (day < 0) return defaultUtcDate;

            var hour = DatePart(text, 9, 2, 0, 60);
            if (hour < 0) return defaultUtcDate;

            var minute = DatePart(text, 11, 2, 0, 60);
            if (minute < 0) return defaultUtcDate;

            var second = DatePart(text, 13, 2, 0, 60);
            if (second < 0) return defaultUtcDate;

            return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
        }

        private static int DatePart(string text, int offset, int length, int min, int max)
        {
            int v = 0;
            for (int i = 0; i < length; i++)
            {
                var c = text[offset + i];
                if (c < '0' || c > '9') return -1;
                v = v * 10 + (c - '0');
            }
            if (v < min) return -1;
            if (v > max) return -1;
            return v;
        }

        // DateTime conversion
        public static long UnixEpochTicks = new DateTime(1970, 1, 1, 0, 0, 0, 0).Ticks;
        public static long Timestamp(DateTime d) { return (d.Ticks - UnixEpochTicks ) / 10000; }
        public static DateTime Timestamp(long t) { return new DateTime((t + UnixEpochTicks) * 10000); }

        // Compare byte array segments
        public static int Compare(ArraySegment<byte> segment0, ArraySegment<byte> segment1)
        {
            var minCount = segment0.Count < segment1.Count ? segment0.Count : segment1.Count;
            for (var n = 0; n < minCount; n++)
            {
                if (segment0.Array[segment0.Offset + n] > segment1.Array[segment1.Offset + n]) return 1;    // abd > abc
                if (segment0.Array[segment0.Offset + n] < segment1.Array[segment1.Offset + n]) return -1;   // abc < abd
            }
            if (segment0.Count > segment1.Count) return 1;    // abcd > ab
            if (segment0.Count < segment1.Count) return -1;   // ab < abcd
            return 0;
        }

        public static bool IsEqual(ArraySegment<byte> segment0, ArraySegment<byte> segment1)
        {
            if (segment0.Count != segment1.Count) return false;
            for (var n = 0; n < segment0.Count; n++)
                if (segment0.Array[segment0.Offset + n] != segment1.Array[segment1.Offset + n]) return false;
            return true;
        }

        public static byte[] ByteArray(ArraySegment<byte> segment) {
            if (segment.Offset==0 && segment.Count == segment.Array.Length) return segment.Array;
            var bytes = new byte[segment.Count];
            Array.Copy(segment.Array, segment.Offset, bytes, 0, segment.Count);
            return bytes;
        }
    }
}
