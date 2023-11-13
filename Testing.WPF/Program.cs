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
}
