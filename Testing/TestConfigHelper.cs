using System.Reflection;

namespace Testing
{
    static class TestConfigHelper
    {
        public static List<string> ResourceNames { get; } = Assembly.GetExecutingAssembly().GetManifestResourceNames().ToList();

        public static string? GetManifestResourceString(string resourceName)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream == null) return null;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}