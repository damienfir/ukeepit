using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.Serialization
{
    public class BurrowObject
    {
        // *** Static ***

        public static BurrowObject From(byte[] bytes) { return bytes == null ? null : new BurrowObject(new ArraySegment<byte>(bytes)); }
        public static BurrowObject From(ArraySegment<byte> bytes) { return bytes.Array == null ? null : new BurrowObject(bytes); }
        public static BurrowObject For(ObjectHeader hashCollector, byte[] data) { return For(hashCollector, new ArraySegment<byte>(data)); }

        public static BurrowObject For(ObjectHeader hashCollector, ArraySegment<byte> data)
        {
            // Allocate memory
            var headerLength = hashCollector.ByteLength();
            var bytes = new byte[headerLength + data.Count];

            // Serialize
            hashCollector.WriteToByteArray(bytes, 0);
            Array.Copy(data.Array, data.Offset, bytes, headerLength, data.Count);
            return new BurrowObject(new ArraySegment<byte>(bytes));
        }

        public static BurrowObject For(ObjectHeader hashCollector, ByteWriter data)
        {
            // Allocate memory
            var headerLength = hashCollector.ByteLength();
            var bytes = new byte[headerLength + data.ByteLength()];

            // Serialize
            hashCollector.WriteToByteArray(bytes, 0);
            data.WriteToByteArray(bytes, headerLength);
            return new BurrowObject(new ArraySegment<byte>(bytes));
        }

        // *** Object ***

        public readonly ArraySegment<byte> Bytes;
        public readonly ArraySegment<byte> Data;
        Hash CachedHash;
        int HashesCount;

        public BurrowObject(ArraySegment<byte> bytes)
        {
            this.Bytes = bytes;
            HashesCount = (bytes.Array[bytes.Offset + 0] << 24) | (bytes.Array[bytes.Offset + 1] << 16) | (bytes.Array[bytes.Offset + 2] << 8) | bytes.Array[bytes.Offset + 3];
            var dataStart = HashesCount * 32 + 4;
            if (dataStart < bytes.Count) Data = new ArraySegment<byte>(bytes.Array, dataStart, bytes.Count - dataStart);
        }

        public bool IsValid() { return Data.Array != null; }

        public Hash HashAtIndex(int index)
        {
            if (index < 0 || index >= HashesCount) return null;
            return Burrow.Hash.From(Bytes.Array, Bytes.Offset + index * 32 + 4);
        }

        public Hash[] Hashes()
        {
            var hashes = new Hash[HashesCount];
            for (var i = 0; i < HashesCount; i++) hashes[i] = Burrow.Hash.From(Bytes.Array, Bytes.Offset + i * 32 + 4);
            return hashes;
        }

        public string Utf8Data()
        {
            try { return Encoding.UTF8.GetString(Data.Array, Data.Offset, Data.Count); }
            catch (Exception) { return null; }
        }

        public Hash Hash()
        {
            if (CachedHash != null) return CachedHash;
            var sha256 = new System.Security.Cryptography.SHA256Managed();
            var hashBytes = sha256.ComputeHash(Bytes.Array, Bytes.Offset, Bytes.Count);
            CachedHash = Burrow.Hash.From(hashBytes);
            return CachedHash;
        }
    }
}
