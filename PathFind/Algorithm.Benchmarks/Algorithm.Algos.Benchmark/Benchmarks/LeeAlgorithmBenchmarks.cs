﻿using Algorithm.Algos.Algos;
using Algorithm.Interfaces;
using BenchmarkDotNet.Attributes;
using GraphLib.Interfaces;

namespace Algorithm.Algos.Benchmark.Benchmarks
{
    [MemoryDiagnoser]
    public class LeeAlgorithmBenchmarks : AlgorithmBenchmarks
    {
        protected override IAlgorithm CreateAlgorithm(IGraph graph, IEndPoints endPoints)
        {
            return new LeeAlgorithm(graph, endPoints);
        }
    }
}
