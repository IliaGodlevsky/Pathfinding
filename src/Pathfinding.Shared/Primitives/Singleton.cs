using Pathfinding.Shared.Exceptions;
using System.Reflection;

using static System.Reflection.BindingFlags;

namespace Pathfinding.Shared.Primitives;

/// <summary>
/// A base class of singleton classes
/// </summary>
/// <typeparam name="TInstance">A type, that will be representing 
/// an <see cref="Instance"/> property</typeparam>
/// <typeparam name="TInterface">An interface, that will be representing
/// an <see cref="Interface"/> property</typeparam>
public abstract class Singleton<TInstance, TInterface>
    where TInstance : Singleton<TInstance, TInterface>, TInterface
{
    private static readonly Lazy<TInstance> instance;

    /// <summary>
    /// A property, that contains an instance of type, 
    /// but returns only a main interface of the type
    /// </summary>
    /// <exception cref="SingletonException"> if there is 
    /// no private parameterless constructor</exception>
    public static TInterface Interface => Instance;

    /// <summary>
    /// A property, that contains an instance of type
    /// </summary>
    /// <exception cref="SingletonException"> if there is 
    /// no private parameterless constructor</exception>
    public static TInstance Instance => instance.Value;

    static Singleton()
    {
        instance = new(CreateInstance, true);
    }

    private static TInstance CreateInstance()
    {
        var instanceType = typeof(TInstance);
        var ctor = instanceType
            .GetConstructors(NonPublic | BindingFlags.Instance)
            .SingleOrDefault(c => c.GetParameters().Length == 0);
        return ctor != null
            ? (TInstance)ctor.Invoke([])
            : throw new SingletonException(instanceType);
    }
}