using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uKeepIt.MiniBurrow.Serialization;

namespace uKeepIt.MiniBurrow.Aes
{
    public class EncryptedObject
    {
        // *** Static ***

        public static EncryptedObject For(ObjectHeader objectHeader, ByteWriter byteChain)
        {
            return For(BurrowObject.For(objectHeader, byteChain));
        }

        public static EncryptedObject For(byte[] bytes) { return For(BurrowObject.For(new ObjectHeader(), bytes)); }

        public static EncryptedObject For(ArraySegment<byte> bytes) { return For(BurrowObject.For(new ObjectHeader(), bytes)); }

        // Note: for efficiency, the object is not copied, but encrypted in-place
        private static EncryptedObject For(BurrowObject burrowObject)
        {
            var key = Static.SecureRandomBytes(32);
            var nonce = Static.SecureRandomBytes(16);
            Static.Process(burrowObject.Data, key, nonce);
            return new EncryptedObject(burrowObject, key, nonce);
        }

        // *** Object ***

        public readonly Serialization.BurrowObject Object;
        public readonly ArraySegment<byte> Key;
        public readonly ArraySegment<byte> Nonce;

        public EncryptedObject(Serialization.BurrowObject obj, ArraySegment<byte> aesKey, ArraySegment<byte> aesNonce)
        {
            this.Object = obj;
            this.Key = aesKey;
            this.Nonce = aesNonce;
        }
    }
}
