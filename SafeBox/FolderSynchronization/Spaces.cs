using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow;
using SafeBox.Burrow.Serialization;
using SafeBox.Burrow.Backend;

namespace SafeBox.FolderSynchronization
{
    public class SpaceReference
    {
        public ArraySegment<byte> Id = new ArraySegment<byte>(null);
        public readonly HashWithAesParameters HashWithAesParameters;
        public SpaceReference(HashWithAesParameters hashWithAesParameters)
        {
            this.HashWithAesParameters = hashWithAesParameters;
        }
    }

    public class Spaces
    {
        public readonly Index Index;
        internal ImmutableStack<SpaceReference> References = new ImmutableStack<SpaceReference>();
        private Dictionary<string, Space> ByHexId = new Dictionary<string, Space>();
        
        public Spaces(Index index) {
            this.Index = index;
        }
        
        public void AddReference(SpaceReference reference) { References = References.With(reference); }

        public Space ById(ArraySegment<byte> id) {
            var hexId = Text.ToHexString(id);
            var space = null as Space;
            if (ByHexId.TryGetValue(hexId, out space)) return space;
            space = new Space(this, id);
            ByHexId[hexId] = space;
            return space;
        }

        public void Reload() { new LoadSpaces(this); }
    }

    internal class LoadSpaces
    {
        public readonly Spaces Spaces;
        int Expected = 1;

        internal LoadSpaces(Spaces spaces)
        {
            this.Spaces = spaces;
            while (spaces.References.Length > 0)
            {
                new ProcessSpace(this, spaces.References.Head);
                spaces.References = spaces.References.Tail;
            }
        }

        internal void Done()
        {
            Expected -= 1;
            if (Expected > 0) return;
            //Spaces.Index.Loaded(); // TODO, notify
        }
    }

    internal class ProcessSpace
    {
        LoadSpaces LoadSpaces;
        SpaceReference Reference;

        internal ProcessSpace(LoadSpaces loadSpaces, SpaceReference reference)
        {
            this.LoadSpaces = loadSpaces;
            this.Reference = reference;
            new Burrow.Operations.GetFromAnyStore(reference.HashWithAesParameters.Hash, spaces.Index.BurrowSnapshot.ObjectStores, Process);
        }

        void Process(BurrowObject obj, ObjectStore source)
        {
            if (obj == null) return;
            var dictionary = Dictionary.From(obj);

            // Get the corresponding space
            // Note: this is the ID that counts. The ID sometimes passed along with the reference only has information value, so that loading of a single space is possible in the future. This is not relevant for security: writing a wrong informational ID would cause that reference to be ignored (in the worst case), which is not in the interest of an attacker.
            Reference.Id = dictionary.Get("id").AsBytes(Reference.Id);
            var space = LoadSpaces.Spaces.ById(Reference.Id);
            space.Merge(Reference, dictionary);
            LoadSpaces.Done();
        }
    }
}
