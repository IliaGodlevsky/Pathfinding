using Pathfinding.Shared.Primitives;

namespace Pathfinding.Shared.Extensions
{
    public static class InclusiveValueRangeExtensions
    {
        public struct RangeEnumerator
        {
            private readonly int start;
            private readonly int end;

            public int Current { get; private set; }

            internal RangeEnumerator(int start, int end)
            {
                this.start = start;
                this.end = end;
                Current = start - 1;
            }

            internal RangeEnumerator(InclusiveValueRange<int> range)
                : this(range.LowerValueOfRange, range.UpperValueOfRange)
            {

            }

            public bool MoveNext()
            {
                return ++Current <= end;
            }

            public void Reset()
            {
                Current = start - 1;
            }

            public void Dispose()
            {
                Reset();
            }
        }

        /// <summary>
        /// Returns all values from <paramref name="range"/>
        /// </summary>
        /// <param name="range"></param>
        /// <returns>An enumerable, that contains all 
        /// values from <paramref name="range"/></returns>
        public static IEnumerable<int> Iterate(this InclusiveValueRange<int> range)
        {
            foreach (var item in range) yield return item;
        }

        public static IEnumerable<int> Iterate(this (int LowerValueRange, int UpperValueRange) range)
        {
            var valueRange = new InclusiveValueRange<int>(range.LowerValueRange, range.UpperValueRange);
            return Iterate(valueRange);
        }

        public static RangeEnumerator GetEnumerator(this InclusiveValueRange<int> range)
        {
            return new(range);
        }

        public static T ReturnInRange<T>(this InclusiveValueRange<T> self, T value, ReturnOptions options)
            where T : IComparable<T>
        {
            return options.ReturnInRange(value, self);
        }

        public static T ReturnInRange<T>(this InclusiveValueRange<T> self, T value)
            where T : IComparable<T>
        {
            return self.ReturnInRange(value, ReturnOptions.Limit);
        }
    }
}
