public partial class SfxOneShot : AudioStreamPlayer, ISfxNode<SfxOneShotData>
{
    private Callable _returnAction;

    public void InjectReturnAction(Callable returnAction)
    {
        _returnAction = returnAction;
        Connect(SignalName.Finished, _returnAction);
    }

    public void Initialize(SfxOneShotData data)
    {
        Stream = data.Stream;
        VolumeDb = data.VolumeDb;
        PitchScale = data.PitchScaleMax > 0f
            ? (float)GD.RandRange(data.PitchScaleMin > 0f ? data.PitchScaleMin : 1f, data.PitchScaleMax)
            : 1f;
    }

    public void Play()
    {
        if (Stream == null)
        {
            _returnAction.Call();   // Finished가 안 터지므로 직접 반환(안 하면 null 스트림 던질 때마다 풀이 샘)
            return;
        }

        base.Play();
    }

    public new void Stop()
    {
        base.Stop();
        _returnAction.Call();
    }
}
