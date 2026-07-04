using System.Collections.Generic;

[Icon("res://dev/editor_icon/editor_icon_1_4.tres")]
public partial class VfxPool<TNode, TData>: Node2D, IVfx
    where TNode : Node2D, IVfxNode<TData> 
    where TData : IVfxData
{
    [Export] public PackedScene PackedVfx { get; private set; }
    [Export] public int PoolSize { get; private set; } = 5;
    private readonly Stack<TNode> _poolStack = new();
    
    [ExportCategory("Debug Settings")]
    [Export] private bool _printLog = false;
    
    public override void _Ready()
    {
        for (int i = 0; i < PoolSize; i++)
        {
            TNode vfxNode = PackedVfx.Instantiate<TNode>();
            vfxNode.InjectReturnAction(
                Callable.From(() => Collect(vfxNode))
            );
            vfxNode.Hide();
            vfxNode.SetProcess(false);
            _poolStack.Push(vfxNode);
            
            AddChild(vfxNode);
        }
    }
    
    public void Throw(TData data)
    {
        TNode vfxNode;

        if (_poolStack.Count > 0)
        {
            if (_printLog) GD.Print("VFX 발현");
            vfxNode = _poolStack.Pop();
            vfxNode.SetProcess(true);
            vfxNode.Initialize(data);
            vfxNode.Show();
        }
        else
        {
            if (_printLog) GD.Print("VFX 부족");
            return;
        }
        
        vfxNode.Play();
    }
    
    public void Collect(TNode vfxNode)
    {
        if (_printLog) GD.Print("VFX 수집");
        vfxNode.Hide();
        vfxNode.SetProcess(false);
        _poolStack.Push(vfxNode);
    }

    public void Clear()
    {
        foreach (Node child in GetChildren())
        {
            if (child is TNode vfx)
            {
                Collect(vfx);
            }
        }
    }
}
