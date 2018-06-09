using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace qpwakaba.Utils
{
    public static class EnumeratorEnumerable
    {
        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
            => new Enumerable<T>(enumerator);
        private class Enumerable<T> : IEnumerable<T>
        {
            private readonly IEnumerator<T> enumerator;
            internal Enumerable(IEnumerator<T> enumerator) => this.enumerator = enumerator;
            public IEnumerator<T> GetEnumerator() => this.enumerator;
            IEnumerator IEnumerable.GetEnumerator() => this.enumerator;
        }
    }
}
