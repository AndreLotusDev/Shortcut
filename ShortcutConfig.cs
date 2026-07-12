namespace Shotcut;

public class ShortcutConfig
{
    public int Id { get; set; }
    public Keys Key { get; set; }
    public Keys Modifiers { get; set; }
    public string DestinationPath { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;

    public string HotkeyDisplay
    {
        get
        {
            var parts = new List<string>();
            if (Modifiers.HasFlag(Keys.Control)) parts.Add("Ctrl");
            if (Modifiers.HasFlag(Keys.Alt)) parts.Add("Alt");
            if (Modifiers.HasFlag(Keys.Shift)) parts.Add("Shift");
            parts.Add(Key.ToString());
            return string.Join(" + ", parts);
        }
    }
}

public class ShortcutGroup
{
    public string Name { get; set; } = string.Empty;
    public List<ShortcutConfig> Shortcuts { get; set; } = new();
}
