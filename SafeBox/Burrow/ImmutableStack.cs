using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace SafeBox.Burrow
{
    public class ImmutableStack
    {
        //public static ImmutableStack<T> With<T>(T element) { return new ImmutableStack<T>(element); }

        public static ImmutableStack<T> With<T>(params T[] elements)
        {
            var list = new ImmutableStack<T>();
            foreach (var element in elements) list = list.With(element);
            return list;
        }

        public static ImmutableStack<T> From<T>(IEnumerable<T> elements) {
            var list = new ImmutableStack<T>();
            if (elements == null) return list;
            foreach (var element in elements) list = list.With(element);
            return list; 
        }

        public static ImmutableStack<T> FromNotNull<T>(IEnumerable<T> elements) {
            var list = new ImmutableStack<T>();
            if (elements == null) return list;
            foreach (var element in elements) if (element != null) list = list.With(element);
            return list; 
        }

    }

    public class ImmutableStack<T> : IEnumerable<T>
    {
        // *** Static ***

        // *** Object ***

        public readonly int Length;
        public readonly T Head;
        public readonly ImmutableStack<T> Tail;

        public ImmutableStack()
        {
            Length = 0;
            Head = default(T);
            Tail = null;
        }

        public ImmutableStack(T element)
        {
            Length = 1;
            Head = element;
            Tail = new ImmutableStack<T>();
        }

        public ImmutableStack(params T[] elements)
        {
            var tail = new ImmutableStack<T>();
            for (var i = 0; i < elements.Length - 1; i++)
                tail = new ImmutableStack<T>(tail, elements[i]);

            Length = elements.Length;
            Head = elements[elements.Length - 1];
            Tail = tail;
        }

        public ImmutableStack(ImmutableStack<T> stack, T element)
        {
            Length = stack.Length + 1;
            Head = element;
            Tail = stack;
        }

        public ImmutableStack<T> With(T element) { return new ImmutableStack<T>(this, element); }

        public ImmutableStack<T> With(params T[] elements) {
            var stack = this;
            foreach (var element in elements) stack = new ImmutableStack<T>(this, element); 
            return stack;
        }

        public ImmutableStack<T> With(IEnumerable<T> elements)
        {
            var stack = this;
            foreach (var element in elements) stack = new ImmutableStack<T>(this, element);
            return stack;
        }

        public ImmutableStack<T> WithNotNull(IEnumerable<T> elements)
        {
            var stack = this;
            foreach (var element in elements) if (element != null) stack = new ImmutableStack<T>(this, element);
            return stack;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() { return new ImmutableListEnumerator<T>(this); }
        IEnumerator IEnumerable.GetEnumerator() { return new ImmutableListEnumerator<T>(this); }

        public T[] ToArray()
        {
            var array = new T[Length];
            var i = 0;
            var element = this;
            while (i < Length)
            {
                array[i] = element.Head;
                i++;
                element = element.Tail;
            }
            return array;
        }
    }

    class ImmutableListEnumerator<T> : IEnumerator<T>
    {
        ImmutableStack<T> start;
        ImmutableStack<T> current;

        public ImmutableListEnumerator(ImmutableStack<T> list)
        {
            start = list;
            current = null;
        }

        object IEnumerator.Current
        {
            get { if (current == null) throw new System.InvalidOperationException("ImmutableListEnumerator is before the first or after the last element."); else return current.Head; }
        }

        T IEnumerator<T>.Current
        {
            get { if (current == null) throw new System.InvalidOperationException("ImmutableListEnumerator is before the first or after the last element."); else return current.Head; }
        }

        bool IEnumerator.MoveNext()
        {
            if (current == null)
            {
                // Before the first element (or after the last element)
                if (start == null || start.Length <= 0) return false;
                current = start;
            }
            else
            {
                // In the middle of the list
                if (current.Length <= 1) { current = null; return false; }
                current = current.Tail;
            }
            return true;
        }

        void IEnumerator.Reset()
        {
            current = null;
        }

        void IDisposable.Dispose()
        {
            current = null;
            start = null;
        }
    }
}
