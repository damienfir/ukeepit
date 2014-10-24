using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow;

namespace SafeBox.FolderSynchronization
{
    public class Index
    {
        public readonly Burrow.Configuration.PrivateIdentity Identity;
        public readonly Burrow.Configuration.Snapshot BurrowSnapshot;
        public readonly Spaces Spaces;
        internal ImmutableStack<Hash> MergedHashes = new ImmutableStack<Hash>();
        public long Revision = 0;
        public string FirstName = "";
        public string LastName = "";
        public string Email = "";
        public string Phone = "";

        public Index(Burrow.Configuration.Snapshot snapshot, Burrow.Configuration.PrivateIdentity identity)
        {
            this.Identity = identity;
            this.BurrowSnapshot= snapshot;
            this.Spaces = new Spaces(this);
            new LoadIndex(this);
        }
    }

    internal class LoadIndex {
        internal Index Index;
        private Burrow.Serialization.Dictionary MostRecentDictionary= null;
        private long MostRecentRevision = 0;

        internal LoadIndex(Index index)
        {
            this.Index = index;
            this.MostRecentRevision = index.Revision;
            new Burrow.Operations.ReadRoot(index.Identity.PublicKey.Hash, "data", index.BurrowSnapshot.AccountStores, index.BurrowSnapshot.ObjectStores, index.Identity, Merge, Done);
        }

        private bool Merge(Burrow.Serialization.Dictionary dictionary)
        {
            // Check if this dictionary is newer
            var revision = dictionary.Get("revision").AsLong();
            if (MostRecentRevision < revision) { MostRecentRevision = revision; MostRecentDictionary = dictionary; }

            // Merge all spaces and addresses
            foreach (var pair in dictionary.Pairs)
            {
                if (pair.KeyAsText == "space")
                {
                    var reader = pair.AsByteReader();
                    var hash= pair.HashAtIndex( reader.ReadUint16(-1));
                    var aesKey = reader.ReadBytes(32);
                    var aesIv= reader.ReadBytes(16);
                    if (hash== null || aesKey == null || aesIv == null) continue;
                    Index.Spaces.AddReference(new SpaceReference(new HashWithAesParameters(hash, aesKey, aesIv)));
                }
                else if (pair.KeyAsText == "contact")
                {
                    // TODO if necessary
                }
            }

            return true;
        }

        private void Done(ImmutableStack<Hash> mergedHashes)
        {
            Index.MergedHashes = Index.MergedHashes.With(mergedHashes);
            if (MostRecentDictionary != null)
            {
                Index.Revision = MostRecentRevision;
                Index.FirstName = MostRecentDictionary.Get("first name").AsText("");
                Index.LastName= MostRecentDictionary.Get("last name").AsText("");
                Index.Email = MostRecentDictionary.Get("email").AsText("");
                Index.Phone = MostRecentDictionary.Get("phone").AsText("");
            }
            // TODO notify about reload
        }
    }
}
