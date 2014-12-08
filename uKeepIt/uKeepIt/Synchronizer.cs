using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using uKeepIt.MiniBurrow.Serialization;
using uKeepIt.MiniBurrow.Folder;
using uKeepIt.MiniBurrow.Aes;
using uKeepIt.MiniBurrow;

namespace uKeepIt
{
    public class Synchronizer
    {
        public readonly SpaceEditor SpaceEditor;
        public readonly string CheckoutFolder;
        public readonly MultiObjectStore MultiObjectStore;
        public readonly ulong Now;

        public Synchronizer(SpaceEditor spaceEditor, string folder, MultiObjectStore multiObjectStore)
        {
            this.SpaceEditor = spaceEditor;
            this.CheckoutFolder = folder;
            this.MultiObjectStore = multiObjectStore;

            // Find a date that is at least now (in UTC), but more recent than the state
            Now = Math.Max((ulong)DateTime.UtcNow.Ticks, SpaceEditor.GetRevision() + 1);

            // Mark all files and folders as "not processed"
            foreach (var entry in SpaceEditor.FilesByContentId) entry.Value.InUse = false;
            foreach (var entry in SpaceEditor.FoldersByPath) entry.Value.InUse = false;

            // Traverse the file system to check in changes
            Traverse("");

            // Traverse the space to check out changes
            foreach (var entry in SpaceEditor.FilesByContentId) ProcessEntry(entry.Value);

            // Delete files not in use any more
            DeleteNotInUse();
        }

        private void DeleteNotInUse()
        {
            foreach (var entry in SpaceEditor.FilesByContentId)
            {
                if (entry.Value.InUse) continue;
                if (entry.Value.Deleted) continue;
                entry.Value.Deleted = true;
                entry.Value.Revision = Now;
            }

            foreach (var entry in SpaceEditor.FoldersByPath)
            {
                if (entry.Value.InUse) continue;
                if (entry.Value.Deleted) continue;
                entry.Value.Deleted = true;
                entry.Value.Revision = Now;
            }

        }

        public bool Traverse(string folder)
        {
            // Add the folder if necessary
            var folderEntry = SpaceEditor.FolderEntryByPath(folder);
            if (folderEntry == null)
                SpaceEditor.Merge(new FolderEntry(folder, Now, false));
            else
                folderEntry.InUse = true;

            // Add all files that the user created or modified
            foreach (var filename in MiniBurrow.Static.DirectoryEnumerateFiles(folder))
                ProcessFile(folder + "\\" + filename);

            // Traverse all subfolders
            foreach (var subFolder in MiniBurrow.Static.DirectoryEnumerateDirectories(folder))
            {
                if (!Traverse(folder + "/" + subFolder)) return false;
            }

            return true;
        }

