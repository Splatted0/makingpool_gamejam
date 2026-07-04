
public partial class MagicInfoLayer : Node
{
    [Export] private MagicInfo _magicInfo;

    private bool _isPressed;
    
    public override void _Ready()
    {
        _magicInfo.EnhanceButtonPressed += OnEnhancedButtonPressed;
    }
    
    public void ShowMagicInfo(Magic magic, Vector2 globalPosition)
    {
        _magicInfo.GlobalPosition = ClampToScreen(globalPosition);
        _magicInfo.Setup(magic);
        _magicInfo.Visible = true;
    }

    public void HideMagicInfo()
    {
        if (!_isPressed)
            _magicInfo.Visible = false;
    }

    public void MagicPressed(Magic magic, Vector2 globalPosition)
    {
        ShowMagicInfo(magic, globalPosition);
        _isPressed = true;
    }

    public void OutPressed()
    {
        if (!_isPressed) {return;}
        _isPressed = false;
        HideMagicInfo();
    }
    
    private Vector2 ClampToScreen(Vector2 position)
    {
        Vector2 screenSize = GetViewport().GetVisibleRect().Size * 0.7f;
        return new Vector2(
            Mathf.Min(position.X, screenSize.X),
            Mathf.Min(position.Y, screenSize.Y)
        );
    }

    private void OnEnhancedButtonPressed(Magic magic)
    {
        if (Blackboard.TrySpendGold(50))
            magic.MagicEffect.MagicEnhance();
    }
}
