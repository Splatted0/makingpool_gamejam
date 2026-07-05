using System.Threading;
using Godot;
using System.Threading.Tasks;

public partial class Tutorial : CanvasLayer
{
    [Signal] public delegate void AcceptedEventHandler();
    private const string OverlayName = "RuntimeTutorialOverlay";

    [Export] public Control Overlay;
    [Export] public TextureRect CutSceneImage;
    [Export] public Panel DialoguePanel;
    [Export] public RichTextLabel DialogueText;
    [Export] public Control TextPanel;
    [Export] public RichTextLabel TextLabel;

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent &&  mouseEvent.Pressed)
        {
            EmitSignalAccepted();
        }
    }

    public async Task ShowCutscene(params string[] imagePaths)
    {

        Overlay.Modulate = Colors.White;
        DialoguePanel.Visible = false;
        TextPanel.Visible = false;
        Overlay.Visible = true;
        CutSceneImage.Visible = true;

        foreach (string imagePath in imagePaths)
        {
            CutSceneImage.Texture = GD.Load<Texture2D>(imagePath);
            Tween fadeTween = CreateTween().SetTrans(Tween.TransitionType.Sine);
            fadeTween.TweenProperty(Overlay, "modulate", Colors.White, 0.5).From(Colors.Black);
            await ToSignal(this, SignalName.Accepted);
        }
    }

    public async Task ShowDialogue(string speaker, string text)
    {
        Overlay.Visible = true;
        DialoguePanel.Visible = true;
        TextPanel.Visible = false;
        
        Tween fadeTween = CreateTween().SetTrans(Tween.TransitionType.Sine);
        fadeTween.TweenProperty(DialoguePanel, "modulate", Colors.White, 0.5).From(new Color(1, 1,1, 0));
        DialogueText.Text = $"[b]{speaker}[/b]\n\n{text}";
        await ToSignal(this, SignalName.Accepted);
        fadeTween = CreateTween().SetTrans(Tween.TransitionType.Sine);
        fadeTween.TweenProperty(Overlay, "modulate", Colors.Black, 0.5).From(Colors.White);
        await ToSignal(fadeTween, Tween.SignalName.Finished);
    }

    public async Task ShowText(string speaker, string text)
    {
        Overlay.Modulate = Colors.White;
        Overlay.Visible = true;
        DialoguePanel.Visible = false;
        CutSceneImage.Visible = false;
        TextPanel.Visible = true;
        TextLabel.Text = $"[b]{speaker}[/b]\n\n{text}";
        
        await ToSignal(this, SignalName.Accepted);
    }
    
    public static void Hide(CanvasLayer host)
    {
        host?.GetNodeOrNull<Control>(OverlayName)?.Hide();
        SetSceneContentVisible(host, false);
    }

    private static void SetSceneContentVisible(CanvasLayer host, bool visible)
    {
        if (host == null)
            return;

        foreach (Node child in host.GetChildren())
        {
            if (child.Name == OverlayName)
                continue;

            if (child is CanvasItem canvasItem)
                canvasItem.Visible = visible;
        }
    }

}
