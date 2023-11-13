using Localization;
using Localization.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Testing.WPF
{
    internal static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            Loc.Instance.AddTranslationLoader<JsonTranslationLoader>();

            Loc.Instance.LoadFromString(new JsonTranslationLoader(), ResourceHelper.GetManifestResourceString(ResourceHelper.ResourceNames.First(name => name.EndsWith("test.loc.json")))!);
            Loc.Instance.CurrentLanguageName = "lang1";
            Loc.Instance.FallbackLanguageName = "lang4";

            var app = new App();
            return app.Run(new MainWindow());
        }
    }
    public class TestObject { }
    public class Container
    {
        public ObservableCollection<TestObject> Items { get; } = new() { new(), new(), new(), new() };
    }
    public class MissingTranslation
    {
        public MissingTranslation(string langName, string path)
        {
            LanguageName = langName;
            StringPath = path;
            ID = id++;
        }

        private static int id = 0;

        public int ID { get; }
        public string LanguageName { get; }
        public string StringPath { get; }
    }
    public class MissingTranslationsCollector
    {
        public MissingTranslationsCollector()
        {
            Loc.Instance.MissingTranslationStringRequested += this.Instance_MissingTranslationStringRequested;
        }

        private void Instance_MissingTranslationStringRequested(object sender, Localization.Events.MissingTranslationStringRequestedEventArgs e)
        {
            MissingTranslations.Add(new(e.LanguageName, e.StringPath));
        }

        public ObservableCollection<MissingTranslation> MissingTranslations { get; } = new();
    }
}
