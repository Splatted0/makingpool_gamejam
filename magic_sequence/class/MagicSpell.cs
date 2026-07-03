using System.Collections.Generic;

public partial class MagicSpell : MagicEffect
{
    [Export] public float MaxDistance;
    [Export] public float Range;
    private Dictionary<Type, Node> MagicNodesByType = new Dictionary<Type, Node>();
    
    public void SpawnEffect(){}
    
    public void MoveEffect(float fdelta)
    {

    }

    public void ArrivalEffect()
    {
        
    }
    
}