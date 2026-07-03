
public partial class MainMenu: CanvasLayer
{
    [Signal] public delegate void GameStartPressedEventHandler();
    
    [Export] private BaseButton StartButton;

    public override void _Ready()
    {
        StartButton.Pressed += () => EmitSignalGameStartPressed();
    }
}