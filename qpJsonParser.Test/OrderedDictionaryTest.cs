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
            var dictionaryRev = new OrderedDictionary<string, int>();
            for (int i = 0; i < 65536; i++)
            {
                dictionary[i.ToString()] = i;
                dictionaryRev[(65535 - i).ToString()] = i;
            }

            int x = 0;
            foreach (var e in dictionary) 
            {
                Assert.AreEqual(x.ToString(), e.Key);
                Assert.AreEqual(x, e.Value);
                x++;
            }

            x = 65535;
            foreach (var e in dictionary) 
            {
                Assert.AreEqual((65535 - x).ToString(), e.Key);
                Assert.AreEqual(x, e.Value);
                x--;
            }
        }
    }
}
