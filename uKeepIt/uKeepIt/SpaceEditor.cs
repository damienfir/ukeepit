using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using uKeepIt.MiniBurrow.Serialization;
using uKeepIt.MiniBurrow;
using uKeepIt.MiniBurrow.Folder;
using uKeepIt.MiniBurrow.Aes;

namespace uKeepIt
{
    public class SpaceEditor
    {
        public readonly ConfigurationSnapshot Configuration;
        public readonly ImmutableStack<Root> Roots;
        public readonly ArraySegment<byte> Key;
        public readonly Dictionary<Hash, FileEntry> FilesByContentId = new Dictionary<Hash, FileEntry>();
        public readonly Dictionary<string, ImmutableStack<FileEntry>> FilesByPath = new Dictionary<string, ImmutableStack<FileEntry>>();
        public readonly Dictionary<string, FolderEntry> FoldersByPath = new Dictionary<string, FolderEntry>();
        public readonly Dictionary<string, Hash> MergedHashesByHexId = new Dictionary<string, Hash>();
        public readonly SHA256Managed sha256 = new SHA256Managed();

        private string Title="";
        private long CreationDate = 0L;
        private ulong Revision=0;
        private bool hasChanges = false;

        public SpaceEditor(ConfigurationSnapshot configuration, ImmutableStack<Root> roots, ArraySegment<byte> key)
        {
            this.Configuration  = configuration;
            this.Roots=roots;
            this.Key= key;

            // Fully read the space
            foreach (var root in Roots)
            foreach (var objectUrl in root.List())
            {
                if (MergedHashesByHexId.ContainsKey(objectUrl.Hash.Hex())) continue;

                var envelopeObject= Configuration.MultiObjectStore.Get(objectUrl.Hash);
                var reference = MiniBurrow.Aes.Envelope.Open(envelopeObject, Key);

                var obj = Configuration.MultiObjectStore.Get(reference.Hash);
                MiniBurrow.Aes.Static.Process(obj.Data, reference.Key, reference.Nonce);
                if (Merge(obj)) MergedHashesByHexId.Add(objectUrl.Hash.Hex(), objectUrl.Hash);
            }
        }

        internal ulong GetRevision() { return Revision; }

        internal bool Merge(BurrowObject obj)
        {
            // Open the dictionary
            var dictionary = MiniBurrow.Serialization.Dictionary.From(obj);
            if (dictionary == null) return false;

            // General info is merged using most recent semantics
            var isFirstReference = Revision == 0;
            var revision = dictionary.Get("revision").AsUlong();
            if (revision > Revision)
            {
                Revision = revision;
                Title = dictionary.Get("title").AsText("");
                CreationDate = dictionary.Get("creation date").AsLong(0);
            }

            // Files are union merged
            foreach (var pair in dictionary.Pairs)
            {
                if (pair.KeyAsText == "file")
                {
                    var entry = FileEntry.From(pair.Value, pair.BurrowObject);
                    if (entry != null) Merge(entry);
                }
                else if (pair.KeyAsText == "folder")
                {
                    var entry = FolderEntry.From(pair.Value, pair.BurrowObject);
                    if (entry != null) Merge(entry);
                }
            }

            return true;
        }

        internal FileEntry FileEntryByContentId(Hash contentId)
        {
            var entry = null as FileEntry;
            FilesByContentId.TryGetValue(contentId, out entry);
            return entry;
        }

        internal ImmutableStack<FileEntry> FileEntriesByPath(string path)
        {
            var entry = new ImmutableStack<FileEntry>();
            FilesByPath.TryGetValue(path, out entry);
            return entry;
        }

        internal FileEntry FileEntryById(string path, Hash contentId)
        {
            var list = FileEntriesByPath(path);
            if (list == null) return null;
            foreach (var entry in list)
                if (contentId.Equals( entry.ContentId)) return entry;
            return null;
        }

        // For efficiency, we assume that a file with the same name, lastWriteTime and length has the same contentId
        internal Hash KnownContentId(string path, ulong length, long lastWriteTime)
        {
            var list = FileEntriesByPath(path);
            if (list == null) return null;
            foreach (var entry in list)
                if (entry.Length == length && entry.LastWriteTime == lastWriteTime) return entry.ContentId;
            return null;
        }

        internal FolderEntry FolderEntryByPath(string path)
        {
            var entry = null as FolderEntry;
            FoldersByPath.TryGetValue(path, out entry);
            return entry;
        }

        public void Merge(FileEntry entry)
        {
            // Check if we have this entry already
            var existingEntry = FileEntryById(entry.Path, entry.ContentId);
            if (existingEntry != null && existingEntry.Revision >= entry.Revision) return;

            // Add (or replace)
            //FilesByContentId.Add(entry.ContentId, entry);
            // --> cannot do that, because two files with the same content but different names have to stay separate

            var list = null as ImmutableStack<FileEntry>;
            FilesByPath.TryGetValue(entry.Path, out list);
            if (list!=null) {
                if (existingEntry!=null) list=list.ReversedWithout(existingEntry);
                list.With(entry);
            }
            else
            {
                list = new ImmutableStack<FileEntry>(entry);
                FilesByPath.Add(entry.Path, list);
            }
        }

        public void Merge(FolderEntry entry)
        {
            // Check if we have this entry already
            var existingEntry = FolderEntryByPath(entry.Path);
            if (existingEntry != null && existingEntry.Revision >= entry.Revision) return;

            // Add (or replace)
            FoldersByPath.Add(entry.Path, entry);
            hasChanges = true;
        }

        public bool SaveIfChanged()
        {
            if (!hasChanges) return true;

            // Re-serialize
            var dictionary = new DictionaryConstructor();
            dictionary.Add("title", Title);
            dictionary.Add("creation date", CreationDate);
            dictionary.Add("revision", Revision);
            foreach (var list in FilesByPath)
                foreach (var entry in list.Value)
                    dictionary.Add("file", entry.Serialize(dictionary.ObjectHeader));
            foreach (var entry in FoldersByPath)
                dictionary.Add("folder", entry.Value.Serialize(dictionary.ObjectHeader));

            // Submit the dictionary
            var encryptedObject = MiniBurrow.Aes.EncryptedObject.For(dictionary.ObjectHeader, dictionary.ByteWriter);
            var hash = Configuration.MultiObjectStore.Put(encryptedObject.Object);
            if (hash == null) return false;

            // Submit the envelope
            var envelope = MiniBurrow.Aes.Envelope.Create(new ObjectReference(hash, encryptedObject.Key, encryptedObject.Nonce), Key);
            var envelopeHash = Configuration.MultiObjectStore.Put(envelope);
            if (envelopeHash==null) return false;

            // Update all roots
            var count = 0;
            foreach (var root in Roots)
                if (root.Modify(ImmutableStack.With(new ObjectUrl(envelopeHash)), MergedHashesByHexId.Values)) count++;
            if (count < 1) return false;

            hasChanges = false;
            return true;
        }

        public void UpdateRevision(ulong new_revision)
        {
            Revision = new_revision;
        }
    }
}
