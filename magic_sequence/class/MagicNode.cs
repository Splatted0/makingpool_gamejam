
public partial class MagicNode: Node2D
{
    private MagicSpell MagicSpell;

    public void Setup(MagicSpell magicSpell)
    {
        
    }

    public void OnSpawn()
    {
        MagicSpell.SpawnEffect();
    }

    public override void _PhysicsProcess(double delta)
    {
        float fdelta  = (float)delta;
        Move(fdelta);
        OnMove(fdelta);
    }
    
    private void Move(float fdelta) {}

    private void OnMove(float fdelta)
    {
        MagicSpell.MoveEffect(fdelta);
    }
}