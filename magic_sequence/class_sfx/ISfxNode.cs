public interface ISfxNode<TData> where TData : ISfxData
{
    public void InjectReturnAction(Callable returnAction);
    public void Initialize(TData data);
    public void Play();
    public void Stop();
}
