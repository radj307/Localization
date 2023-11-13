using Newtonsoft.Json;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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

        public static string? CreateYamlFromJson(string resourceName)
        {
            var content = GetManifestResourceString(resourceName);
            if (content == null) return null;

            var yaml = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()
                .Deserialize(content);
            var jsonSerializer = new JsonSerializer();
            using var writer = new StringWriter();
            jsonSerializer.Serialize(writer, yaml);

            return writer.ToString();
        }
    }
}