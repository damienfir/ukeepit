using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.DataTree
{
    public class Path
    {
        public readonly ImmutableStack<ArraySegment<byte>> Labels;

        public Path() { Labels = new ImmutableStack<ArraySegment<byte>>(); }
        public Path(params ArraySegment<byte>[] labels) { Labels = new ImmutableStack<ArraySegment<byte>>(labels); }
        public Path(params byte[][] labels) { Labels = new ImmutableStack<ArraySegment<byte>>().With(labels.Select(label => new ArraySegment<byte>(label))); }
        public Path(params string[] labels) { Labels = new ImmutableStack<ArraySegment<byte>>().With(labels.Select(label => new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(label)))); }
        public Path(ImmutableStack<ArraySegment<byte>> labels) { this.Labels = labels; }

        public Path Node(ArraySegment<byte> label) { return new Path(Labels.With(label)); }
        public Path Node(byte[] label) { return new Path(Labels.With(new ArraySegment<byte>(label))); }
        public Path Node(string label) { return new Path(Labels.With(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(label)))); }
        public Path Parent() { return Labels.Length > 0 ? new Path(Labels.Tail) : null; }
    }
}
