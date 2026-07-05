
public static class Sfx
{
    public static SfxLayer Instance { private get; set; }

    public static SfxOneShotPool OneShot
        => Instance.OneShotPool;

    public static void Clear()
        => Instance.Clear();
}
