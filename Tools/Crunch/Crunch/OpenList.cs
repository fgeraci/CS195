using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crunch
{
    public class OpenList<TValue, TEdge>
    {
        public static EntryComparer CreateComparer(
            IComparer<TValue> valueComparer,
            IEqualityComparer<TValue> equalityComparer,
            int heuristicWeight)
        {
            return
                new EntryComparer(
                    valueComparer, 
                    equalityComparer, 
                    heuristicWeight);
        }

        public class Entry
        {
            public readonly TValue Value;
            public readonly IList<TEdge> Path;
            public readonly int Cost;

            internal Entry(TValue value, IList<TEdge> path)
            {
                this.Value = value;
                this.Path = path;
                this.Cost = path.Count;
            }
        }

        private class TupleComparer : IEqualityComparer<Tuple<TValue, TValue>>
        {
            private IEqualityComparer<TValue> valueComparer;

            public TupleComparer(IEqualityComparer<TValue> valueComparer)
            {
                this.valueComparer = valueComparer;
            }

            public bool Equals(Tuple<TValue, TValue> x, Tuple<TValue, TValue> y)
            {
                // TODO: Might want to replace this with just ReferenceEquals()
                //if (this.valueComparer.Equals(x.Item1, y.Item1) == false)
                //    return false;
                //if (this.valueComparer.Equals(x.Item2, y.Item2) == false)
                //    return false;

                return 
                    object.ReferenceEquals(x.Item1, y.Item1) && 
                    object.ReferenceEquals(x.Item2, y.Item2);
            }

            public int GetHashCode(Tuple<TValue, TValue> obj)
            {
                return obj.Item1.GetHashCode() ^ obj.Item2.GetHashCode();
            }
        }

        public class EntryComparer : IComparer<Entry>
        {
            private IComparer<TValue> comparer;
            private int heuristicWeight;
            private ConcurrentDictionary<Tuple<TValue, TValue>, int> cache;

            private int? GetCached(Tuple<TValue, TValue> key)
            {
                int cached;
                if (this.cache.TryGetValue(key, out cached) == true)
                    return cached;
                return null;
            }

            private void Store(Tuple<TValue, TValue> key, int value)
            {
                this.cache[key] = value;
            }

            internal EntryComparer(
                IComparer<TValue> valueComparer,
                IEqualityComparer<TValue> equalityComparer,
                int heuristicWeight)
            {
                this.cache =
                    new ConcurrentDictionary<Tuple<TValue, TValue>, int>(
                        new TupleComparer(equalityComparer));
                this.comparer = valueComparer;
                this.heuristicWeight = heuristicWeight;
            }

            public int Compare(Entry x, Entry y)
            {
                Tuple<TValue, TValue> key =
                    new Tuple<TValue, TValue>(x.Value, y.Value);
                int? cached = this.GetCached(key);
                if (cached != null)
                    return cached.Value;
                int val = ComputeComparison(x, y);
                this.Store(key, val);
                return val;
            }

            private int ComputeComparison(Entry x, Entry y)
            {
                int matrixComparison =
                    this.comparer.Compare(x.Value, y.Value);
                int costComparison = x.Cost.CompareTo(y.Cost);
                int total =
                    (matrixComparison * heuristicWeight)
                    + costComparison;
                return total;
            }
        }

        private Heap<Entry> heap;
        private EntryComparer comparer;

        public int Count
        {
            get { return this.heap.Count; }
        }

        public OpenList(EntryComparer comparer, int heuristicWeight)
        {
            this.comparer = comparer;
            this.heap = new MinHeap<Entry>(this.comparer);
        }

        public bool Pop(out TValue value, out IList<TEdge> path)
        {
            if (this.heap.Count > 0)
            {
                Entry entry = this.heap.Pop();
                value = entry.Value;
                path = entry.Path;
                return true;
            }
            value = default(TValue);
            path = null;
            return false;
        }

        public void Add(TValue value, IList<TEdge> path)
        {
            this.heap.Add(new Entry(value, path));
        }
    }
}
