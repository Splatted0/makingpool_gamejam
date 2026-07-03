
using System.Collections.Generic;

public partial class MagicNode : Node2D
{
    private MagicSpell MagicSpell;

    public void Setup(MagicSpell magicSpell, List<MagicPerk> magicPerks)
    {
        
    }

    public void OnSpawn()
    {
        if (_perkMap.TryGetValue(typeof(MagicPerkSpawn), out var spawnPerks))
        {
            foreach (MagicPerk perk in spawnPerks)
                ((MagicPerkSpawn)perk).SpawnEffect(MagicSpell);
        }
        MagicSpell.SpawnEffect(this);
    }

    public override void _Ready()
    {
        OnSpawn();
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
        if (_perkMap.TryGetValue(typeof(MagicPerkMove), out var movePerks))
        {
            foreach (MagicPerk perk in movePerks)
                ((MagicPerkMove)perk).MoveEffect(fdelta, MagicSpell);
        }

        MagicSpell.MoveEffect(this, fdelta);
    }

    private void OnArrival(float fdelta)
    {
        if (_perkMap.TryGetValue(typeof(MagicPerkArrival), out var arrivalPerks))
        {
            foreach (MagicPerk perk in arrivalPerks)
                ((MagicPerkArrival)perk).ArrivalEffect(MagicSpell);
        }
        MagicSpell.ArrivalEffect(this, fdelta);

    }
}