using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uKeepIt.MiniBurrow.Serialization
{
    public class ByteReader
    {
        // *** Static ***

        public static ByteReader Empty() { return new ByteReader(MiniBurrow.Static.EmptyByteArray, 0, 0); }
        public static ByteReader From(byte[] bytes) { return new ByteReader(bytes, 0, bytes.Length); }
        public static ByteReader From(ArraySegment<byte> bytes) { return new ByteReader(bytes.Array, bytes.Offset, bytes.Offset + bytes.Count); }
        
        // *** Object ***

        public readonly byte[] Buffer;
        private int Pos;
        private int End;

        public ByteReader(byte[] buffer, int pos, int end)
        {
            this.Buffer = buffer;
            Pos = pos;
            End = end;
        }

        public ArraySegment<byte> ReadBytes(int length) { var value = Pos + length <= End ? new ArraySegment<byte>(Buffer, Pos, length) : MiniBurrow.Static.NullByteSegment; Pos += length; return value; }
        public ArraySegment<byte> ReadRemainingBytes() { var value = Pos < End ? new ArraySegment<byte>(Buffer, Pos, End - Pos) : MiniBurrow.Static.EmptyByteSegment; Pos = End; return value; }
        public string ReadRemainingText() { var value = Pos < End ? System.Text.Encoding.UTF8.GetString(Buffer, Pos, End - Pos) : ""; Pos = End; return value; }
        public byte ReadUint8(byte defaultValue) { var value = Pos + 1 <= End ? Buffer[Pos] : defaultValue; Pos += 1; return value; }
        public ushort ReadUint16(ushort defaultValue) { var value = Pos + 2 <= End ? BigEndian.UInt16(Buffer, Pos) : defaultValue; Pos += 2; return value; }
        public uint ReadUint32(uint defaultValue) { var value = Pos + 4 <= End ? BigEndian.UInt32(Buffer, Pos) : defaultValue; Pos += 4; return value; }
        public ulong ReadUint64(ulong defaultValue) { var value = Pos + 8 <= End ? BigEndian.UInt64(Buffer, Pos) : defaultValue; Pos += 8; return value; }
        public sbyte ReadInt8(sbyte defaultValue) { var value = Pos + 1 <= End ? BigEndian.Int8(Buffer, Pos) : defaultValue; Pos += 1; return value; }
        public short ReadInt16(short defaultValue) { var value = Pos + 2 <= End ? BigEndian.Int16(Buffer, Pos) : defaultValue; Pos += 2; return value; }
        public int ReadInt32(int defaultValue) { var value = Pos + 4 <= End ? BigEndian.Int32(Buffer, Pos) : defaultValue; Pos += 4; return value; }
        public long ReadInt64(long defaultValue) { var value = Pos + 8 <= End ? BigEndian.Int64(Buffer, Pos) : defaultValue; Pos += 8; return value; }
        public bool AtEnd() { return Pos >= End; }
        public int RemainingBytes() { return End - Pos; }
        
        public ArraySegment<byte> ReadUint16Bytes(ArraySegment<byte> defaultValue)
        {
            if (Pos + 2 > End) { Pos += 2; return defaultValue; }
            var length = BigEndian.UInt16(Buffer, Pos);
            Pos += 2;
            var value = Pos + length <= End ? new ArraySegment<byte>(Buffer, Pos, length) : defaultValue;
            Pos += length;
            return value;
        }

        public ArraySegment<byte> ReadUint32Bytes(ArraySegment<byte> defaultValue)
        {
            if (Pos + 4 > End) { Pos += 4; return defaultValue; }
            var length = BigEndian.Int32(Buffer, Pos);
            if (length < 0) { Pos = End; return defaultValue; }
            Pos += 4;
            var value = Pos + length <= End ? new ArraySegment<byte>(Buffer, Pos, length) : defaultValue;
            Pos += length;
            return value;
        }

        public string ReadUint16Text(string defaultValue)
        {
            var bytes = ReadUint16Bytes(MiniBurrow.Static.NullByteSegment);
            return bytes.Array == null ? defaultValue : System.Text.Encoding.UTF8.GetString(bytes.Array, bytes.Offset, bytes.Count);
        }

        public string ReadUint32Text(string defaultValue)
        {
            var bytes = ReadUint32Bytes(MiniBurrow.Static.NullByteSegment);
            return bytes.Array == null ? defaultValue : System.Text.Encoding.UTF8.GetString(bytes.Array, bytes.Offset, bytes.Count);
        }
    }
}
