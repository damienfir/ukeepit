using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SafeBox.Burrow.Serialization
{
    public class Dictionary
    {
        // *** Static ***

        public struct Pair
        {
            public ArraySegment<byte> Key;
            public ArraySegment<byte> Value;
        }

        public static Dictionary From(BurrowObject obj) { return From(obj, obj.Data); }

        public static Dictionary From(BurrowObject obj, ArraySegment<byte> bytes)
        {
            var pairs = new ImmutableStack<Pair>();
            if (bytes.Array == null) return new Dictionary(obj, pairs);

            var pos = bytes.Offset;
            while (pos < bytes.Offset + bytes.Count)
            {
                // Key and value length
                var keyLength = BitConverter.ToInt16(bytes.Array, pos);
                if (pos + 2 + keyLength + 2 > bytes.Offset + bytes.Count) return new Dictionary(obj, pairs);
                var valueLength = BitConverter.ToInt16(bytes.Array, pos + 2 + keyLength);
                if (pos + 2 + keyLength + 2 + valueLength > bytes.Offset + bytes.Count) return new Dictionary(obj, pairs);

                // Key and value
                var pair = new Pair();
                pair.Key = new ArraySegment<byte>(bytes.Array, pos+2, keyLength);
                pair.Value = new ArraySegment<byte>(bytes.Array, pos + 2 + keyLength + 2, valueLength);
                pairs = pairs.With(pair);
                pos += pos + 2 + keyLength + 2 + valueLength;
            }

            return new Dictionary(obj, pairs);
        }

        // *** Object ***

        public readonly BurrowObject Object;
        public readonly Dictionary<string, ArraySegment<byte>> PairsByStringKey = new Dictionary<string, ArraySegment<byte>>();
        public readonly ImmutableStack<Pair> Pairs;

        public Dictionary(BurrowObject obj, ImmutableStack<Pair> pairs) {
            Object = obj;
            Pairs = pairs;
            foreach (var pair in pairs)
            {
                var key = System.Text.Encoding.UTF8.GetString(pair.Key.Array, pair.Key.Offset, pair.Key.Count);
                PairsByStringKey[key] = pair.Value;
            }
        }

        public ArraySegment<byte> Get(byte[] key) { return Get(new ArraySegment<byte>(key)); }

        // This is not very efficient, but supposed to be a rare operation, and on small dictionaries
        public ArraySegment<byte> Get(ArraySegment<byte> key) {
            foreach (var pair in Pairs)
                if (Static.IsEqual(key, pair.Key)) return pair.Value;
            return new ArraySegment<byte>();
        }

        public ArraySegment<byte> Get(string key)
        {
            ArraySegment<byte> bytes;
            PairsByStringKey.TryGetValue(key, out bytes);
            return bytes;
        }

        public ArraySegment<byte> Get(string key, ArraySegment<byte> defaultValue)
        {
            ArraySegment<byte> bytes;
            if (!PairsByStringKey.TryGetValue(key, out bytes)) return defaultValue;
            return bytes;
        }

        public string Get(string key, string defaultValue)
        {
            ArraySegment<byte> bytes;
            if (!PairsByStringKey.TryGetValue(key, out bytes)) return defaultValue;
            return System.Text.Encoding.UTF8.GetString(bytes.Array, bytes.Offset, bytes.Count);
        }

        public bool Get(string key, bool defaultValue)
        {
            ArraySegment<byte> bytes;
            if (!PairsByStringKey.TryGetValue(key, out bytes)) return defaultValue;
            return bytes.Count == 1 && bytes.Array[bytes.Offset] != 0;
        }

        public ulong Get(string key, ulong defaultValue)
        {
            ArraySegment<byte> bytes;
            if (!PairsByStringKey.TryGetValue(key, out bytes)) return defaultValue;
            return ReadUnsignedLong(bytes);
        }

        public static ulong ReadUnsignedLong(ArraySegment<byte> bytes)
        {
            return
                bytes.Count == 1 ? bytes.Array[bytes.Offset] :
                bytes.Count == 2 ? BigEndian.UInt16(bytes.Array, bytes.Offset) :
                bytes.Count == 4 ? BigEndian.UInt32(bytes.Array, bytes.Offset) :
                bytes.Count == 8 ? BigEndian.UInt64(bytes.Array, bytes.Offset) : 0;
        }

        public long Get(string key, long defaultValue)
        {
            ArraySegment<byte> bytes;
            if (!PairsByStringKey.TryGetValue(key, out bytes)) return defaultValue;
            return ReadSignedLong(bytes);
        }

        public static long ReadSignedLong(ArraySegment<byte> bytes)
        {
            return
                bytes.Count == 1 ? bytes.Array[bytes.Offset] :
                bytes.Count == 2 ? BigEndian.Int16(bytes.Array, bytes.Offset) :
                bytes.Count == 4 ? BigEndian.Int32(bytes.Array, bytes.Offset) :
                bytes.Count == 8 ? BigEndian.Int64(bytes.Array, bytes.Offset) : 0;
        }

        public DateTime Get(string key, DateTime defaultValue)
        {
            ArraySegment<byte> bytes;
            if (!PairsByStringKey.TryGetValue(key, out bytes)) return defaultValue;
            return Static.Timestamp(ReadSignedLong(bytes));
        }

        public Hash GetHash(string key)
        {
            ArraySegment<byte> bytes;
            if (!PairsByStringKey.TryGetValue(key, out bytes)) return null;
            if (bytes.Count == 1) return Object.Hashes(bytes.Array[bytes.Offset]);
            if (bytes.Count == 2) return Object.Hashes(BigEndian.UInt16(bytes.Array, bytes.Offset));
            if (bytes.Count == 4) return Object.Hashes(BigEndian.Int32(bytes.Array, bytes.Offset));
            if (bytes.Count == 32) return Hash.From(bytes.Array, bytes.Offset);
            return null;
            
        }

    }
}
