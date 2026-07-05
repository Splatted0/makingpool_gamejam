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
        Control[] skillUis = GetSkillUis();

        for (int i = 0; i < skillUis.Length; i++)
        {
            Wand wand = wands != null && i < wands.Length ? wands[i] : null;
            SetupSkillUi(skillUis[i], wand);
        }
    }

    private void SetupSkillUi(Control skillUi, Wand wand)
    {
        if (skillUi == null)
            return;

        skillUi.Visible = true;
        skillUi.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

        Control content = skillUi.GetNodeOrNull<Control>("MarginContainer/VBoxContainer");
        if (content != null)
            content.Visible = wand != null;

        skillUi.Modulate = wand != null ? Colors.White : new Color(1, 1, 1, 0);

        if (wand == null)
            return;

        RichTextLabel wandName = skillUi.GetNodeOrNull<RichTextLabel>("MarginContainer/VBoxContainer/WandName");
        if (wandName != null)
            wandName.Text = wand.WandName;

        Array<MagicPanel> panels = GetMagicPanels(skillUi);

        for (int i = 0; i < panels.Count; i++)
        {
            MagicPanel panel = panels[i];
            Magic magic = i < wand.Magics.Count ? wand.Magics[i] : null;
            bool hasMagic = magic != null;

            panel.Setup(magic);
            panel.Visible = true;
            panel.Modulate = hasMagic ? Colors.White : new Color(1, 1, 1, 0.35f);

            if (!hasMagic)
            {
                panel.Unselected();
                continue;
            }

            panel.Unselected();
        }
    }

    private static Array<MagicPanel> GetMagicPanels(Control skillUi)
    {
        var panels = new Array<MagicPanel>();
        Node container = skillUi.GetNodeOrNull("MarginContainer/VBoxContainer/HBoxContainer");

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
