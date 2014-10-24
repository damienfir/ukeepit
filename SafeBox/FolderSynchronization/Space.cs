using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow;

namespace SafeBox.FolderSynchronization
{
    class Space
    {
        // *** Static ***

        public static Space Create(Spaces spaces) { return new Space(spaces, Burrow.Static.RandomBytes(16)); }

        // *** Object ***

        public readonly Spaces Spaces;
        public readonly ArraySegment<byte> Id;
        public readonly Dictionary<string, FileEntry> FilesByHexId = new Dictionary<string, FileEntry>();
        public readonly Dictionary<string, FolderEntry> FoldersByHexId = new Dictionary<string, FolderEntry>();
        public readonly Dictionary<string, Hash> MergedHashesByHexId = new Dictionary<string, Hash>();
        public SpaceReference CommitReference = null;
        private ulong Revision = 0;
        private string Title = "";
        private long CreationDate = 0;
        private ArraySegment<byte> CreationAuthor;
        private ArraySegment<byte> Key;

        public Space(Spaces spaces, ArraySegment<byte> id)
        {
            this.Spaces = spaces;
            this.Id = id;
        }

        internal void Merge(SpaceReference Reference, Burrow.Serialization.Dictionary dictionary)
        {
            // If this reference has been merged already, there is no need to do anything
            var hashHex = Reference.HashWithAesParameters.Hash.Hex();
            var hash = null as Hash;
            if (MergedHashesByHexId.TryGetValue(hashHex, out hash)) return;

            // General info is merged using most recent semantics
            var isFirstReference = Revision == 0;
            var revision = dictionary.Get("revision").AsUlong();
            if (revision > Revision)
            {
                Revision = revision;
                Title = dictionary.Get("title").AsText("");
                CreationDate = dictionary.Get("creation date").AsLong(0);
                CreationAuthor = dictionary.Get("creation author").AsBytes();
                if (CreationAuthor.Array == null || CreationAuthor.Count != 32) CreationAuthor = Burrow.Static.RandomBytes(32);
                Key = dictionary.Get("key").AsBytes();
                if (Key.Array == null || Key.Count != 32) Key = Burrow.Static.RandomBytes(32);
            }

            // Files are union merged
            foreach (var pair in dictionary.Pairs)
            {
                if (pair.KeyAsText == "slice")
                {
                    // A file (id 16 | size uint64 | date int64 | name uint16+text | chunks (hash index uint32 | aes key 32 | aes iv 16))
                } else if (pair.KeyAsText == "folder")
                {
                }
            }
        }

        internal List<SpaceSlice> Slices = new List<SpaceSlice>();

        public void AddFile(FileEntry file)
        {
            // Find the right place
            var s = 0;
            while (s < Slices.Count)
            {
                if (!Slices[s].BelongsHere(file)) break;
                s++;
            }

            // Insert before this, unless we are at the begining
            if (s > 0) s--;

            // Check if we have enough space here
            var slice = Slices[s];
            if (slice.UsedFiles >= 32)
            {
                var newSlice = new SpaceSlice();
                for (var i = 0; i < 16; i++)
                    newSlice.Files[i] = slice.Files[i + 16];
                slice.UsedFiles = 16;
                newSlice.UsedFiles = 16;
                Slices.Insert(s + 1, newSlice);
                if (newSlice.BelongsHere(file)) slice = newSlice;
            }

            // Append
            slice.Files[slice.UsedFiles] = file;
            slice.UsedFiles += 1;

            // Merge consecutive slices with less than 100 entries
        }
    }

    class SpaceSlice
    {
        
        internal FileEntry[] Files= new FileEntry[32];
        internal int UsedFiles = 0;
        internal HashWithAesParameters CommitReference;
        internal bool HasChanged = false;

        public SpaceSlice()
        {

        }

        internal bool BelongsHere(FileEntry file) { return Burrow.Static.Compare(file.Title, Files[0].Title) >= 0; }
        internal bool IsFull() { return UsedFiles >= Files.Length; }
    }
}
