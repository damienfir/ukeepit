using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uKeepIt.MiniBurrow.Serialization
{
    public class DictionaryConstructor
    {
        //public readonly Dictionary<string, Hash> HashPairs = new Dictionary<string, Hash>();
        public readonly ObjectHeader ObjectHeader = new ObjectHeader();
        public readonly ByteWriter ByteWriter = new ByteWriter();
        private BigEndian bigEndian = new BigEndian();

        public DictionaryConstructor()
        {
            ByteWriter.Append(Dictionary.MimeType);
        }

        public BurrowObject ToBurrowObject() { return BurrowObject.For(ObjectHeader, ByteWriter); }

        private void Append(string text) { Append(System.Text.Encoding.UTF8.GetBytes(text)); }

        private void Append(byte[] bytes)
        {
            ByteWriter.Append(bigEndian.UInt16((ushort)bytes.Length));
            ByteWriter.Append(bytes);
        }

        private void Append(ArraySegment<byte> bytes)
        {
            ByteWriter.Append(bigEndian.UInt16((ushort)bytes.Count));
            ByteWriter.Append(bytes);
        }

        private void Append(ByteWriter bytes)
        {
            ByteWriter.Append(bigEndian.UInt16((ushort)bytes.ByteLength()));
            ByteWriter.Append(bytes);
        }

        public void Add(ArraySegment<byte> key, ByteWriter value) { Append(key); Append(value); }
        public void Add(ArraySegment<byte> key, ArraySegment<byte> value) { Append(key); Append(value); }
        public void Add(ArraySegment<byte> key, byte[] value) { Append(key); Append(value); }

        public void Add(byte[] key, ByteWriter value) { Append(key); Append(value); }
        public void Add(byte[] key, ArraySegment<byte> value) { Append(key); Append(value); }
        public void Add(byte[] key, byte[] value) { Append(key); Append(value); }

        public void Add(string key, ByteWriter value) { Append(key); Append(value); }
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
        public void Add(string key, Hash value) { Append(key); Append(bigEndian.UInt(ObjectHeader.Add(value))); }
    }
}
