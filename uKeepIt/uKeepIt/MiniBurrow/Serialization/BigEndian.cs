using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uKeepIt.MiniBurrow.Serialization
{
    public class BigEndian
    {
        // *** Object ***
        // For efficiency reasons, we do not offer creating small byte arrays, but create larger buffers and serve chunks of it.

        // Append values
        byte[] byteArray;
        int bytesUsed = 0;

        public BigEndian() { byteArray = new byte[1024]; }
        public BigEndian(int length) { byteArray = new byte[length]; }

        private void allocateBytes(int length)
        {
            // Allocate a new byte array if necessary
            if (bytesUsed + length >= byteArray.Length) return;
            byteArray = new byte[1024];
            bytesUsed = 0;
        }

        private ArraySegment<byte> useBytes(int length)
        {
            var segment = new ArraySegment<byte>(byteArray, bytesUsed, length);
            bytesUsed += length;
            return segment;
        }

        public ArraySegment<byte> Int(ulong value) { allocateBytes(8); return useBytes(UInt(value, byteArray, bytesUsed)); }
        public ArraySegment<byte> Int8(sbyte value) { allocateBytes(1); Int8(value, byteArray, bytesUsed); return useBytes(1); }
        public ArraySegment<byte> Int16(short value) { allocateBytes(2); Int16(value, byteArray, bytesUsed); return useBytes(2); }
        public ArraySegment<byte> Int32(int value) { allocateBytes(4); Int32(value, byteArray, bytesUsed); return useBytes(4); }
        public ArraySegment<byte> Int64(long value) { allocateBytes(8); Int64(value, byteArray, bytesUsed); return useBytes(8); }
        public ArraySegment<byte> UInt(ulong value) { allocateBytes(8); return useBytes(UInt(value, byteArray, bytesUsed)); }
        public ArraySegment<byte> UInt8(byte value) { allocateBytes(1); UInt8(value, byteArray, bytesUsed); return useBytes(1); }
        public ArraySegment<byte> UInt16(ushort value) { allocateBytes(2); UInt16(value, byteArray, bytesUsed); return useBytes(2); }
        public ArraySegment<byte> UInt32(uint value) { allocateBytes(4); UInt32(value, byteArray, bytesUsed); return useBytes(4); }
        public ArraySegment<byte> UInt64(ulong value) { allocateBytes(8); UInt64(value, byteArray, bytesUsed); return useBytes(8); }

        //*** Static ***
        public static void Int8(sbyte value, byte[] array, int offset)
        {
            array[offset] = (byte)value;
        }

        public static void Int16(short value, byte[] array, int offset)
        {
            array[offset + 0] = (byte)(value >> 8);
            array[offset + 1] = (byte)value;
        }

        public static void Int32(int value, byte[] array, int offset)
        {
            array[offset + 0] = (byte)(value >> 24);
            array[offset + 1] = (byte)(value >> 16);
            array[offset + 2] = (byte)(value >> 8);
            array[offset + 3] = (byte)value;
        }

        public static void Int64(long value, byte[] array, int offset)
        {
            array[offset + 0] = (byte)(value >> 56);
            array[offset + 1] = (byte)(value >> 48);
            array[offset + 2] = (byte)(value >> 40);
            array[offset + 3] = (byte)(value >> 32);
            array[offset + 4] = (byte)(value >> 24);
            array[offset + 5] = (byte)(value >> 16);
            array[offset + 6] = (byte)(value >> 8);
            array[offset + 7] = (byte)value;
        }

        public static void UInt8(byte value, byte[] array, int offset)
        {
            array[offset] = value;
        }

        public static void UInt16(ushort value, byte[] array, int offset)
        {
            array[offset + 0] = (byte)(value >> 8);
            array[offset + 1] = (byte)value;
        }

        public static void UInt32(uint value, byte[] array, int offset)
        {
            array[offset + 0] = (byte)(value >> 24);
            array[offset + 1] = (byte)(value >> 16);
            array[offset + 2] = (byte)(value >> 8);
            array[offset + 3] = (byte)value;
        }

        public static void UInt64(ulong value, byte[] array, int offset)
        {
            array[offset + 0] = (byte)(value >> 56);
            array[offset + 1] = (byte)(value >> 48);
            array[offset + 2] = (byte)(value >> 40);
            array[offset + 3] = (byte)(value >> 32);
            array[offset + 4] = (byte)(value >> 24);
            array[offset + 5] = (byte)(value >> 16);
            array[offset + 6] = (byte)(value >> 8);
            array[offset + 7] = (byte)value;
        }

        public static int Int(long value, byte[] array, int offset)
        {
            if (value == 0) return 0;
            if (value >= -0x80 && value <= 0x7f) { Int8((sbyte)value, array, offset); return 1; }
            if (value >= -0x8000 && value <= 0x7fff) { Int16((short)value, array, offset); return 2; }
            if (value >= -0x80000000 && value <= 0x7fffffff) { Int32((int)value, array, offset); return 4; }
            Int64(value, array, offset); return 8;
        }

        public static int UInt(ulong value, byte[] array, int offset)
        {
            if (value == 0) return 0;
            if (value <= 0xff) { UInt8((byte)value, array, offset); return 1; }
            if (value <= 0xffff) { UInt16((ushort)value, array, offset); return 2; }
            if (value <= 0xffffffff) { UInt32((uint)value, array, offset); return 4; }
            UInt64(value, array, offset); return 8;
        }

        public static sbyte Int8(byte[] array, int offset) { return (sbyte)array[offset]; }
        public static short Int16(byte[] array, int offset) { return (short)((array[offset] << 8) + array[offset + 1]); }
        public static int Int32(byte[] array, int offset) { return (int)((array[offset] << 32) + (array[offset + 1] << 16) + (array[offset + 2] << 8) + array[offset + 3]); }
        public static long Int64(byte[] array, int offset) { return (long)(((ulong)array[offset] << 56) + ((ulong)array[offset + 1] << 48) + ((ulong)array[offset + 2] << 40) + ((ulong)array[offset + 3] << 32) + ((ulong)array[offset + 4] << 24) + ((ulong)array[offset + 5] << 16) + ((ulong)array[offset + 6] << 8) + array[offset + 7]); }

        public static byte UInt8(byte[] array, int offset) { return (byte)array[offset]; }
        public static ushort UInt16(byte[] array, int offset) { return (ushort)((array[offset] << 8) + array[offset + 1]); }
        public static uint UInt32(byte[] array, int offset) { return (uint)((array[offset] << 32) + (array[offset + 1] << 16) + (array[offset + 2] << 8) + array[offset + 3]); }
        public static ulong UInt64(byte[] array, int offset) { return (((ulong)array[offset] << 56) + ((ulong)array[offset + 1] << 48) + ((ulong)array[offset + 2] << 40) + ((ulong)array[offset + 3] << 32) + ((ulong)array[offset + 4] << 24) + ((ulong)array[offset + 5] << 16) + ((ulong)array[offset + 6] << 8) + array[offset + 7]); }
    }
}
