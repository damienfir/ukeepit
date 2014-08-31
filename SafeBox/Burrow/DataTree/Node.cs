using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.DataTree
{
    public class Node
    {
        public int TopValue = 0;
        public long TopDate = long.MinValue;
        public ArraySegment<byte> Broom;
        public List<ArraySegment<byte>> Bytes = new List<ArraySegment<byte>>();
        public List<HashWithAesParameters> Hashes = new List<HashWithAesParameters>();

        public Node() { }

        internal void MergeTop(long date, int value)
        {
            if (date <= TopDate) return;
            TopDate = date;
            TopValue = value;
        }

        internal void MergeBroom(ArraySegment<byte> broom)
        {
            if (Broom.Array != null && Static.Compare(broom, Broom) <= 0) return;
            Broom = broom;
        }

        internal void MergeHash(HashWithAesParameters newHash)
        {
            foreach (var hash in Hashes) if (hash.Equals(newHash)) return;
            Hashes.Add(newHash);
        }

        internal void MergeBytes(ArraySegment<byte> newBytes)
        {
            foreach (var bytes in Bytes) if (Static.IsEqual(bytes, newBytes)) return;
            Bytes.Add(newBytes);
        }
    }
}
