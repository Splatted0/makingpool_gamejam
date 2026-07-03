
public partial class MagicInfoLayer : Node
{
    [Export] private MagicInfo _magicInfo;

    public void ShowInfo(Magic magic, Vector2 globalPosition)
    {
        _magicInfo.GlobalPosition = globalPosition;
        _magicInfo.Setup(magic);
        _magicInfo.Visible = true;
    }

    public void HideInfo()
    {
        _magicInfo.Visible = false;
    }
}
