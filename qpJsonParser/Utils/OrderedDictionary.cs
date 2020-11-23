using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
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
    public class OrderedDictionary<TKey, TValue> :
        IEnumerable<KeyValuePair<TKey, TValue>>,
        IOrderedDictionary<TKey, TValue>,
        IOrderedDictionary,
        IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>,
        ISerializable
        where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> dic;
        private readonly LinkedList<TKey> order = new LinkedList<TKey>();
        private LinkedListNode<TKey> GetKeyFromIndex(int index)
        {
            if ((uint) index >= order.Count)
                throw new IndexOutOfRangeException();

            LinkedListNode<TKey> node = order.First!;
            for (int i = 1; i < index; ++i)
            {
                node = node.Next!;
            }
            return node;
        }
        public TValue this[int index]
        {
            get => this[GetKeyFromIndex(index).Value];
            set => this[GetKeyFromIndex(index).Value] = value;
        }
        object? IOrderedDictionary.this[int index] { get => this[index]; set => this[index] = (TValue)value!; }

        public void Insert(int index, TKey key, TValue value)
        {
            if (this.Count <= (uint) index)
                throw new ArgumentOutOfRangeException();
            if (key is null)
                throw new ArgumentNullException(nameof(key));
            if (this.ContainsKey(key))
                throw new ArgumentException();

            order.AddBefore(GetKeyFromIndex(index), key);
            this[key] = value;
        }
        public void RemoveAt(int index)
        {
            if (this.Count <= (uint) index)
                throw new ArgumentOutOfRangeException();
            var key = GetKeyFromIndex(index);
            this.Remove(key.Value);
            order.Remove(key);
        }
        public OrderedDictionary()
        {
            dic = new();
        }
        public OrderedDictionary(IEqualityComparer<TKey> comparer)
        {
            dic = new(comparer);
        }
        public OrderedDictionary(int capacity)
        {
            dic = new(capacity);
        }
        public OrderedDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            dic = new(capacity, comparer);
        }

        private const string OrderSerializeName = "OrderedDictionary.order";

        public ICollection<TKey> Keys => ((IDictionary<TKey, TValue>) this.dic).Keys;

        public ICollection<TValue> Values => ((IDictionary<TKey, TValue>) this.dic).Values;

        public int Count => ((ICollection<KeyValuePair<TKey, TValue>>) this.dic).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>) this.dic).IsReadOnly;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => ((IReadOnlyDictionary<TKey, TValue>) this.dic).Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => ((IReadOnlyDictionary<TKey, TValue>) this.dic).Values;

        bool IDictionary.IsFixedSize { get; }
        ICollection IDictionary.Keys => new CollectionProxy<TKey>(this.Keys, this);
        ICollection IDictionary.Values => new CollectionProxy<TValue>(this.Values, this);
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => this;

        object? IDictionary.this[object key] { get => this[(TKey)key]; set => this[(TKey)key] = (TValue)value!; }
        public TValue this[TKey key] { get => ((IDictionary<TKey, TValue>) this.dic)[key]; set => ((IDictionary<TKey, TValue>) this.dic)[key] = value; }

        public OrderedDictionary(SerializationInfo info, StreamingContext context)
        {
            this.dic = new DeserializeDictionary(info, context);
            this.order = (LinkedList<TKey>) (info.GetValue(OrderSerializeName, typeof(LinkedList<TKey>)) ?? throw new SerializationException());
        }

        public void Insert(int index, object key, object? value) => this.Insert(index, (TKey) key, (TValue) value!);
        public void Add(TKey key, TValue value) => ((IDictionary<TKey, TValue>) this.dic).Add(key, value);
        public bool ContainsKey(TKey key) => ((IDictionary<TKey, TValue>) this.dic).ContainsKey(key);
        public bool Remove(TKey key) => ((IDictionary<TKey, TValue>) this.dic).Remove(key);
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => ((IDictionary<TKey, TValue>) this.dic).TryGetValue(key, out value);
        public void Add(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>) this.dic).Add(item);
        public void Clear() => ((ICollection<KeyValuePair<TKey, TValue>>) this.dic).Clear();
        public bool Contains(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>) this.dic).Contains(item);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection)this).CopyTo(array, arrayIndex);
        public bool Remove(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>) this.dic).Remove(item);
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            => new OrderedDictionaryEnumerator<TKey, TValue>(this, this.order.GetEnumerator());
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) this.dic).GetEnumerator();
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            dic.GetObjectData(info, context);
            info.AddValue(OrderSerializeName, order);
        }
        IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
            => new OrderedDictionaryEnumerator<TKey, TValue>(this, order.GetEnumerator());
        void IDictionary.Add(object key, object? value) => this.Add((TKey)key, (TValue)value!);
        bool IDictionary.Contains(object key) => key is TKey k && this.ContainsKey(k);
        IDictionaryEnumerator IDictionary.GetEnumerator() => ((IOrderedDictionary) this).GetEnumerator();
        void IDictionary.Remove(object key)
        {
            if (key is TKey k) this.Remove(k);
        }
        void ICollection.CopyTo(Array array, int index)
        {
            foreach (var pair in this)
            {
                array.SetValue(pair, index++);
            }
        }

        private class DeserializeDictionary : Dictionary<TKey, TValue>
        {
            internal DeserializeDictionary(SerializationInfo info, StreamingContext context)
                : base(info, context) { }
        }
        private class CollectionProxy<T> : ICollection
        {
            private readonly ICollection<T> collection;

            public CollectionProxy(ICollection<T> collection, object syncRoot)
            {
                this.collection = collection;
                this.SyncRoot = syncRoot;
            }

            public int Count => collection.Count;
            public bool IsSynchronized => false;
            public object SyncRoot { get; }

            public void CopyTo(Array array, int index) => collection.CopyTo((T[]) array, index);
            public IEnumerator GetEnumerator() => collection.GetEnumerator();
        }
    }
    public class OrderedDictionaryEnumerator<TKey, TValue> :
        IEnumerator<KeyValuePair<TKey, TValue>>,
        IDictionaryEnumerator
        where TKey : notnull
    {
        private readonly IOrderedDictionary<TKey, TValue> dic;
        private readonly IEnumerator<TKey> keys;

        public OrderedDictionaryEnumerator(IOrderedDictionary<TKey, TValue> dic, IEnumerator<TKey> keys)
        {
            this.dic = dic;
            this.keys = keys;
        }

        public KeyValuePair<TKey, TValue> Current => new(keys.Current, dic[keys.Current]);
        public DictionaryEntry Entry => new(Current.Key, Current.Value);
        public object Key => Current.Key;
        public object? Value => Current.Value;
        object IEnumerator.Current => Current;

        public void Dispose() => keys.Dispose();
        public bool MoveNext() => keys.MoveNext();
        public void Reset() => keys.Reset();
    }
}
