using Moq;

namespace Pathfinding.App.Console.Tests;

[TypeMatcher]
internal sealed class IsAnyToken : ITypeMatcher, IEquatable<IsAnyToken>
{
    public bool Matches(Type typeArgument) => true;

    public bool Equals(IsAnyToken other) => true;

    public override bool Equals(object obj)
    {
        return Equals(obj as IsAnyToken);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}