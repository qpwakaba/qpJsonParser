using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Text;

namespace qpwakaba.Utils
{
    public interface IOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        TValue this[int index] { get; set; }
        void Insert(int index, TKey key, TValue value);
        void RemoveAt(int index);
    }
    public class OrderedDictionary<TKey, TValue> : IOrderedDictionary<TKey, TValue>, IOrderedDictionary, IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        private readonly OrderedDictionary dictionary;
        public OrderedDictionary() : this(null) { }
        public OrderedDictionary(IEqualityComparer<TKey> comparer)
            => this.dictionary = new OrderedDictionary((comparer ?? EqualityComparer<TKey>.Default).ToNonGeneric());
        public OrderedDictionary(int capacity) : this(capacity, null) { }
        public OrderedDictionary(int capacity, IEqualityComparer<TKey> comparer)
            => this.dictionary = new OrderedDictionary(capacity, (comparer ?? EqualityComparer<TKey>.Default).ToNonGeneric());

        public TValue this[int index]
        {
            get => (TValue) this.dictionary[index];
            set => this.dictionary[index] = value;
        }
        public TValue this[TKey key]
        {
            get => (TValue) this.dictionary[key];
            set => this.dictionary[key] = value;
        }

        object IDictionary.this[object key]
        {
            get {
                if (!(key is TKey))
                    throw new KeyNotFoundException();
                return this[(TKey) key];
            }
            set => this[(TKey) key] = (TValue) value;
        }
        object IOrderedDictionary.this[int index]
        {
            get => this[index];
            set => this[index] = (TValue) value;
        }

        public ICollection<TKey> Keys { get; }

        public ICollection<TValue> Values { get; }

        public int Count => this.dictionary.Count;

        public bool IsReadOnly => this.dictionary.IsReadOnly;

        public bool IsFixedSize => false;

        public bool IsSynchronized => false;

        public object SyncRoot => this.dictionary;

        ICollection IDictionary.Keys => (ICollection) this.Keys;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get {
                var enumerator = this.GetEnumerator();
                while (enumerator.MoveNext())
                    yield return enumerator.Current.Key;
            }
        }

        ICollection IDictionary.Values => (ICollection) this.Values;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get {
                var enumerator = this.GetEnumerator();
                while (enumerator.MoveNext())
                    yield return enumerator.Current.Value;
            }
        }

        public void Add(TKey key, TValue value) => this.dictionary.Add(key, value);
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
            => this.Add(item.Key, item.Value);
        public void Add(object key, object value) => this.Add((TKey) key, (TValue) value);
        public void Clear() => this.dictionary.Clear();
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            if (!this.ContainsKey(item.Key))
                return false;
            return EqualityComparer<TValue>.Default.Equals(item.Value, this[item.Key]);
        }
        bool IDictionary.Contains(object key) => this.dictionary.Contains(key);
        public bool ContainsKey(TKey key) => ((IDictionary) this).Contains(key);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            var enumerator = this.dictionary.GetEnumerator();
            enumerator.MoveNext();
            for (int i = 0; i < this.dictionary.Count; i++, enumerator.MoveNext())
            {
                var entry = (DictionaryEntry) enumerator.Current;
                array[i + index] = new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value);
            }
        }
        void ICollection.CopyTo(Array array, int index) => this.dictionary.CopyTo(array, index);

        public void Insert(int index, object key, object value) => this.dictionary.Insert(index, key, value);
        public void Insert(int index, TKey key, TValue value) => this.Insert(index, key, value);
        public bool Remove(TKey key)
        {
            if (this.ContainsKey(key))
            {
                ((IDictionary) this).Remove(key);
                return !this.ContainsKey(key);
            }
            return false;
        }
        void IDictionary.Remove(object key) => this.dictionary.Remove(key);
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!this.ContainsKey(item.Key))
                return false;
            if (EqualityComparer<TValue>.Default.Equals(item.Value, this[item.Key]))
            {
                return this.Remove(item.Key);
            }
            return false;
        }
        public void RemoveAt(int index) => this.dictionary.RemoveAt(index);
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (this.ContainsKey(key))
            {
                value = this[key];
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var key in this.Keys)
                yield return new KeyValuePair<TKey, TValue>(key, this[key]);
        }
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        IDictionaryEnumerator IOrderedDictionary.GetEnumerator() => this.dictionary.GetEnumerator();
        IDictionaryEnumerator IDictionary.GetEnumerator() => this.dictionary.GetEnumerator();
    }
}
