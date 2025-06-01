using Bogus;
using Pathfinding.App.Console.Models;
using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.App.Console.Tests;

internal static class Generators
{
    private static readonly Faker<GraphInfoModel> graphInfoFaker;

    static Generators()
    {
        graphInfoFaker = new Faker<GraphInfoModel>()
            .RuleFor(x => x.Id, x => x.IndexFaker)
            .RuleFor(x => x.Status, x => x.Random.Enum<GraphStatuses>())
            .RuleFor(x => x.Neighborhood, x => x.Random.Enum<Neighborhoods>())
            .RuleFor(x => x.SmoothLevel, x => x.Random.Enum<SmoothLevels>())
            .RuleFor(x => x.Length, x => x.Random.Int(10, 25))
            .RuleFor(x => x.Width, x => x.Random.Int(10, 25))
            .RuleFor(x => x.Name, x => x.Person.UserName)
            .RuleFor(x => x.ObstaclesCount, x => x.Random.Int(15, 25));
    }

    public static List<GraphInfoModel> GenerateGraphInfos(int number)
    {
        return graphInfoFaker.Generate(number);
    }
}