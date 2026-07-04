using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Tutorial : CanvasLayer
{
    private const string OverlayName = "RuntimeTutorialOverlay";

    public readonly record struct Step(string Speaker, string Text, string IconPath);

    public static async Task PlaySteps(CanvasLayer host, IReadOnlyList<Step> steps)
    {
        if (host == null)
            return;

        Control overlay = GetOrCreateOverlay(host);
        TextureRect portrait = overlay.GetNode<TextureRect>("Panel/Margin/Rows/Portrait");
        RichTextLabel textLabel = overlay.GetNode<RichTextLabel>("Panel/Margin/Rows/Text");

        overlay.Visible = true;

        foreach (Step step in steps)
        {
            portrait.Texture = string.IsNullOrEmpty(step.IconPath) ? null : GD.Load<Texture2D>(step.IconPath);
            textLabel.Text = $"[b]{step.Speaker}[/b]\n{step.Text}\n\n[color=#cfcfcf][font_size=14]클릭 / Space / Enter[/font_size][/color]";
            await WaitForAdvance(host);
        }
    }

    public static async Task ShowMessage(CanvasLayer host, string speaker, string text, string iconPath = "")
    {
        await PlaySteps(host, new[] { new Step(speaker, text, iconPath) });
    }

    public static void Hide(CanvasLayer host)
    {
        host?.GetNodeOrNull<Control>(OverlayName)?.Hide();
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

        var dim = new ColorRect { Name = "Dim", Color = new Color(0f, 0f, 0f, 0.35f) };
        dim.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        overlay.AddChild(dim);

        var panel = new PanelContainer { Name = "Panel" };
        panel.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
        panel.OffsetLeft = 48f;
        panel.OffsetRight = -48f;
        panel.OffsetTop = -188f;
        panel.OffsetBottom = -24f;
        overlay.AddChild(panel);

        var margin = new MarginContainer { Name = "Margin" };
        margin.AddThemeConstantOverride("margin_left", 18);
        margin.AddThemeConstantOverride("margin_right", 18);
        margin.AddThemeConstantOverride("margin_top", 14);
        margin.AddThemeConstantOverride("margin_bottom", 14);
        panel.AddChild(margin);

        var rows = new HBoxContainer { Name = "Rows" };
        rows.AddThemeConstantOverride("separation", 18);
        margin.AddChild(rows);

        var portrait = new TextureRect { Name = "Portrait" };
        portrait.CustomMinimumSize = new Vector2(96f, 96f);
        portrait.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
        portrait.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        rows.AddChild(portrait);

        var text = new RichTextLabel { Name = "Text" };
        text.BbcodeEnabled = true;
        text.FitContent = true;
        text.ScrollActive = false;
        text.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        text.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        text.AddThemeFontSizeOverride("normal_font_size", 20);
        text.AddThemeFontSizeOverride("bold_font_size", 22);
        rows.AddChild(text);

        return overlay;
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
