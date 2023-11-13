using Localization;
using Localization.Json;
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
                {
                    args.Add(string.Format(m.Value.Replace(m.Groups[1].Value, "0"), arg));
                }
                else args.Add(m.Value);
            }

            return string.Format(format, args.ToArray());
        }
        static void Main(string[] args)
        {
            string format = "{0}{1}{2}";
            string arg0 = "Hello";
            string arg1 = " ";
            string arg2 = "World!";
            DebugProfiler2.WithCount(1000000)
                .Profile(out var elapsed1, () =>
                {
                    string.Format(format, arg0, arg1, arg2);
                })
                .Profile(out var elapsed2, () =>
                {
                    PartialFormat(format, (0, arg0));
                })
                .Profile(out var elapsed3, () =>
                {
                    PartialFormat(format, (2, arg2));
                })
                ;


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