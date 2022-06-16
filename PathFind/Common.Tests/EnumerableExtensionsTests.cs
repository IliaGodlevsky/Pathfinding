﻿using Common.Extensions.EnumerableExtensions;
using NUnit.Framework;
using System.Linq;

namespace Common.Tests
{
    [TestFixture]
    public class EnumerableExtensionsTests
    {
        [TestCase(new int[] { 1, 2, 7, 100, 15, 33, -1 }, new int[] { 1, 2, 7, 100, 15, 33, -1 })]
        public void Juxtapose_ArraysAreEqual_ComparesForEquality_ReturnsTrue(int[] firstArray, int[] secondArray)
        {
            bool matches = firstArray.Juxtapose(secondArray, (a, b) => a == b);

            Assert.IsTrue(matches);
        }

        [TestCase(new int[] { 1, 2, 7, 100, 15, 11, -1 }, new int[] { 1, 3, 7, 100, 15, 33, -1 })]
        public void Juxtapose_ArraysAreNotEqual_CpmparesForEquality_ReturnsFalse(int[] firstArray, int[] secondArray)
        {
            bool matches = firstArray.Juxtapose(secondArray, (a, b) => a == b);

            Assert.IsFalse(matches);
        }

        [TestCase(new int[] { 1, 2, 3, 4, 5, 6, 7 }, new int[] { 2, 3, 4, 5, 6, 7, 8 })]
        public void Juxtapose_EachValueInFirstArrayIsLessThanInSecond_ComparesForLess_ReturnsTrue(
            int[] firstArray, int[] secondArray)
        {
            bool matches = firstArray.Juxtapose(secondArray, (a, b) => a < b);

            Assert.IsTrue(matches);
        }

        [TestCase(new int[] { }, new int[] { })]
        public void Juxtapose_EmptyCollections_ReturnsTrue(
            int[] firstArray, int[] secondArray)
        {
            bool matches = firstArray.Juxtapose(secondArray, (a, b) => a == b);

            Assert.IsTrue(matches);
        }

        [TestCase(10, new[] { 1, 2, 4, 5, 6, 5 })]
        [TestCase(6, new[] { 1, 2, 4, 5, 5 })]
        public void TakeOrDefault_TakeFromValues_RetunsValuesAndDefaults(int take, int[] values)
        {
            var taken = values.TakeOrDefault(take).ToArray();
            var notDefaultCount = taken.TakeWhile(i => i != default).Count();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(taken.Length == take);
                Assert.IsTrue(notDefaultCount == values.Length);
            });
        }
    }
}