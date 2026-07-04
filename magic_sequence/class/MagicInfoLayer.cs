
public partial class MagicInfoLayer : Node
{
    [Export] private MagicInfo _magicInfo;

    public void ShowInfo(Magic magic, Vector2 globalPosition)
    {
        _magicInfo.GlobalPosition = ClampToScreen(globalPosition);
        _magicInfo.Setup(magic);
        _magicInfo.Visible = true;
    }

    public void HideInfo()
    {
        _magicInfo.Visible = false;
    }

    private Vector2 ClampToScreen(Vector2 position)
    {
        Vector2 screenSize = GetViewport().GetVisibleRect().Size * 0.7f;
        return new Vector2(
            Mathf.Min(position.X, screenSize.X),
            Mathf.Min(position.Y, screenSize.Y)
        );
    }
}
