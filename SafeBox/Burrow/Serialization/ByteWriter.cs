using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.Serialization
{
    // A byte chain with fast append and traverse operations (at the expense of a little bit of memory overhead).
    // For optimization reasons, this is not an immutable chain.
    public class ByteWriter
    {
        ByteChain first;
        ByteChain last;
        int byteLength = 0;

        public ByteWriter(int bufferSize = 4) {
            first = new ByteChain(bufferSize); 
            last = first; 
        }

        // Append another chain (=concat). Note that the other chain is unusable afterwards.
        public void Append(ByteWriter byteWriter)
        {
            last.next = byteWriter.first;
            last = byteWriter.last;
            byteWriter.last = null;  // If somebody tries to reuse the old chain, this would immediately raise an exception.
            byteLength += byteWriter.byteLength;
        }

        // Append more bytes.
        public void Append(byte[] bytes) { Append(new ArraySegment<byte>(bytes)); }
        public void Append(string text) { Append(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(text))); }
        public void Append(ArraySegment<byte> bytes)
        {
            // Create a new chain if necessary
            if (last.used >= last.segments.Length)
            {
                var newChain = new ByteChain(last.segments.Length * 2);
                last.next = newChain;
                last = newChain;
            }

            // Add the segment
            last.segments[last.used] = bytes;
            last.used += 1;
            byteLength += bytes.Count;
        }

        public int ByteLength() { return byteLength; }

        public int WriteToByteArray(byte[] bytes, int offset)
        {
            for (var current = first; current != null; current = current.next)
            {
                for (var i = 0; i < current.used; i++)
                {
                    Array.Copy(current.segments[i].Array, current.segments[i].Offset, bytes, offset, current.segments[i].Count);
                    offset += current.segments[i].Count;
                }
            }
            return offset;
        }

        public byte[] ToBytes()
        {
            var bytes = new byte[byteLength];
            WriteToByteArray(bytes, 0);
            return bytes;
        }
    }

    internal class ByteChain
    {
        internal int used = 0;
        internal ArraySegment<byte>[] segments;
        internal ByteChain next = null;

        internal ByteChain(int bufferSize) { segments = new ArraySegment<byte>[bufferSize]; }
    }
}
