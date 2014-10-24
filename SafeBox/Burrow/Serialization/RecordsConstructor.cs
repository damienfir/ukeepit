using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SafeBox.Burrow.Serialization
{
    public class RecordsConstructor
    {
        public List<ByteWriter> Records = new List<ByteWriter>();
        public RecordsConstructor() { }
        public void Add(ByteWriter byteChain) { Records.Add(byteChain); }

        public ByteWriter Serialize()
        {
            // Record count
            var header = new byte[4 + Records.Count * 4];
            BigEndian.Int32(Records.Count, header, 0);

            // Calculate and add the offsets
            uint offset = 0;
            for (var i = 0; i < Records.Count; i++)
            {
                offset += (uint)Records[i].ByteLength();
                BigEndian.UInt32(offset, header, 4 + i * 4);
            }

            // Add header and all records
            var byteChain = new ByteWriter(1);
            byteChain.Append(header);
            foreach (var record in Records) byteChain.Append(record);
            return byteChain;
        }
    }
}
