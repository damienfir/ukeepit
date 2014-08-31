using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Serialization;

namespace SafeBox.Burrow.DataTree
{
    internal struct PartialTreeNodeHeader
    {
        internal bool Read;
        internal byte Type;
        internal int ParentNodeIndex;
        internal ArraySegment<byte> Label;
        internal HashWithAesParameters HashWithAesParameters;   // For nodes of type 1 (sub node in other object) and 2 (B-Tree head)
        internal int ValueOffset;
        internal int ValueLength;
    }

    public class PartialTree
    {
        public static PartialTree From(BurrowObject obj) { return From(obj, obj.Data); }

        public static PartialTree From(BurrowObject obj, ArraySegment<byte> bytes)
        {
            // Check the absolute minimum size
            if (bytes.Count < 1 + 4) return null;

            // Read the mime type
            int mimeTypeLength = BigEndian.Int8(bytes.Array, bytes.Offset);
            if (bytes.Count < 1 + mimeTypeLength + 4) return null;
            var mimeType = System.Text.Encoding.UTF8.GetString(bytes.Array, bytes.Offset + 1, mimeTypeLength);

            // Read the node count
            var pos = bytes.Offset + 1 + mimeTypeLength + 4;
            int count = BigEndian.Int32(bytes.Array, pos);
            if (pos + 4 + count * 4 > bytes.Offset + bytes.Count) return null;

            // Read the offsets
            var offsets = new int[count + 1];
            offsets[0] = 0;
            for (var i = 1; i <= count; i++) offsets[i] = BigEndian.Int32(bytes.Array, bytes.Offset + i * 4);

            // Verify the length (we silently accept trailing data)
            var dataStart = 4 + count * 4;
            var dataLength = offsets[count];
            if (dataStart + dataLength < bytes.Count) return null;

            return new PartialTree(obj, offsets, new ArraySegment<byte>(bytes.Array, bytes.Offset + dataStart, dataLength));
        }

        public readonly BurrowObject Object;
        internal int[] offsets;
        internal ArraySegment<byte> data;
        internal PartialTreeNodeHeader[] nodes;

        public PartialTree(BurrowObject obj, int[] offsets, ArraySegment<byte> data)
        {
            this.Object = obj;
            this.offsets = offsets;
            this.data = data;

            // Reserve space for all node headers
            this.nodes = new PartialTreeNodeHeader[offsets.Length - 1];

            // Offset 0 contains the values of the root node (by definition)
            this.nodes[0].Read = true;
            this.nodes[0].Type = 0;
            this.nodes[0].ParentNodeIndex = 0;
            this.nodes[0].ValueOffset = offsets[0];
            this.nodes[0].ValueLength = offsets[1] - offsets[0];
        }

        private void ReadNodeHeader(int index)
        {
            if (nodes[index].Read) return;
            nodes[index].Read = true;

            // Get the position
            var pos = data.Offset + offsets[index];
            var end = data.Offset + offsets[index + 1];
            if (pos + 5 > end) return;

            // Read the type
            nodes[index].Type = BigEndian.UInt8(data.Array, pos);
            if (nodes[index].Type > 2) return;
            pos += 1;

            // Read the parent node and label
            nodes[index].ParentNodeIndex = BigEndian.Int32(data.Array, pos);
            pos += 4;
            var labelLength = BigEndian.Int16(data.Array, pos);
            pos += 2;
            nodes[index].Label = new ArraySegment<byte>(data.Array, pos, labelLength);

            if (nodes[index].Type == 0)
            {
                // Node with values
                nodes[index].ValueOffset = pos;
                nodes[index].ValueLength = end - pos;
            }
            else
            {
                // Subtree head or B-tree head
                nodes[index].HashWithAesParameters = new HashWithAesParameters(Hash.From(data.Array, pos), new ArraySegment<byte>(data.Array, pos + 32, 32), new ArraySegment<byte>(data.Array, pos + 32 + 16, 16));
            }
        }

