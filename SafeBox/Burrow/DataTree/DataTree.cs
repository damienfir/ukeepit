using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Serialization;
using SafeBox.Burrow.Backend;
using System.Threading;

namespace SafeBox.Burrow.DataTree
{
    // Immutable, can be used on multiple threads
    public class DataTree
    {
        public readonly ImmutableStack<HashWithAesParameters> Hashes;

        public DataTree(ImmutableStack<HashWithAesParameters> hashes)
        {
            this.Hashes = hashes;
        }

        public DataTree MergedWith(HashWithAesParameters hash)
        {
            return new DataTree(Hashes.With(hash));
        }


        /*
        // Traverses all children of a node, and calls the node handler once for each node. At the end, the done handler is called.
        public void TraverseChildren(Path path, NodeHandler nodeHandler, DoneHandler doneHandler)
        {

        }

        // Traverses all children of a node, and calls the node handler once for each node. At the end, the done handler is called.
        public void TraverseSubtree(ImmutableStack<byte[]> path, NodeHandler nodeHandler, DoneHandler doneHandler)
        {

        }

        // Traverses a range of nodes, and calls the node handler once for each node. At the end, the done handler is called.
        public void TraverseRange(ImmutableStack<byte[]> pathFrom, ImmutableStack<byte[]> pathTo, NodeHandler nodeHandler, DoneHandler doneHandler)
        {

        }
        */
    }
}
