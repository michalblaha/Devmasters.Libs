using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Devmasters.Collections
{

    public class CircularIterator<T> : IEnumerable<T>
    {
        public CircularIterator(IEnumerable<T> collection)
        {
            _enumerator = CreateCircularEnumerator(collection, false).GetEnumerator();
            _enumType = _enumerator.GetType();
        }

        public void Stop()
        {
            _enumType.GetField("stop").SetValue(_enumerator, true);
        }

        private readonly IEnumerator<T> _enumerator;
        private readonly Type _enumType;

        /// <exception cref="InvalidOperationException">CircularEnumerable cannot work with empty collections.</exception>
        private static IEnumerable<T> CreateCircularEnumerator(IEnumerable<T> collection, bool stop)
        {
            if (collection == null || collection.Count() <= 0)
            {
                throw new InvalidOperationException("CircularIterator cannot work with empty collections.");
            }
            IEnumerator<T> flatEnumerator = collection.GetEnumerator();
            flatEnumerator.Reset();
            flatEnumerator.MoveNext();//Point to the first element in the collection
            do
            {
                T current = flatEnumerator.Current;
                if (!flatEnumerator.MoveNext())
                {
                    flatEnumerator.Reset();
                    flatEnumerator.MoveNext();
                }
                yield return current;

            } while (!stop);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /* SAMPLE
        public void ArrayTest()
        {
            int[] ar = { 1, 2, 3, 4, 5 };
            int[] ver = { 1, 2, 3, 4, 5, 1, 2, 3, 4, 5 };
 
            var citr = (new CircularEnumerable<int>(ar)).GetEnumerator();
 
            for (int i = 0; i < 10; i++)
            {
                citr.MoveNext();
                Assert.AreEqual(ver[i], citr.Current);
            }
        }
 
        [TestMethod]
        public void ListTest()
        {
            List<string> l = new List<string> { "bibi", "kaka" };
            List<string> verl = new List<string> { "bibi", "kaka", "bibi", "kaka", "bibi", "kaka" };
 
            var citr = (new CircularEnumerable<string>(l)).GetEnumerator();
 
            for (int i = 0; i < 6; i++)
            {
                citr.MoveNext();
                Assert.AreEqual(verl[i], citr.Current);
            }
        }</pre>
    */
}

