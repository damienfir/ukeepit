using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uKeepIt.MiniBurrow.Serialization;

namespace uKeepIt
{
    // Serialization: path uint16+text | revision uint64 | deleted uint8
    public class FolderEntry
    {
        public static FolderEntry From(ArraySegment<byte> bytes , BurrowObject obj)
        {
            var byteReader = ByteReader.From(bytes);
            var path = byteReader.ReadUint16Text(null);
            if (path == null) return null;
            var revision = byteReader.ReadUint64(0);
            var deleted = byteReader.ReadUint8(0);
            return new FolderEntry(path, revision, deleted != 0);
        }

        public readonly string Path;
        public ulong Revision;
        public bool Deleted;

        // Used by the Synchronizer, and not saved
        public bool InUse;

        public FolderEntry(string path, ulong revision, bool deleted)
        {
            this.Path = path;
            this.Revision = revision;
            this.Deleted = deleted;
        }

        public ArraySegment<byte> Serialize(ObjectHeader objectHeader)
        {
            var bigEndian = new BigEndian();
            var byteWriter = new ByteWriter();
            byteWriter.Append(Path);
            byteWriter.Append(bigEndian.UInt64(Revision));
            byteWriter.Append(bigEndian.UInt8(Deleted ? (byte)1 : (byte)0));
            return byteWriter.ToBytes();
        }
    }
}
