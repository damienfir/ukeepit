using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uKeepIt.MiniBurrow.Aes
{
    public class ObjectReference
    {
        public readonly Hash Hash;
        public readonly ArraySegment<byte> Key;
        public readonly ArraySegment<byte> Nonce;

        public ObjectReference(Hash hash, ArraySegment<byte> aesKey, ArraySegment<byte> aesNonce)
        {
            this.Hash = hash;
            this.Key = aesKey;
            this.Nonce = aesNonce;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as ObjectReference);
        }

        public bool Equals(ObjectReference that)
        {
            if (that == null) return false;
            if (ReferenceEquals(this, that)) return true;
            return Hash.Equals(that.Hash) && MiniBurrow.Static.IsEqual(Key, that.Key) && MiniBurrow.Static.IsEqual(Nonce, that.Nonce);
        }

        // We do not expect the same hash with distinct AES keys.
        public override int GetHashCode() { return Hash.GetHashCode(); }
    }
}
