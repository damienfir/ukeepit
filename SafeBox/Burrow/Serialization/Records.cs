using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.Serialization
{
    public class Records : System.Collections.Generic.IEnumerable<ArraySegment<byte>>
    {
        public static Records From(ArraySegment<byte> bytes)
        {
            // Read the record count
            if (bytes.Count < 4) return null;
            int count = BigEndian.Int32(bytes.Array, bytes.Offset);
            if (bytes.Count < 4 + count * 4) return null;

            // Read the offsets
            var offsets = new int[count + 1];
            offsets[0] = 0;
            for (var i = 1; i <= count; i++) offsets[i] = BigEndian.Int32(bytes.Array, bytes.Offset + i * 4);

            // Verify the length (we silently accept trailing data)
            var dataStart = 4 + count * 4;
            var dataLength = offsets[count];
            if (dataStart + dataLength < bytes.Count) return null;

            return new Records(offsets, new ArraySegment<byte>(bytes.Array, bytes.Offset + dataStart, dataLength));
        }

        private int[] offsets;
        public readonly ArraySegment<byte> Bytes;
        public Records(int[] offsets, ArraySegment<byte> bytes)
        {
            this.offsets = offsets;
            this.Bytes = bytes;
        }

        public int Count() { return offsets.Length - 1; }

        public ArraySegment<byte> Record(int index)
        {
            if (index < 0 || index >= offsets.Length) return new ArraySegment<byte>(null, 0, 0);
            return new ArraySegment<byte>(Bytes.Array, Bytes.Offset + offsets[index], offsets[index + 1] - offsets[index]);
        }

        System.Collections.Generic.IEnumerator<ArraySegment<byte>> System.Collections.IEnumerable.GetEnumerator()
        {
            return new RecordsEnumerator(this);
        }
    }

    public class RecordsEnumerator : System.Collections.Generic.IEnumerator<ArraySegment<byte>>
    {
        private int position = -1;
        private Records records;

        public RecordsEnumerator(Records records)
        {
            this.records = records;
        }

        ArraySegment<byte> Current
        {
            get { return records.Record(position); }
        }

        bool MoveNext()
        {
            if (position >= records.Count()) return false;
            position += 1;
            return true;
        }

        void Reset() { position = -1; }
    }
}
