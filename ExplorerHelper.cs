namespace Shotcut;

public static class ExplorerHelper
{
    private static readonly string[] ImageExtensions =
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff", ".tif", ".svg", ".ico", ".heic", ".avif"
    };

    public static List<string> GetSelectedFiles()
    {
        var selected = new List<string>();

        try
        {
            var shellType = Type.GetTypeFromProgID("Shell.Application");
            if (shellType == null) return selected;

            dynamic shell = Activator.CreateInstance(shellType)!;
            var windows = shell.Windows();

            for (int i = 0; i < windows.Count; i++)
            {
                dynamic window = windows.Item(i);
                if (window == null) continue;

                try
                {
                    string name = Path.GetFileNameWithoutExtension((string)window.FullName);
                    if (!name.Equals("explorer", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var doc = window.Document;
                    if (doc == null) continue;

                    var items = doc.SelectedItems();
                    for (int j = 0; j < items.Count; j++)
                    {
                        string path = items.Item(j).Path;
                        if (!string.IsNullOrEmpty(path))
                            selected.Add(path);
                    }
                }
                catch
                {
                    continue;
                }
            }
        }
        catch
        {
        }

        return selected;
    }

    public static List<string> FilterImages(List<string> files)
    {
        return files.Where(f =>
            ImageExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .ToList();
    }
}
