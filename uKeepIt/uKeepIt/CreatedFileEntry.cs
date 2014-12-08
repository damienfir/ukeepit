using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uKeepIt.MiniBurrow;
using uKeepIt.MiniBurrow.Aes;

namespace uKeepIt
{
    class CreatedFileEntry
    {
        public readonly string Path;
        public readonly DateTime LastWriteTime;
        public readonly long Length;
        public readonly Hash ContentId;
        public readonly ImmutableStack<ObjectReference> Chunks;


        public CreatedFileEntry(string path, DateTime lastWriteTime, long size, Hash contentId, ImmutableStack<ObjectReference> chunks)
        {
            this.Path = path;
            this.LastWriteTime = lastWriteTime;
            this.Length = size;
            this.ContentId = contentId;
            this.Chunks = chunks;
        }
    }
}
