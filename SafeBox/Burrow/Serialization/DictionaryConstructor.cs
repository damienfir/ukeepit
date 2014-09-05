using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.Serialization
{
    public class DictionaryConstructor
    {
        public readonly Dictionary<string, Hash> HashPairs = new Dictionary<string, Hash>();
        private ByteChain byteChain = new ByteChain();
        private BigEndian bigEndian = new BigEndian();

        public DictionaryConstructor() {
            byteChain.Append(Dictionary.MimeType);
        }

        public ByteChain Serialize(HashCollector hashCollector)
        {
            // Add hash pairs
            foreach (var pair in HashPairs)
            {
                var index = hashCollector.Add(pair.Value);
                Append(pair.Key);
                Append(bigEndian.UInt(index));
            }

            // Return the chain, and give it up locally to avoid adding more stuff
            var byteChainToReturn = byteChain;
            byteChain = null;
            return byteChainToReturn;
        }

        public BurrowObject ToBurrowObject()
        {
            var hashCollector = new HashCollector();
            return BurrowObject.For(hashCollector, Serialize(hashCollector));
        }

        private void Append(string text) { Append(System.Text.Encoding.UTF8.GetBytes(text)); }

        private void Append(byte[] bytes)
        {
            byteChain.Append(bigEndian.UInt16((ushort)bytes.Length));
            byteChain.Append(bytes);
        }

        private void Append(ArraySegment<byte> bytes)
        {
            byteChain.Append(bigEndian.UInt16((ushort)bytes.Count));
            byteChain.Append(bytes);
        }

        private void Append(ByteChain bytes)
        {
            byteChain.Append(bigEndian.UInt16((ushort)bytes.ByteLength()));
            byteChain.Append(bytes);
        }

        public void Add(ArraySegment<byte> key, ByteChain value) { Append(key); Append(value); }
        public void Add(ArraySegment<byte> key, ArraySegment<byte> value) { Append(key); Append(value); }
        public void Add(ArraySegment<byte> key, byte[] value) { Append(key); Append(value); }

        public void Add(byte[] key, ByteChain value) { Append(key); Append(value); }
        public void Add(byte[] key, ArraySegment<byte> value) { Append(key); Append(value); }
        public void Add(byte[] key, byte[] value) { Append(key); Append(value); }

        public void Add(string key, ByteChain value) { Append(key); Append(value); }
        public void Add(string key, ArraySegment<byte> value) { Append(key); Append(value); }
        public void Add(string key, byte[] value) { Append(key); Append(value); }
        public void Add(string key, string value) { Append(key); Append(value); }
        public void Add(string key, DateTime utcDate) { Append(key); Append(bigEndian.Int64(Static.Timestamp(utcDate))); }
        public void Add(string key, bool value) { Append(key); Append(bigEndian.UInt8(value ? (byte)1 : (byte)0)); }
        public void Add(string key, byte value) { Append(key); Append(bigEndian.UInt8(value)); }
        public void Add(string key, ushort value) { Append(key); Append(bigEndian.UInt16(value)); }
        public void Add(string key, uint value) { Append(key); Append(bigEndian.UInt32(value)); }
        public void Add(string key, ulong value) { Append(key); Append(bigEndian.UInt64(value)); }
        public void Add(string key, sbyte value) { Append(key); Append(bigEndian.Int8(value)); }
        public void Add(string key, short value) { Append(key); Append(bigEndian.Int16(value)); }
        public void Add(string key, int value) { Append(key); Append(bigEndian.Int32(value)); }
        public void Add(string key, long value) { Append(key); Append(bigEndian.Int64(value)); }
        public void Add(string key, float value) { Append(key); Append(BitConverter.GetBytes(value)); }
        public void Add(string key, double value) { Append(key); Append(BitConverter.GetBytes(value)); }
        public void Add(string key, Hash value) { HashPairs[key] = value; }
    }
}
