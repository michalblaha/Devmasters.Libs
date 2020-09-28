using System.Collections.Generic;

namespace Devmasters.Collections
{

    public class GroupedCollection<T> : Dictionary<T, int>
    {
        public GroupedCollection()
            : base()
        { }

        public GroupedCollection(IEqualityComparer<T> comparer)
            : base(comparer)
        { }


        List<int> counts = new List<int>();

        public void Add(T obj)
        {

            if (!this.ContainsKey(obj))
            {
                base.Add(obj, 1);
            }
            else
            {
                this[obj]++;
            }

        }
        public void AddRange(IEnumerable<KeyValuePair<T, int>> collection)
        {
            foreach (KeyValuePair<T, int> c in collection)
            {
                if (!this.ContainsKey(c.Key))
                    this.Add(c.Key, c.Value);
            }
        }

        public int GroupCount(T obj)
        {
            if (this.ContainsKey(obj))
                return this[obj];
            else
                throw new KeyNotFoundException();
        }


    }
}
