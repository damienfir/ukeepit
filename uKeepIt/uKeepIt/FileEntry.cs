using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uKeepIt.MiniBurrow;
using uKeepIt.MiniBurrow.Aes;
using uKeepIt.MiniBurrow.Serialization;

namespace uKeepIt
{
    // Serialization: path uint16+text | content id 32 | revision | length uint64 | date int64 | chunks (hash index uint32 | aes key 32 | aes nonce 16)
    public class FileEntry
    {
        public static FileEntry From(ArraySegment<byte> bytes, BurrowObject obj)
        {
            var byteReader = ByteReader.From(bytes);
            var path = byteReader.ReadUint16Text("");
            var contentId = Hash.From(byteReader.ReadBytes(32));
            if (contentId == null) return null;
            var revision = byteReader.ReadUint64(0);

            // If the entry stops here, the file has been deleted
            if (byteReader.AtEnd())
                return new FileEntry(path, contentId, revision, true, 0, 0, new ImmutableStack<ObjectReference>());

            // Read the rest
            var length = byteReader.ReadUint64(0);
            var date = byteReader.ReadInt64(0);
            var chunks = new ImmutableStack<ObjectReference>();
            while (!byteReader.AtEnd())
            {
                var hash = obj.HashAtIndex(byteReader.ReadInt32(-1));
                if (hash == null) break;
                var aesKey = byteReader.ReadBytes(32);
                if (aesKey.Array == null) break;
                var aesNonce = byteReader.ReadBytes(16);
                if (aesNonce.Array == null) break;
                chunks = chunks.With(new ObjectReference(hash, aesKey, aesNonce));
            }

            return new FileEntry(path, contentId, revision, false, length, date, chunks.Reversed());
        }

        public readonly string Path;
        public readonly Hash ContentId;
        public ulong Revision;
        public bool Deleted;
        public readonly ulong Length;
        public readonly long LastWriteTime;
        public readonly ImmutableStack<ObjectReference> Chunks;

        // These are temporary flags used by the Synchronizer, and not saved
        public bool Processed = false;
        public bool InUse = false;

        public FileEntry(string path, Hash contentId, ulong revision, bool deleted, ulong length, long lastWriteTime, ImmutableStack<ObjectReference> chunks)
        {
            this.Path = path;
            this.ContentId = contentId;
            this.Revision = revision;
            this.Deleted = deleted;
            this.Length = length;
            this.LastWriteTime = lastWriteTime;
            this.Chunks = chunks;
        }

        public ArraySegment<byte> Serialize(ObjectHeader objectHeader)
        {
            var bigEndian= new BigEndian();
            var byteWriter= new ByteWriter();
            byteWriter.Append(Path);
            byteWriter.Append(ContentId.Bytes());
            byteWriter.Append(bigEndian.UInt64(Revision));
            if (Deleted) return byteWriter.ToBytes();
            byteWriter.Append(bigEndian.UInt64(Length));
            byteWriter.Append(bigEndian.Int64(LastWriteTime));
            foreach (var chunk in Chunks) {
                byteWriter.Append(bigEndian.UInt32(objectHeader.Add(chunk.Hash)));
                byteWriter.Append(chunk.Key);
                byteWriter.Append(chunk.Nonce);
            }
            return byteWriter.ToBytes();
        }
    }
}
