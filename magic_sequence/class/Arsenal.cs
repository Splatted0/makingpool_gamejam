using Godot.Collections;

public partial class Arsenal : PanelContainer
{
    [Export] public WandManager WandManager { get; set; }
    [Export] public Control SkillUI1 { get; set; }
    [Export] public Control SkillUI2 { get; set; }
    [Export] public Control SkillUI3 { get; set; }

    public override void _Ready()
    {
        ResolveReferences();
        Refresh();
    }

    public void Refresh()
    {
        ResolveReferences();

        Wand[] wands = Blackboard.Wands;
        WandNode[] wandNodes = WandManager?.GetWandNodes() ?? System.Array.Empty<WandNode>();
        Control[] skillUis = GetSkillUis();

        for (int i = 0; i < skillUis.Length; i++)
        {
            Wand wand = wands != null && i < wands.Length ? wands[i] : null;
            WandNode wandNode = i < wandNodes.Length ? wandNodes[i] : null;
            SetupSkillUi(skillUis[i], wand, wandNode);
        }
    }

    private void SetupSkillUi(Control skillUi, Wand wand, WandNode wandNode)
    {
        if (skillUi == null)
            return;

        skillUi.Visible = true;
        skillUi.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

        Control content = skillUi.GetNodeOrNull<Control>("VBoxContainer");
        if (content != null)
            content.Visible = wand != null;

        skillUi.Modulate = wand != null ? Colors.White : new Color(1, 1, 1, 0);

        if (wand == null)
            return;

        RichTextLabel wandName = skillUi.GetNodeOrNull<RichTextLabel>("VBoxContainer/WandName");
        if (wandName != null)
            wandName.Text = wand.WandName;

        Array<MagicPanel> panels = GetMagicPanels(skillUi);
        int chargedIndex = wandNode?.GetChargedMagicIndex() ?? -1;
        Magic chargedMagic = chargedIndex >= 0 && chargedIndex < wand.Magics.Count ? wand.Magics[chargedIndex] : null;

        Label loadedIndicator = skillUi.GetNodeOrNull<Label>("VBoxContainer/LoadedIndicator");
        if (loadedIndicator != null)
            loadedIndicator.Text = chargedMagic != null ? $"Loaded: {chargedMagic.Name}" : "Loaded: -";

        for (int i = 0; i < panels.Count; i++)
        {
            MagicPanel panel = panels[i];
            Magic magic = i < wand.Magics.Count ? wand.Magics[i] : null;
            bool hasMagic = magic != null;

            panel.Visible = hasMagic;
            panel.Setup(magic);

            if (!hasMagic)
            {
                panel.Unselected();
                continue;
            }

            if (i == chargedIndex)
                panel.Selected();
            else
                panel.Unselected();
        }
    }

    private static Array<MagicPanel> GetMagicPanels(Control skillUi)
    {
        var panels = new Array<MagicPanel>();
        Node container = skillUi.GetNodeOrNull("VBoxContainer/HBoxContainer");

        if (container == null)
            return panels;

        foreach (Node child in container.GetChildren())
        {
            if (child is MagicPanel panel)
                panels.Add(panel);
        }

        return panels;
    }

    private Control[] GetSkillUis() => new[] { SkillUI1, SkillUI2, SkillUI3 };

    private void ResolveReferences()
    {
        WandManager ??= GetNodeOrNull<WandManager>("../../BattleCenter/WandManager");
        SkillUI1 ??= GetNodeOrNull<Control>("HBoxContainer/SkillUI");
        SkillUI2 ??= GetNodeOrNull<Control>("HBoxContainer/SkillUI2");
        SkillUI3 ??= GetNodeOrNull<Control>("HBoxContainer/SkillUI3");
    }
}
