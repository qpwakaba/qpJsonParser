using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace qpwakaba.Utils
{
    public static class EqualityComparerConnector
    {
        public static IEqualityComparer<T> ToGeneric<T>(this IEqualityComparer comparer) => new GenericConnector<T>(comparer);
        public static IEqualityComparer ToNonGeneric<T>(this IEqualityComparer<T> comparer) => new NonGenericConnector<T>(comparer);
        private class GenericConnector<T> : IEqualityComparer<T>
        {
            private readonly IEqualityComparer comparer;
            internal GenericConnector(IEqualityComparer comparer) => this.comparer = comparer;
            public bool Equals(T x, T y) => this.comparer.Equals(x, y);
            public int GetHashCode(T obj) => this.comparer.GetHashCode(obj);
        }
        private class NonGenericConnector<T> : IEqualityComparer
        {
            private readonly IEqualityComparer<T> comparer;
            internal NonGenericConnector(IEqualityComparer<T> comparer) => this.comparer = comparer;
            public new bool Equals(object x, object y) => this.comparer.Equals((T) x, (T) y);
            public int GetHashCode(object obj) => this.comparer.GetHashCode((T) obj);
        }
    }
}
