namespace PyClickerRecorder
{
    internal static class EmbeddedResourceName
    {
        public static string? FindIcon(IEnumerable<string> resourceNames)
        {
            return resourceNames.FirstOrDefault(name =>
                name.EndsWith(".icon.ico", StringComparison.OrdinalIgnoreCase));
        }
    }
}
