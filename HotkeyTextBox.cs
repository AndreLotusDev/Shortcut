namespace Shotcut;

public class HotkeyTextBox : TextBox
{
    public Keys HotKey { get; private set; } = Keys.None;
    public Keys HotKeyModifiers { get; private set; } = Keys.None;

    public HotkeyTextBox()
    {
        ReadOnly = true;
        BackColor = SystemColors.Window;
        Text = "Pressione um atalho...";
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        e.SuppressKeyPress = true;
        e.Handled = true;

        var modifiers = Keys.None;
        if (e.Control) modifiers |= Keys.Control;
        if (e.Alt) modifiers |= Keys.Alt;
        if (e.Shift) modifiers |= Keys.Shift;

        var key = e.KeyCode;
        if (key == Keys.ControlKey || key == Keys.ShiftKey || key == Keys.Menu)
            return;

        HotKey = key;
        HotKeyModifiers = modifiers;

        var parts = new List<string>();
        if (modifiers.HasFlag(Keys.Control)) parts.Add("Ctrl");
        if (modifiers.HasFlag(Keys.Alt)) parts.Add("Alt");
        if (modifiers.HasFlag(Keys.Shift)) parts.Add("Shift");
        parts.Add(key.ToString());
        Text = string.Join(" + ", parts);
    }

    public void SetHotkey(Keys modifiers, Keys key)
    {
        HotKey = key;
        HotKeyModifiers = modifiers;

        if (key == Keys.None)
        {
            Text = "Pressione um atalho...";
            return;
        }

        var parts = new List<string>();
        if (modifiers.HasFlag(Keys.Control)) parts.Add("Ctrl");
        if (modifiers.HasFlag(Keys.Alt)) parts.Add("Alt");
        if (modifiers.HasFlag(Keys.Shift)) parts.Add("Shift");
        parts.Add(key.ToString());
        Text = string.Join(" + ", parts);
    }
}
