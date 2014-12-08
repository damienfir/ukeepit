using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace uKeepIt.MiniBurrow.Folder
{
    public class Root
    {
        public readonly string Folder;

        public Root(string folder)
        {
            this.Folder = folder;
        }

        public IEnumerable<ObjectUrl> List()
        {
            var list = new ImmutableStack<ObjectUrl>();
            foreach (var file in MiniBurrow.Static.DirectoryEnumerateFiles(Folder))
            {
                var name = Path.GetFileName(file);
                if (name.Length < 64) continue;
                var hash = Hash.From(name.Substring(0, 64));
                if (hash == null) continue;
                var url = MiniBurrow.Static.FileUtf8Text(file, null as string);
                list = list.With(new ObjectUrl(url, hash));
            }

            return list;
        }

        public bool Modify(IEnumerable<ObjectUrl> add, IEnumerable<Hash> remove)
        {
            // List of files to remove
            var hexHashesToRemove = new SortedSet<string>();
            foreach (var hash in remove) hexHashesToRemove.Add(hash.Hex());

            // Create the folder to add hashes if necessary
            MiniBurrow.Static.DirectoryCreate(Folder, LogLevel.Warning);

            // Add
            var sha256 = new System.Security.Cryptography.SHA256Managed();
            foreach (var objectUrl in add)
            {
                var url = objectUrl.Url;
                var fileContent = url == null ? new ArraySegment<byte>(new byte[0]) : new ArraySegment<byte>(Encoding.UTF8.GetBytes(url));
                var temporaryFile = Folder + "\\." + MiniBurrow.Static.RandomHex(16);
                if (!MiniBurrow.Static.WriteFile(temporaryFile, fileContent)) return false;
                var suffix = url == null ? "" : "-" + MiniBurrow.Serialization.Text.ToHexString(sha256.ComputeHash(fileContent.Array), 0, 16);
                var hashHex = objectUrl.Hash.Hex();
                var file = Folder + '\\' + hashHex + suffix;
                MiniBurrow.Static.FileMove(temporaryFile, file, LogLevel.Warning);
                MiniBurrow.Static.FileDelete(temporaryFile, LogLevel.Warning);
                if (!File.Exists(file)) return false;
                hexHashesToRemove.Remove(hashHex);
            }

            // Remove (and ignore all errors)
            if (hexHashesToRemove.Count > 0)
                foreach (var file in MiniBurrow.Static.DirectoryEnumerateFiles(Folder))
                {
                    var name = Path.GetFileName(file);
                    if (name.Length < 64) continue;
                    if (hexHashesToRemove.Contains(name.Substring(0, 64))) Burrow.Static.FileDelete(file, LogLevel.Warning);
                }

            return true;
        }
    }
}
