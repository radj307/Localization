using System.IO;
using System.Reflection;

namespace Testing.WPF
{
    internal static class ResourceHelper
    {
        public static readonly string[] ResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();

        public static string? GetManifestResourceString(string resourceName)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream == null) return null;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
