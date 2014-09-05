using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Configuration;

namespace SafeBox.Burrow.Backend
{
    // Methods are called from the main thread, and must not block, but call the handler (on the main thread) when done.
    // More than one method may be called at the same time.
    public abstract class ObjectStore
    {
        // Two stores with the same Url are supposed to be the same.
        public readonly string Url;
        // A lower value means that the store can be accessed more easily, or faster, and should therefore be preferred when getting objects.
        public readonly int Priority;

        // Returns true if the object exists.
        public delegate void HasObjectResult(bool result);
        public abstract void HasObject(Hash hash, HasObjectResult handler);

        // Returns the object, or null if it does not exist.
        public delegate void GetObjectResult(Serialization.BurrowObject result);
        public abstract void GetObject(Hash hash, GetObjectResult handler);

        // Stores the object and returns its hash. Returns null if the object could not be stored.
        public delegate void PutObjectResult(Hash result);
        public abstract void PutObject(Serialization.BurrowObject serializedObject, PrivateIdentity identity, PutObjectResult handler);

        public ObjectStore(string url, int priority)
        {
            this.Url = url;
            this.Priority = priority;
        }
    }
}
