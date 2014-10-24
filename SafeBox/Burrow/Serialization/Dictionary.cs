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

        private static DictionaryPair EmptyPair = new DictionaryPair(Burrow.Static.EmptyByteSegment, null, null);

        internal static byte[] MimeType = { 29, 97, 112, 112, 108, 105, 99, 97, 116, 105, 111, 110, 47, 98, 117, 114, 114, 111, 119, 45, 100, 105, 99, 116, 105, 111, 110, 97, 114, 121 };

        public static Dictionary From(BurrowObject obj)
        {
            if (obj == null) return null;

            // Verify the mime type
            if (obj.Data.Count < 30) return null;
            for (var i = 0; i < 30; i++) if (obj.Data.Array[i + obj.Data.Offset] != MimeType[i]) return null;

            // Read the dictionary
            return From(obj, new ArraySegment<byte>(obj.Data.Array, obj.Data.Offset + 30, obj.Data.Count - 30));
        }

        public static Dictionary From(BurrowObject obj, ArraySegment<byte> bytes)
        {
            var pairs = new ImmutableStack<DictionaryPair>();
            if (bytes.Array == null) return new Dictionary(pairs);

            var pos = bytes.Offset;
            while (pos < bytes.Offset + bytes.Count)
            {
                // Key and value length
                var keyLength = BigEndian.Int16(bytes.Array, pos);
                if (pos + 2 + keyLength + 2 > bytes.Offset + bytes.Count) return new Dictionary(pairs);
                var valueLength = BigEndian.Int16(bytes.Array, pos + 2 + keyLength);
                if (pos + 2 + keyLength + 2 + valueLength > bytes.Offset + bytes.Count) return new Dictionary(pairs);

                // Key and value
                var pair = new DictionaryPair(new ArraySegment<byte>(bytes.Array, pos + 2, keyLength), new ArraySegment<byte>(bytes.Array, pos + 2 + keyLength + 2, valueLength), obj);
                pairs = pairs.With(pair);
                pos += pos + 2 + keyLength + 2 + valueLength;
            }

            return new Dictionary(pairs);
        }

        // *** Object ***

        public readonly Dictionary<string, DictionaryPair> PairsByStringKey = new Dictionary<string, DictionaryPair>();
        public readonly ImmutableStack<DictionaryPair> Pairs;

        public Dictionary(ImmutableStack<DictionaryPair> pairs)
        {
            Pairs = pairs;
            foreach (var pair in pairs)
                PairsByStringKey[pair.KeyAsText] = pair;
        }

        public DictionaryPair Get(byte[] key) { return Get(new ArraySegment<byte>(key)); }

        // This is not very efficient, but supposed to be a rare operation, and on small dictionaries
        public DictionaryPair Get(ArraySegment<byte> key)
        {
            foreach (var pair in Pairs)
                if (Static.IsEqual(key, pair.Key)) return pair;
            return EmptyPair;
        }

        public DictionaryPair Get(string key)
        {
            DictionaryPair pair;
            PairsByStringKey.TryGetValue(key, out pair);
            return pair;
        }
    }

    public class DictionaryPair
    {
        public readonly ArraySegment<byte> Key;
        public readonly ArraySegment<byte> Value;
        public readonly BurrowObject BurrowObject;
        public readonly string KeyAsText;

        public DictionaryPair(ArraySegment<byte> key, ArraySegment<byte> value, BurrowObject burrowObject)
        {
            this.Key = key;
            this.Value = value;
            this.BurrowObject = burrowObject;
            this.KeyAsText = System.Text.Encoding.UTF8.GetString(key.Array, key.Offset, key.Count);
        }

        public ArraySegment<byte> AsBytes() { return Value.Array == null ? Burrow.Static.NullByteSegment: Value; }
        public ArraySegment<byte> AsBytes(ArraySegment<byte> defaultValue) { return Value.Array == null ? defaultValue : Value; }
        public ByteReader AsByteReader() { return Value.Array == null ? ByteReader.Empty() : ByteReader.From(Value); }
        public string AsText(string defaultValue = null) { return Value.Array == null ? defaultValue : System.Text.Encoding.UTF8.GetString(Value.Array, Value.Offset, Value.Count); }
        public bool AsBoolean(bool defaultValue = false) { return Value.Array == null || Value.Count > 1 ? defaultValue : Value.Count == 1 && Value.Array[Value.Offset] != 0; }

        public ulong AsUlong(ulong defaultValue = 0L)
        {
            if (Value.Array == null) return defaultValue;
            if (Value.Count == 0) return 0;
            if (Value.Count == 1) return Value.Array[Value.Offset];
            if (Value.Count == 2) return BigEndian.UInt16(Value.Array, Value.Offset);
            if (Value.Count == 4) return BigEndian.UInt32(Value.Array, Value.Offset);
            if (Value.Count == 8) return BigEndian.UInt64(Value.Array, Value.Offset);
            return defaultValue;
        }

        public long AsLong(long defaultValue = 0L)
        {
            if (Value.Array == null) return defaultValue;
            if (Value.Count == 0) return 0;
            if (Value.Count == 1) return Value.Array[Value.Offset];
            if (Value.Count == 2) return BigEndian.Int16(Value.Array, Value.Offset);
            if (Value.Count == 4) return BigEndian.Int32(Value.Array, Value.Offset);
            if (Value.Count == 8) return BigEndian.Int64(Value.Array, Value.Offset);
            return defaultValue;
        }

        public DateTime AsDateTime(DateTime defaultValue)
        {
            if (Value.Array == null) return defaultValue;
            if (Value.Count == 0) return Static.Timestamp(0);
            if (Value.Count == 1) return Static.Timestamp(Value.Array[Value.Offset]);
            if (Value.Count == 2) return Static.Timestamp(BigEndian.Int16(Value.Array, Value.Offset));
            if (Value.Count == 4) return Static.Timestamp(BigEndian.Int32(Value.Array, Value.Offset));
            if (Value.Count == 8) return Static.Timestamp(BigEndian.Int64(Value.Array, Value.Offset));
            return defaultValue;
        }

        public Hash AsHash()
        {
            if (Value.Array == null) return null;
            if (Value.Count == 0) return BurrowObject.HashAtIndex(0);
            if (Value.Count == 1) return BurrowObject.HashAtIndex(Value.Array[Value.Offset]);
            if (Value.Count == 2) return BurrowObject.HashAtIndex(BigEndian.UInt16(Value.Array, Value.Offset));
            if (Value.Count == 4) return BurrowObject.HashAtIndex(BigEndian.Int32(Value.Array, Value.Offset));
            if (Value.Count == 32) return Hash.From(Value.Array, Value.Offset);
            return null;
        }

        public Hash HashAtIndex(int index) { return BurrowObject == null ? null : BurrowObject.HashAtIndex(index); }
    }
}
