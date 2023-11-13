using Localization;
using Localization.Json;
using Localization.Yaml;
using System.Diagnostics;

namespace Testing
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            var yamlLoader = Loc.Instance.AddTranslationLoader<YamlTranslationLoader>();
            var jsonLoader = Loc.Instance.AddTranslationLoader<JsonTranslationLoader>();

            foreach (var embeddedResourceName in TestConfigHelper.ResourceNames)
            {
                if (yamlLoader.CanLoadFile(embeddedResourceName))
                {
                    var dict = yamlLoader.Deserialize(TestConfigHelper.GetManifestResourceString(embeddedResourceName)!);
                    if (dict == null) continue;
                    Loc.Instance.AddLanguageDictionaries(dict);
                }
                else if (jsonLoader.CanLoadFile(embeddedResourceName))
                {
                    var dict = jsonLoader.Deserialize(TestConfigHelper.GetManifestResourceString(embeddedResourceName)!);
                    if (dict == null) continue;
                    Loc.Instance.AddLanguageDictionaries(dict);
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

                Console.Error.WriteLine($" Language \"{e.LanguageName}\" is missing translation \"{e.StringPath}\"");
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
            var serialized = jsonLoader.Serialize(Loc.Instance.Languages[serializeLanguage].ToDictionary(serializeLanguage));
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