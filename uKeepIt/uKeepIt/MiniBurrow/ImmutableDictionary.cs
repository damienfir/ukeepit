using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace uKeepIt.MiniBurrow
{
    public class ImmutableDictionary
    {
        public static ImmutableDictionary<K,V> With<K,V>(K key, V value)
        {
            var list = new ImmutableDictionary<K,V>();
            return list.With(key, value);
        }

        public static ImmutableDictionary<K,V> From<K,V>(Dictionary<K, V> dictionary)
        {
            var list = new ImmutableDictionary<K, V>();
            if (dictionary == null) return list;
            foreach (var element in dictionary) list = list.With(element.Key, element.Value);
            return list;
        }
    }

    public class ImmutableDictionary<K, V> : IEnumerable<KeyValuePair<K, V>>
    {
        public readonly int Length;
        public readonly KeyValuePair<K, V> Head;
        public readonly ImmutableDictionary<K,V> Tail;

        public ImmutableDictionary()
        {
            Length = 0;
            Head = default(KeyValuePair<K, V>);
            Tail = null;
        }

        public ImmutableDictionary(K key, V value)
        {
            Length = 1;
            Head = new KeyValuePair<K,V>(  key, value);
            Tail = new ImmutableDictionary<K,V>();
        }

        public ImmutableDictionary(KeyValuePair<K, V> pair)
        {
            Length = 1;
            Head = pair;
            Tail = new ImmutableDictionary<K, V>();
        }

        public ImmutableDictionary(ImmutableDictionary<K,V> tail, K key, V value)
        {
            Length = tail.Length + 1;
            Head = new KeyValuePair<K, V>(key, value);
            Tail = tail;
        }

        public ImmutableDictionary(ImmutableDictionary<K, V> tail, KeyValuePair<K, V> pair)
        {
            Length = tail.Length + 1;
            Head = pair;
            Tail = tail;
        }

        public ImmutableDictionary<K, V> With(K key, V value) { return new ImmutableDictionary<K, V>(this, key, value); }

        public ImmutableDictionary<K, V> With(params KeyValuePair<K, V>[] elements) { return With(elements); }

        public ImmutableDictionary<K, V> With(IEnumerable<KeyValuePair<K, V>> elements)
        {
            var dictionary = this;
            foreach (var element in elements) dictionary = new ImmutableDictionary<K, V>(this, element);
            return dictionary;
        }

        IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator() { return new ImmutableDictionaryEnumerator<K, V>(this); }
        IEnumerator IEnumerable.GetEnumerator() { return new ImmutableDictionaryEnumerator<K,V>(this); }

        public K[] Keys()
        {
            var array = new K[Length];
            var element = this;
            for (var i = 0; i < Length; i++, element = element.Tail)
                array[i] = element.Head.Key;
            return array;
        }

        public V[] Values()
        {
            var array = new V[Length];
            var element = this;
            for (var i = 0; i < Length; i++, element = element.Tail)
                array[i] = element.Head.Value;
            return array;
        }

        public V Get(K key)
        {
            for (var element = this; element != null; element = element.Tail)
                if (element.Head.Key.Equals(key)) return element.Head.Value;
            return default(V);
        }

        public KeyValuePair<K, V> GetPair(K key)
        {
            for (var element = this; element != null; element = element.Tail)
                if (element.Head.Key.Equals(key)) return element.Head;
            return default(KeyValuePair<K, V>);
        }

        public bool Exists(K key)
        {
            for (var element = this; element != null; element = element.Tail)
                if (element.Head.Key.Equals(key)) return true;
            return false;
        }

        // Note that this is not very efficient right now (N^2), but OK for small dictionaries
        public ImmutableDictionary<K, V> Normalized()
        {
            var newDictionary = new ImmutableDictionary<K, V>();
            for (var element = this; element != null; element = element.Tail)
                if (!newDictionary.Exists(element.Head.Key)) newDictionary = newDictionary.With(element.Head);
            return newDictionary;
        }

        public Dictionary<K, V> ToDictionary()
        {
            var dictionary = new Dictionary<K, V>();
            for (var element = this; element != null; element = element.Tail)
                dictionary[element.Head.Key] = element.Head.Value;
            return dictionary;
        }
    }

    class ImmutableDictionaryEnumerator<K, V> : IEnumerator<KeyValuePair<K, V>>
    {
        ImmutableDictionary<K,V> start;
        ImmutableDictionary<K,V> current;

        public ImmutableDictionaryEnumerator(ImmutableDictionary<K,V> list)
        {
            start = list;
            current = null;
        }

        object IEnumerator.Current
        {
            get { if (current == null) throw new System.InvalidOperationException("ImmutableDictionaryEnumerator is before the first or after the last element."); else return current.Head; }
        }

        KeyValuePair<K, V> IEnumerator<KeyValuePair<K, V>>.Current
        {
            get { if (current == null) throw new System.InvalidOperationException("ImmutableDictionaryEnumerator is before the first or after the last element."); else return current.Head; }
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
