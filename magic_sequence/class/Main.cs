
using System.Runtime.InteropServices.ComTypes;

public partial class Main: Node
{
    [Export] public Node BattleWorld;

    public int Wave;
    public int Health;
    public int Gold;
    
    public Wand FirstWand;
    public Wand SecondWand;
    public Wand ThirdWand;
    
    public override void _Ready()
    {
        Blackboard.Main = this;
    }
}