
public interface IVfxNode<TData> where TData : IVfxData
{
    public void InjectReturnAction(Callable returnAction);
    public void Initialize(TData data);
    public void Play();
    public void Stop();
}
