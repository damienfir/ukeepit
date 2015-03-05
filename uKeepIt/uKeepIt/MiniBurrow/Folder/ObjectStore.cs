using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using uKeepIt.MiniBurrow.Serialization;

namespace uKeepIt.MiniBurrow.Folder
{
    public class ObjectStore
    {
        public readonly string Folder;

        public ObjectStore(string folder)
        {
            this.Folder = folder;
        }

        public bool Has(Hash hash)
        {
            var hashHex = hash.Hex();
            var file = Folder + "\\" + hashHex.Substring(0, 2) + "\\" + hashHex.Substring(2);
            return File.Exists(file);
        }

        public List<string> List()
        {
            var folders = Directory.EnumerateDirectories(Folder).Where((x) => !x.Equals("spaces")).ToList();
            var hashes = new List<string>();
            foreach (var f in folders)
            {
                hashes.AddRange(Directory.EnumerateFiles(f));
            }
            return hashes;
        }

        public BurrowObject Get(Hash hash)
        {
            var hashHex = hash.Hex();
            var file = Folder + "\\" + hashHex.Substring(0, 2) + "\\" + hashHex.Substring(2);
            return BurrowObject.From(MiniBurrow.Static.FileBytes(file, null));
        }

        public Hash Put(BurrowObject obj)
        {
            // Check if that object exists already
            var hash = obj.Hash();
            var hashHex = hash.Hex();
            var folder = Folder + "\\" + hashHex.Substring(0, 2);
            var file = folder + "\\" + hashHex.Substring(2);
            if (File.Exists(file)) return hash;

            // Write the file, and move it to the right place
            if (!Directory.Exists(folder)) MiniBurrow.Static.DirectoryCreate(folder, LogLevel.Warning);
            var temporaryFile = folder + "\\." + MiniBurrow.Static.RandomHex(16);
            if (MiniBurrow.Static.WriteFile(temporaryFile, obj.Bytes) && MiniBurrow.Static.FileMove(temporaryFile, file, LogLevel.Warning) && File.Exists(file)) return hash;
            return null;
        }

        // For optimization purposes (garbage collection)
        public ImmutableStack<Hash> GetHashes(Hash hash)
        {
            var hashHex = hash.Hex();
            var file = Folder + "\\" + hashHex.Substring(0, 2) + "\\" + hashHex.Substring(2);

            try
            {
                // Open the file
                var list = new ImmutableStack<Hash>();
                var stream = File.OpenRead(file);

                // Read the number of hashes, and calculate the header length
                var count = new byte[4];
                stream.Read(count, 0, 4);
                var countHashes = (count[0] << 24) | (count[1] << 16) | (count[2] << 8) | count[3];
                var hashesLength = countHashes * 32;

                // If the object is not a valid burrow object, we pretend it has no hashes
                if (4 + hashesLength > stream.Length)
                {
                    stream.Close();
                    return list;
                }

                // Read the hashes
                var read = 0;
                var bytes = new byte[hashesLength];
                while (read < hashesLength)
                    read += stream.Read(bytes, read, hashesLength - read);
                stream.Close();

                // Process all hashes
                for (int i = 0; i < countHashes; i++)
                    list = list.With(Hash.From(bytes, i * 32));
                return list;
            }
            catch (Exception ex)
            {
                MiniBurrow.Static.Log.Message(LogLevel.Warning, "Failed to read file '" + file + "'. " + ex.ToString());
                return null;
            }
        }
    }
}
