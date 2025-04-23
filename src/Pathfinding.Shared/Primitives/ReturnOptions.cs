using Pathfinding.Shared.Extensions;
using System.Runtime.CompilerServices;

namespace Pathfinding.Shared.Primitives
{
    public abstract class ReturnOptions
    {
        public static readonly ReturnOptions Limit = new LimitReturnOptions();
        public static readonly ReturnOptions Cycle = new CycleReturnOptions();

        internal T ReturnInRange<T>(T value, InclusiveValueRange<T> range)
            where T : IComparable<T>
        {
            if (value.IsGreaterThan(range.UpperValueOfRange)) return GetIfGreater(range);
            return value.IsLessThan(range.LowerValueOfRange) ? GetIfLess(range) : value;
        }

        protected abstract T GetIfGreater<T>(InclusiveValueRange<T> range) where T : IComparable<T>;

        protected abstract T GetIfLess<T>(InclusiveValueRange<T> range) where T : IComparable<T>;

        private sealed class CycleReturnOptions : ReturnOptions
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override T GetIfGreater<T>(InclusiveValueRange<T> range) => range.LowerValueOfRange;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override T GetIfLess<T>(InclusiveValueRange<T> range) => range.UpperValueOfRange;
        }

        private sealed class LimitReturnOptions : ReturnOptions
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override T GetIfGreater<T>(InclusiveValueRange<T> range) => range.UpperValueOfRange;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override T GetIfLess<T>(InclusiveValueRange<T> range) => range.LowerValueOfRange;
        }
    }
}
