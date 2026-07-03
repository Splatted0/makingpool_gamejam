public interface IModifier<T>
{
    T Modify(T data, WorldBlackboard blackboard);
}