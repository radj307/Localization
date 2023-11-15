using Localization;
using Localization.Interfaces;
using Localization.Json;
using Localization.Xml;
using Localization.Yaml;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Testing
{
    internal static class Program
    {
        public static string PartialFormat(string format, params (int, object)[] formatArgs)
        {
            var matches = Regex.Matches(format, @"{(\d+)(,-{0,1}\d+){0,1}(:[\w!@#$%^&*()_+-=\/\\|;:'"",.<>?`~]+){0,1}}");

            if (matches.Count == 0)
                return format;

            int providedCount = formatArgs.Length;

            bool TryGetArgForIndex(int index, out object arg)
            {
                for (int i = 0; i < providedCount; ++i)
                {
                    var (k, v) = formatArgs[i];
                    if (k == index)
                    {
                        arg = v;
                        return true;
                    }
                }
                arg = null!;
                return false;
            }
            List<object> args = new();

            foreach (var m in (IEnumerable<Match>)matches)
            {
                var argIndex = int.Parse(m.Groups[1].Value);

                if (TryGetArgForIndex(argIndex, out var arg))
                    args.Add(string.Format(m.Value.Replace(m.Groups[1].Value, "0"), arg));
                else args.Add(m.Value);
            }

            return string.Format(format, args.ToArray());
        }
        class TESTLOADER : ITranslationLoader
        {
            public string[] SupportedFileExtensions { get; } = new string[] { ".xml", ".json", ".yml", ".yaml" };

            public Dictionary<string, Dictionary<string, string>>? Deserialize(string serializedData) => throw new NotImplementedException();
            public string Serialize(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> languageDictionaries) => throw new NotImplementedException();
        }
        static void Main(string[] args)
        {
            var jsonLoader = Loc.Instance.AddTranslationLoader<JsonTranslationLoader>();
            var yamlLoader = Loc.Instance.AddTranslationLoader<YamlTranslationLoader>();
            var xmlLoader = Loc.Instance.AddTranslationLoader<XmlTranslationLoader>();

            var dict = xmlLoader.Deserialize(TestConfigHelper.GetManifestResourceString(TestConfigHelper.ResourceNames.First(name => name.EndsWith(".xml"))));
            var serial = xmlLoader.Serialize(dict);

            foreach (var embeddedResourceName in TestConfigHelper.ResourceNames)
            {
                if (Loc.Instance.GetTranslationLoaderForFile(embeddedResourceName) is ITranslationLoader loader)
                {
                    Loc.Instance.LoadFromString(loader, TestConfigHelper.GetManifestResourceString(embeddedResourceName)!);
                }
                else throw new InvalidOperationException($"Resource file doesn't have any associated loaders: \"{embeddedResourceName}\"");
            }

            // set the current language
            Loc.Instance.CurrentLanguageName = Loc.Instance.AvailableLanguageNames.First();

            // set a handler that prints an error when a missing translation is requested
            Loc.Instance.MissingTranslationStringRequested += (s, e) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.Write("[ERROR]");
                Console.ForegroundColor = ConsoleColor.White;

                Console.Error.WriteLine($" Language \"{e.LanguageName}\" is missing translation \"{e.Keys}\"");
            };

            Console.WriteLine(Loc.Tr("VolumeControl.MainWindow.Settings.Language.Header"));

            Console.WriteLine(Loc.Tr("VolumeControl.MainWindow.%$AE$RWERA")); //< [ERROR] Language "German (DE/DE)" is missing translation "VolumeControl.MainWindow.%$AE$RWERA"

            // print out a test key in all languages
            var margin = 2 + Loc.Instance.AvailableLanguageNames.Select(s => s.Length).Max();
            foreach (var langName in Loc.Instance.AvailableLanguageNames)
            {
                Console.WriteLine($"{langName}: {new string(' ', margin - langName.Length)}{Loc.Tr("VolumeControl.MainWindow.Settings.Language.Header", "(not found)", langName)}");
            }

            // serialize the english translation
            string serializeLanguage = "English (US/CA)";
            var serialized = jsonLoader.Serialize(Loc.Instance.Languages[serializeLanguage].ToLanguageDictionaries(serializeLanguage));
            // write the serialized translation to a file
            var path = Path.GetFullPath("test.loc.json");
            File.WriteAllText(path, serialized);
            // write a console message
            Console.WriteLine($"Wrote serialized language \"{serializeLanguage}\" to \"{path}\"");
            // open the file
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true })?.Dispose();
        }
    }
}