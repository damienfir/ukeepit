using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Backend;
using SafeBox.Burrow.Serialization;

namespace SafeBox.Burrow.DataTree.Operations
{
    public class ReadNode
    {
        public delegate void Progress(int done, int total);
        private readonly Progress progressHandler;
        public delegate void Done(Node node);
        private readonly Done doneHandler;

        private readonly DataTree dataTree;
        internal readonly ArraySegment<byte>[] pathSegments;
        internal readonly Cache cache;
        internal readonly ImmutableStack<ObjectStore> objectStores;
        internal readonly Node node = new Node();
        private int done = 0;

        public ReadNode(DataTree dataTree, Path path, Cache cache, ImmutableStack<ObjectStore> objectStores, Progress progressHandler, Done doneHandler)
        {
            this.dataTree = dataTree;
            this.pathSegments = path.Labels.ToArray();
            this.cache = cache;
            this.objectStores = objectStores;
            this.progressHandler = progressHandler;
            this.doneHandler = doneHandler;
            foreach (var hash in dataTree.Hashes) new ReadNodePartial(this, hash, 0);
        }

        internal void ReadNodePartialDone()
        {
            done += 1;
            if (progressHandler!= null) progressHandler(done, dataTree.Hashes.Length);
            if (done <= dataTree.Hashes.Length) return;
            doneHandler(node);
        }
    }

    class ReadNodePartial
    {
        private ReadNode readNode;
        private HashWithAesParameters hash;
        private int pathOffset;
        private PartialTree partialTree;

        internal ReadNodePartial(ReadNode readNode, HashWithAesParameters hash, int pathOffset)
        {
            this.readNode = readNode;
            this.hash = hash;
            this.pathOffset = pathOffset;
            readNode.cache.PartialTree(hash, readNode.objectStores, PartialTreeAvailable);
        }

        void PartialTreeAvailable(PartialTree partialTree)
        {
            if (partialTree == null) { readNode.ReadNodePartialDone(); return; }
            this.partialTree = partialTree;
            partialTree.FindNodeIndex(readNode.pathSegments, pathOffset, NotFound, Found, FoundSubtreeHead, FoundBTreeHead);
        }

        void NotFound() { readNode.ReadNodePartialDone(); }
        void Found(int index) { partialTree.MergeNodeValues(index, readNode.node); readNode.ReadNodePartialDone(); }
        void FoundSubtreeHead(int index, int pathOffset) { new ReadNodePartial(readNode, partialTree.nodes[index].HashWithAesParameters, pathOffset); }
        void FoundBTreeHead(int index) { new ReadNodePartial(readNode, partialTree.nodes[index].HashWithAesParameters, pathOffset); }
    }

}
