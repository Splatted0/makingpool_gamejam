
public static class Vfx
{
    public static VfxLayer Instance { private get; set; }
    
    public static VfxExplanationDamagePool ExplanationDamage
        => Instance.ExplanationDamagePool;
    public static VfxDeathParticlePool DeathParticle
        => Instance.DeathParticlePool;
    public static VfxExplanationHealPool ExplanationHeal
        => Instance.ExplanationHealPool;

    
    public static void Clear()
        => Instance.Clear();
}
