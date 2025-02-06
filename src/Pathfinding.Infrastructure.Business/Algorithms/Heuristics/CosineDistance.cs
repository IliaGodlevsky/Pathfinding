using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms.Heuristics
{
    public sealed class CosineDistance : IHeuristic
    {
        private const double Radians = 57.2956;

        public double Calculate(IPathfindingVertex first, IPathfindingVertex second)
        {
            var firstVector = first.Position;
            var secondVector = second.Position;
            double scalarProduct = GetScalarProduct(firstVector.CoordinatesValues,
                secondVector.CoordinatesValues);
            double firstVectorLength = GetVectorLength(firstVector.CoordinatesValues);
            double secondVectorLength = GetVectorLength(secondVector.CoordinatesValues);
            double vectorSum = firstVectorLength * secondVectorLength;
            double cosine = vectorSum > 0 ? scalarProduct / vectorSum : 0;
            return Math.Round(Radians * Math.Acos(cosine), digits: 10);
        }

        private static double GetScalarProduct(int[] self,
            int[] coordinate)
        {
            double result = default;
            for (int i = 0; i < self.Length; i++)
            {
                result += self[i] * coordinate[i];
            }
            return result;
        }

        private static double GetVectorLength(int[] self)
        {
            double result = 0;
            for (int i = 0; i < self.Length; i++)
            {
                result += self[i] * self[i];
            }
            return Math.Sqrt(result);
        }
    }
}