        public bool ProcessFile(string path)
        {
            var file = CheckoutFolder + "\\" + path;
            var fileInfo = new FileInfo(file);

            // Get or calculate the content ID
            var contentId = SpaceEditor.KnownContentId(path, (ulong)fileInfo.Length, fileInfo.LastWriteTimeUtc.Ticks);
            if (contentId == null)
            {
                var stream = File.OpenRead(file);
                contentId = Hash.From(SpaceEditor.sha256.ComputeHash(stream));
                stream.Close();
            }

            // Check if we know about this file
            var fileEntry = SpaceEditor.FileEntryById(path, contentId);
            if (fileEntry == null)
            {
                // This is a new file: reuse the content of a known file, or upload the new content

                // Check if we know that content ID already
                var sameFileEntry = SpaceEditor.FileEntryByContentId(contentId);
                if (sameFileEntry != null)
                {
                    // An entry with the same content exists (e.g. after file rename), reuse that   
                    SpaceEditor.Merge(new FileEntry(path, contentId, Now, false, (ulong)fileInfo.Length, fileInfo.LastWriteTimeUtc.Ticks, sameFileEntry.Chunks));
                    return true;
                }

                // Upload the chunks
                var chunks = new ImmutableStack<ObjectReference>();
                var bytes = MiniBurrow.Static.FileBytes(file);
                var offset = 0;
                var targetChunkSize = Math.Min(8 * 1024 * 1024, bytes.Length / 4);
                while (offset < bytes.Length)
                {
                    // Pick a randum chunk size with the following constraints
                    // - chunk sizes between 10 KiB and 10 MiB
                    // - minimal storage overhead (ideally X * 4096 - 4)
                    var chunkSize = (int)Math.Floor(MiniBurrow.Static.Random.NextDouble() * targetChunkSize * 2) + 10 * 1024;
                    if (bytes.Length - offset - chunkSize < 10 * 1024) chunkSize += 10 * 1024;
                    chunkSize = (int)(chunkSize & 0xfffff000) + 4092;
                    if (chunkSize > bytes.Length - offset) chunkSize = bytes.Length - offset;

                    // Add this block
                    var encryptedObject = EncryptedObject.For(new ArraySegment<byte>(bytes, offset, chunkSize));
                    var hash = MultiObjectStore.Put(encryptedObject.Object);
                    if (hash == null) return false;
                    chunks = chunks.With(new ObjectReference(hash, encryptedObject.Key, encryptedObject.Nonce));
                }

                // Create the entry
                SpaceEditor.Merge(new FileEntry(path, contentId, Now, false, (ulong)fileInfo.Length, fileInfo.LastWriteTimeUtc.Ticks, chunks.Reversed()));
                return true;
            }
            else
            {
                // We know about this file

                // Delete it if necessary
                if (fileEntry.Deleted == true)
                {
                    if (fileEntry.LastWriteTime > fileInfo.LastWriteTimeUtc.Ticks)
                    {
                        File.Delete(file);
                        return true;
                    }
                    else
                    {
                        fileEntry.Deleted = false;
                        fileEntry.Revision = (ulong) fileInfo.LastWriteTimeUtc.Ticks;
                    }
                }

                // Update its lastWriteTime
                if (fileInfo.LastWriteTimeUtc.Ticks != fileEntry.LastWriteTime) File.SetLastAccessTimeUtc(file, new DateTime(fileEntry.LastWriteTime));
                return true;
            }
        }

        public bool ProcessEntry(FileEntry entry)
        {
            if (entry.Deleted)
            {
                // We would have seen this change when traversing the file system.
                // Hence, we don't need to do anything here.
                return true;
            }
            else if (entry.InUse)
            {
                // We have processed this already
                return true;
            }
            else
            {
                // This is a new file: check it out

                // Conflict resolution: keep both versions
                if (File.Exists(CheckoutFolder + "\\" + entry.Path))
                {
                    // Delete this entry
                    entry.Deleted = true;
                    entry.Revision = Now;

                    // Add a new entry with the modification date appended
                    var date = new DateTime( entry.LastWriteTime);
                    var version = date.Year.ToString("04") + "-" + date.Month.ToString("02") + "-" + date.Date.ToString("02") + " " + date.Day.ToString("02") + ", " + date.Date.ToString("02") + ", " + entry.ContentId.Hex().Substring(0, 6);
                    entry = new FileEntry(entry.Path + " (version " + version + ")", entry.ContentId, Now, false, entry.Length, entry.LastWriteTime, entry.Chunks);
                    SpaceEditor.Merge(entry);
                }

                // Write the file
                var stream = File.OpenWrite( CheckoutFolder +"\\"+ entry.Path);
                foreach (var chunk in entry.Chunks) {
                    // Get this chunk
                    var burrowObject = MultiObjectStore.Get(chunk.Hash);
                    if (burrowObject == null) return false;
                    
                    // Decrypt
                    MiniBurrow.Aes.Static.Process(burrowObject.Data, chunk.Key, chunk.Nonce);

                    // Write
                    stream.Write(burrowObject.Data.Array, burrowObject.Data.Offset, burrowObject.Data.Count);
                }
                stream.Close();

                // Set the modification date
                File.SetLastWriteTimeUtc(CheckoutFolder + "\\" + entry.Path, new DateTime( entry.LastWriteTime));
            }

            return true;
        }
    }
}
