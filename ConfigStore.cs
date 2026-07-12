namespace Shotcut;

public static class ConfigStore
{
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Shotcut");

    private static readonly string ConfigPath = Path.Combine(ConfigDir, "shortcuts.txt");

    public static List<ShortcutGroup> Load()
    {
        if (!File.Exists(ConfigPath))
            return new List<ShortcutGroup>();

        var groups = new List<ShortcutGroup>();
        var lines = File.ReadAllLines(ConfigPath);

        ShortcutGroup? currentGroup = null;
        ShortcutConfig? currentShortcut = null;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            if (trimmed.StartsWith("[Group:") && trimmed.EndsWith("]"))
            {
                if (currentShortcut != null && currentGroup != null)
                    currentGroup.Shortcuts.Add(currentShortcut);
                currentShortcut = null;

                currentGroup = new ShortcutGroup
                {
                    Name = trimmed[7..^1].Trim()
                };
                groups.Add(currentGroup);
                continue;
            }

            if (trimmed == "---")
            {
                if (currentShortcut != null && currentGroup != null)
                    currentGroup.Shortcuts.Add(currentShortcut);
                currentShortcut = new ShortcutConfig();
                continue;
            }

            if (currentShortcut == null || !trimmed.Contains('='))
                continue;

            var separatorIndex = trimmed.IndexOf('=');
            var key = trimmed[..separatorIndex].Trim();
            var value = trimmed[(separatorIndex + 1)..].Trim();

            switch (key)
            {
                case "Id":
                    currentShortcut.Id = int.Parse(value);
                    break;
                case "Label":
                    currentShortcut.Label = value;
                    break;
                case "Key":
                    currentShortcut.Key = Enum.Parse<Keys>(value);
                    break;
                case "Modifiers":
                    currentShortcut.Modifiers = Enum.Parse<Keys>(value);
                    break;
                case "DestinationPath":
                    currentShortcut.DestinationPath = value;
                    break;
                case "Enabled":
                    currentShortcut.Enabled = bool.Parse(value);
                    break;
            }
        }

        if (currentShortcut != null && currentGroup != null)
            currentGroup.Shortcuts.Add(currentShortcut);

        return groups;
    }

    public static void Save(List<ShortcutGroup> groups)
    {
        Directory.CreateDirectory(ConfigDir);
        using var writer = new StreamWriter(ConfigPath);
        foreach (var group in groups)
        {
            writer.WriteLine($"[Group: {group.Name}]");
            foreach (var sc in group.Shortcuts)
            {
                writer.WriteLine("---");
                writer.WriteLine($"Id = {sc.Id}");
                writer.WriteLine($"Label = {sc.Label}");
                writer.WriteLine($"Key = {sc.Key}");
                writer.WriteLine($"Modifiers = {sc.Modifiers}");
                writer.WriteLine($"DestinationPath = {sc.DestinationPath}");
                writer.WriteLine($"Enabled = {sc.Enabled}");
            }
            writer.WriteLine();
        }
    }
}
