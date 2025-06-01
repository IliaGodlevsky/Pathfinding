namespace Pathfinding.Shared.Interface;

public interface ICloneable<out T>
    where T : ICloneable<T>
{
    T DeepClone();
}