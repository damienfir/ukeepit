using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.Serialization
{
    public class HashCollector
    {
        private List<Hash> hashes = new List<Hash>();

        public HashCollector() { }

        public uint Add(Hash hash)
        {
            var index = (uint)hashes.Count;
            hashes.Add(hash);
            return index;
        }

        public int ByteLength() { return 4 + hashes.Count; }

        public void WriteToByteArray(byte[] bytes, int offset)
        {
            var c = hashes.Count;
            bytes[0] = (byte)(c >> 24);
            bytes[1] = (byte)(c >> 16);
            bytes[2] = (byte)(c >> 8);
            bytes[3] = (byte)c;
            for (var i = 0; i < c; i++) hashes[i].WriteToByteArray(bytes, 4 + i * 32);
        }
    }
}
