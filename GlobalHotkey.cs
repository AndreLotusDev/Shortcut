using System.Runtime.InteropServices;

namespace Shotcut;

public static class GlobalHotkey
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public const int WM_HOTKEY = 0x0312;

    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_NOREPEAT = 0x4000;

    public static bool Register(IntPtr handle, int id, Keys modifiers, Keys key)
    {
        uint mod = MOD_NOREPEAT;
        if (modifiers.HasFlag(Keys.Control)) mod |= MOD_CONTROL;
        if (modifiers.HasFlag(Keys.Alt)) mod |= MOD_ALT;
        if (modifiers.HasFlag(Keys.Shift)) mod |= MOD_SHIFT;

        return RegisterHotKey(handle, id, mod, (uint)key);
    }

    public static bool Unregister(IntPtr handle, int id)
    {
        return UnregisterHotKey(handle, id);
    }
}
