using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace qpwakaba.Utils
{
    public class EmptyEnumerator<T> : IEnumerator<T>, IEnumerator
    {
        public T Current => throw new NotSupportedException();
        object IEnumerator.Current => this.Current;

        public bool MoveNext() => false;
        public void Reset() { }
        public void Dispose() { }
    }
}
