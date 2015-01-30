using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace uKeepIt.MiniBurrow.Folder
{
    public class GarbageCollection
    {
        public readonly ImmutableStack<Store> stores;
        public readonly ImmutableStack<ObjectStore> objectStores;
        public readonly bool successful;

        public GarbageCollection(ImmutableStack<Store> stores, ImmutableStack<ObjectStore> objectStores)
        {
            this.stores = stores;
            this.objectStores = objectStores;
            successful = run();
        }

        private bool run()
        {
            // Don't delete files that are younger than 24 hours (to keep partially written or synchronized trees)
            var graceTime = DateTime.UtcNow; //.AddHours(-24);

            // Traverse all objects linked from any root
            HashSet<Hash> set = new HashSet<Hash>();
            foreach (var store in stores)
            {
                // Master
                foreach (ObjectUrl objectUrl in store.MasterRoot.List())
                    if (!traverse(objectUrl.Hash, set)) return false;

                // Spaces
                foreach (var spaceName in store.ListSpaces())
                    foreach (ObjectUrl objectUrl in store.SpaceRoot(spaceName).List())
                        if (!traverse(objectUrl.Hash, set)) return false;
            }

            // Delete those remaining
            return delete(set, graceTime);
        }

        public bool traverse(Hash hash, HashSet<Hash> set)
        {
            var hashes = getHashes(hash);
            if (hashes == null) return false;
            foreach (var child in hashes)
            {
                if (set.Contains(child)) continue;
                set.Add(child);
                if (!traverse(child, set)) return false;
            }
            return true;
        }

        public ImmutableStack<Hash> getHashes(Hash hash)
        {
            foreach (var objectStore in objectStores)
            {
                var hashes = objectStore.GetHashes(hash);
                if (hashes != null) return hashes;
            }
            return null;
        }

        public bool delete(HashSet<Hash> keep, DateTime graceTime)
        {
            var allOK = true;

            foreach (var objectStore in objectStores)
                foreach (var folder in MiniBurrow.Static.DirectoryEnumerateDirectories(objectStore.Folder))
                    foreach (var filename in MiniBurrow.Static.DirectoryEnumerateFiles(objectStore.Folder + "\\" + folder))
                    {
                        var hash = Hash.From(folder + filename);
                        if (hash == null) continue;
                        if (keep.Contains(hash)) continue;
                        var file = objectStore.Folder + "\\" + folder + "\\" + filename;
                        var fileInfo = new FileInfo(file);
                        if (fileInfo == null) continue;
                        if (fileInfo.LastWriteTimeUtc.CompareTo(graceTime) > 0) continue;
                        allOK &= MiniBurrow.Static.FileDelete(file);
                    }

            return allOK;
        }
    }
}
