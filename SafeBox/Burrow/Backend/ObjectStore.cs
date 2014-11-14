using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Configuration;

namespace SafeBox.Burrow.Backend
{
    // Methods are called from the main thread, and must not block, but call the handler (on the main thread) when done.
    // More than one method may be called at the same time.
    public abstract class ObjectStore : IComparable<ObjectStore>
    {
        // Two stores with the same Url are supposed to be the same.
        public readonly string Url;

        // A lower value means that the store can be accessed more easily, or faster, and should therefore be preferred when getting objects.
        public readonly int Priority;

        // Returns true if the object exists.
        public abstract bool Has(Hash hash);

        // Returns the object, or null if it does not exist.
        public abstract Serialization.BurrowObject Get(Hash hash);

        // Stores the object and returns its hash. Returns null if the object could not be stored.
        public abstract Hash Put(Serialization.BurrowObject serializedObject, PrivateIdentity identity);

        // Asynchronous interface
        public TaskGroup.Future<bool> Has(Hash hash, TaskGroup whenDone) { return whenDone.Run(() => Has(hash)); }
        public TaskGroup.Future<Serialization.BurrowObject> Get(Hash hash, TaskGroup whenDone) { return whenDone.Run(() => Get(hash)); }
        public TaskGroup.Future<Hash> Put(Serialization.BurrowObject serializedObject, PrivateIdentity identity, TaskGroup whenDone) { return whenDone.Run(() => Put(serializedObject, identity)); }

        public ObjectStore(string url, int priority)
        {
            this.Url = url;
            this.Priority = priority;
        }

        public int CompareTo(ObjectStore other)
        {
            return other.Priority - this.Priority;
        }
    }
}
