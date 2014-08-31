using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.DataTree
{
    // This class keeps a stack and offers the usual push and pop functions.
    // However, the stack has a limited size, and only the top N elements of the stack are kept. When pushing on a "full" stack, the tail element will be deleted, and the overflow flag will be set.
    internal class FixedSizeStack
    {
        private int[] elements;
        private int head = 0;
        private int length = 0;
        private bool overflow = false;

        internal FixedSizeStack(int length)
        {
            this.elements = new int[length];
        }

        internal void Reset() { head = 0; length = 0; overflow = false; }

        internal void Push(int element)
        {
            elements[head] = element;
            head += 1;
            if (head > elements.Length) head -= elements.Length;
            length += 1;
            if (length > elements.Length) { length = elements.Length; overflow = true; }
        }

        internal int Pop()
        {
            if (length == 0) return -1;
            length -= 1;
            head -= 1;
            if (head < 0) head += elements.Length;
            return elements[head];
        }

        internal void Pop(int count)
        {
            length -= count;
            if (length < 0) { length = 0; head = 0; return; }
            head -= count;
            if (head < 0) head += elements.Length;
        }

        internal int Length() { return length; }
        internal bool Overflow() { return overflow; }
    }
}