        internal void MergeNodeValues(int index, Node node)
        {
            if (nodes[index].Type != 0) return;

            // Read all values
            var pos = nodes[index].ValueOffset;
            var end = nodes[index].ValueOffset + nodes[index].ValueLength;
            while (pos < end)
            {
                var type = BigEndian.UInt8(data.Array, pos);
                pos += 1;
                if (type == 255)
                {
                    // BROOM value
                    if (pos + 2 > end) return;
                    var len = BigEndian.UInt16(data.Array, pos);
                    pos += 2;
                    if (pos + len > end) return;
                    node.MergeBroom(new ArraySegment<byte>(data.Array, pos, len));
                    pos += len;
                }
                else if (type == 254)
                {
                    // TOP value
                    if (pos + 10 > end) return;
                    node.MergeTop(BigEndian.Int64(data.Array, pos), BigEndian.UInt16(data.Array, pos+8));
                    pos += 10;
                }
                else if (type == 253)
                {
                    // Referenced object
                    if (pos + 32 + 32 + 16 > end) return;
                    node.MergeHash(new HashWithAesParameters(Hash.From(data.Array, pos), new ArraySegment<byte>(data.Array, pos + 32, 32), new ArraySegment<byte>(data.Array, pos + 16, 16)));
                    pos += 32 + 32 + 16;
                }
                else if (type == 252)
                {
                    // Value with UInt32 length (we only handle 31 bits)
                    if (pos + 4 > end) return;
                    var len = BigEndian.Int32(data.Array, pos);
                    pos += 4;
                    if (pos + len > end) return;
                    node.MergeBytes(new ArraySegment<byte>(data.Array, pos, len));
                    pos += len;
                }
                else if (type == 251)
                {
                    // Value with UInt16 length
                    if (pos + 2 > end) return;
                    var len = BigEndian.UInt16(data.Array, pos);
                    pos += 2;
                    if (pos + len > end) return;
                    node.MergeBytes(new ArraySegment<byte>(data.Array, pos, len));
                    pos += len;
                }
                else
                {
                    // Short value
                    if (pos + type > end) return;
                    node.MergeBytes(new ArraySegment<byte>(data.Array, pos, type));
                    pos += type;
                }
            }
        }

        internal delegate void NotFound();
        internal delegate void Found(int index);
        internal delegate void FoundSubtreeHead(int index, int pathOffset);
        internal delegate void FoundBTreeHead(int index);

        internal void FindNodeIndex(ArraySegment<byte>[] pathSegments, int pathOffset, NotFound notFound, Found found, FoundSubtreeHead foundSubtreeHead, FoundBTreeHead foundBTreeHead)
        {
            // If the path has 0 length, then it's the root node
            if (pathOffset >= pathSegments.Length) { found(0); return; }

            // Binary search in the nodes
            var first = 1;                      // The first potential match
            var firstPathOffset = pathOffset;   // Until this offset, the path segments of the first potential match are correct
            var last = nodes.Length - 1;        // The last potential match
            var lastPathOffset = pathOffset;    // Until this offset, the path segments of the last potential match are correct
            var nodeStack = new FixedSizeStack(pathSegments.Length - pathOffset);
            while (first <= last)
            {
                var middle = (first + last) / 2;

                // Read all nodes along the path to the parent, and pile them up
                nodeStack.Reset();
                var currentNode = middle;
                while (currentNode > 0)
                {
                    ReadNodeHeader(currentNode);
                    nodeStack.Push(currentNode);
                    if (nodes[currentNode].ParentNodeIndex >= currentNode) { notFound(); return; }  // Error in the encoding of the tree
                    currentNode = nodes[currentNode].ParentNodeIndex;
                }

                // Check how many path segments we actually need to verify, and remove the rest
                var pathIndex = Math.Min(firstPathOffset, lastPathOffset);
                nodeStack.Pop(pathIndex - pathOffset);

                // Unpile the rest of the nodes and compare them to the path
                while (true)
                {
                    var nodeIndex = nodeStack.Pop();
                    var result = Static.Compare(nodes[nodeIndex].Label, pathSegments[pathIndex]);

                    // The path is bigger than this node, and therefore bigger than the middle node (which is a child of this)
                    if (result < 0) { first = middle + 1; firstPathOffset = pathIndex + 1; break; }

                    // The path is smaller than the current node
                    if (result > 0) { last = nodeIndex - 1; lastPathOffset = pathIndex + 1; break; }

                    // We are on the right path, and discovered a sub node
                    if (nodes[nodeIndex].Type == 1) { foundSubtreeHead(nodeIndex, pathIndex + 1); return; }

                    // We found the node
                    if (nodeStack.Length() == 0) { found(nodeIndex); return; }

                    // The path is a child of the current node, and therefore bigger
                    if (first < nodeIndex + 1) first = nodeIndex + 1;
                    pathIndex += 1;
                }
            }

            // Check if there is a B-tree node just in front of us
            if (first > 1)
            {
                ReadNodeHeader(first - 1);
                if (nodes[first - 1].Type == 2) { foundBTreeHead(first - 1); return; }
            }

            notFound();
        }
    }
}
