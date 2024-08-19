﻿using Pathfinding.Shared.Interface;
using Pathfinding.Shared.Random;

namespace Pathfinding.Shared.Test.Random
{
    public class CryptoRandomTests : RandomTests
    {
        protected override IRandom Random { get; set; }

        [SetUp]
        public void Setup()
        {
            Random = new XorshiftRandom();
        }

        [TestCaseSource(typeof(RandomTestDataProviders), nameof(RandomTestDataProviders.CryptoRandomDataProvider))]
        public override void GetNextUInt_ShouldReturnValuesInTolerance(int limit, double tolerance)
        {
            base.GetNextUInt_ShouldReturnValuesInTolerance(limit, tolerance);
        }
    }
}
