public readonly struct SfxOneShotData : ISfxData
{
    public required AudioStream Stream { get; init; }
    public float VolumeDb { get; init; }
    public float PitchScaleMin { get; init; }   // 0/미지정이면 피치 지터 없이 1.0 고정
    public float PitchScaleMax { get; init; }
}
