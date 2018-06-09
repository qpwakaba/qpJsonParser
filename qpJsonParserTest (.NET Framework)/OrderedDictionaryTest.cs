using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using qpwakaba.Utils;

namespace qpwakaba.Tests
{
    [TestClass]
    public class OrderedDictionaryTest
    {
        [TestMethod]
        public void Test()
        {
            var dictionary = new OrderedDictionary<string, int>();
            for (int i = 0; i < 65536; i++)
            {
                dictionary[i.ToString()] = i;
            }

            int x = 0;
            dictionary.ToList().ForEach(e =>
            {
                Assert.AreEqual(x.ToString(), e.Key);
                Assert.AreEqual(x, e.Value);
                x++;
            });
        }
    }
}
