using System.Collections.Generic;

[Icon("res://dev/editor_icon/editor_icon_1_4.tres")]
public partial class SfxPool<TNode, TData>: Node, ISfx
    where TNode : Node, ISfxNode<TData>
    where TData : ISfxData
{
    [Export] public PackedScene PackedSfx { get; private set; }
    [Export] public int PoolSize { get; private set; } = 5;
    private readonly Stack<TNode> _poolStack = new();

    [ExportCategory("Debug Settings")]
    [Export] private bool _printLog = false;

    public override void _Ready()
    {
        for (int i = 0; i < PoolSize; i++)
        {
            TNode sfxNode = PackedSfx.Instantiate<TNode>();
            sfxNode.InjectReturnAction(
                Callable.From(() => Collect(sfxNode))
            );
            sfxNode.SetProcess(false);
            _poolStack.Push(sfxNode);

            AddChild(sfxNode);
        }
    }

    public void Throw(TData data)
    {
        TNode sfxNode;

        if (_poolStack.Count > 0)
        {
            if (_printLog) GD.Print("SFX 발현");
            sfxNode = _poolStack.Pop();
            sfxNode.SetProcess(true);
            sfxNode.Initialize(data);
        }
        else
        {
            if (_printLog) GD.Print("SFX 부족");
            return;
        }

        sfxNode.Play();
    }

    public void Collect(TNode sfxNode)
    {
        if (_printLog) GD.Print("SFX 수집");
        sfxNode.SetProcess(false);
        _poolStack.Push(sfxNode);
    }

    public void Clear()
    {
        foreach (Node child in GetChildren())
        {
            if (child is TNode sfx)
            {
                Collect(sfx);
            }
        }
    }
}
