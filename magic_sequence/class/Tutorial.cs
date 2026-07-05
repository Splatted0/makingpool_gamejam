using Godot;
using System.Threading.Tasks;

public partial class Tutorial : CanvasLayer
{
    private const string OverlayName = "RuntimeTutorialOverlay";

    public static async Task ShowCutscene(CanvasLayer host, params string[] imagePaths)
    {
        if (host == null)
            return;

        SetSceneContentVisible(host, false);
        Control overlay = GetOrCreateOverlay(host);
        TextureRect image = overlay.GetNode<TextureRect>("CutsceneImage");
        ColorRect dim = overlay.GetNode<ColorRect>("Dim");
        PanelContainer dialoguePanel = overlay.GetNode<PanelContainer>("DialoguePanel");

        overlay.Visible = true;
        image.Visible = true;
        dim.Visible = false;
        dialoguePanel.Visible = false;

        foreach (string imagePath in imagePaths)
        {
            image.Texture = GD.Load<Texture2D>(imagePath);
            await WaitForAdvance(host);
        }
    }

    public static async Task ShowDialogue(CanvasLayer host, string speaker, string text)
    {
        if (host == null)
            return;

        SetSceneContentVisible(host, false);
        Control overlay = GetOrCreateOverlay(host);
        TextureRect image = overlay.GetNode<TextureRect>("CutsceneImage");
        ColorRect dim = overlay.GetNode<ColorRect>("Dim");
        PanelContainer dialoguePanel = overlay.GetNode<PanelContainer>("DialoguePanel");
        RichTextLabel dialogueText = overlay.GetNode<RichTextLabel>("DialoguePanel/Margin/Text");

        overlay.Visible = true;
        image.Visible = false;
        dim.Visible = false;
        dialoguePanel.Visible = true;
        dialogueText.Text = $"[b]{speaker}[/b]\n{text}\n\n[color=#cfcfcf][font_size=14]Click / Space / Enter[/font_size][/color]";

        await WaitForAdvance(host);
    }

    public static void Hide(CanvasLayer host)
    {
        host?.GetNodeOrNull<Control>(OverlayName)?.Hide();
        SetSceneContentVisible(host, false);
    }

    private static Control GetOrCreateOverlay(CanvasLayer host)
    {
        Control existing = host.GetNodeOrNull<Control>(OverlayName);
        if (existing != null)
            return existing;

        var overlay = new Control { Name = OverlayName };
        overlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        overlay.MouseFilter = Control.MouseFilterEnum.Stop;
        host.AddChild(overlay);

        var image = new TextureRect { Name = "CutsceneImage" };
        image.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        image.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        image.StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered;
        overlay.AddChild(image);

        var dim = new ColorRect { Name = "Dim", Color = new Color(0f, 0f, 0f, 0.25f) };
        dim.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        overlay.AddChild(dim);

        var panel = new PanelContainer { Name = "DialoguePanel" };
        panel.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
        panel.OffsetLeft = 48f;
        panel.OffsetRight = -48f;
        panel.OffsetTop = -164f;
        panel.OffsetBottom = -24f;
        overlay.AddChild(panel);

        var margin = new MarginContainer { Name = "Margin" };
        margin.AddThemeConstantOverride("margin_left", 18);
        margin.AddThemeConstantOverride("margin_right", 18);
        margin.AddThemeConstantOverride("margin_top", 14);
        margin.AddThemeConstantOverride("margin_bottom", 14);
        panel.AddChild(margin);

        var text = new RichTextLabel { Name = "Text" };
        text.BbcodeEnabled = true;
        text.FitContent = true;
        text.ScrollActive = false;
        text.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        text.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        text.AddThemeFontSizeOverride("normal_font_size", 20);
        text.AddThemeFontSizeOverride("bold_font_size", 22);
        margin.AddChild(text);

        return overlay;
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

    private static async Task WaitForAdvance(Node node)
    {
        while (IsAdvancePressed())
            await node.ToSignal(node.GetTree(), SceneTree.SignalName.ProcessFrame);

        while (!IsAdvancePressed())
            await node.ToSignal(node.GetTree(), SceneTree.SignalName.ProcessFrame);

        while (IsAdvancePressed())
            await node.ToSignal(node.GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    private static bool IsAdvancePressed()
    {
        return Input.IsMouseButtonPressed(MouseButton.Left)
            || Input.IsKeyPressed(Key.Space)
            || Input.IsKeyPressed(Key.Enter);
    }
}
