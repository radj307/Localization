﻿using Localization;
using Localization.Json;
using Localization.Yaml;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace Testing.WPF
{
    internal static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            //JsonSingleTranslationLoader jstl = new();
            //var dict = jstl.Deserialize(ResourceHelper.GetManifestResourceString(ResourceHelper.ResourceNames.First(name => name.EndsWith("singlefile-test.loc.json"))));
            //var serial = jstl.Serialize(dict);

            Loc.Instance.AddTranslationLoader<YamlTranslationLoader>();
            Loc.Instance.AddTranslationLoader<JsonSingleTranslationLoader>();

            foreach (var resource in ResourceHelper.ResourceNames)
            {
                var loader = Loc.Instance.GetTranslationLoaderForPath(resource);
                if (loader == null) continue;
                Loc.Instance.LoadFromString(loader, ResourceHelper.GetManifestResourceString(resource));
            }
            Loc.Instance.CurrentLanguageName = "English";
            Loc.Instance.FallbackLanguageName = "lang4";

            var app = new App();
            return app.Run(new MainWindow());
        }
    }

    public class RemoveVowelsAndSometimesYConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = ((string)value)
                .Replace("a", "", StringComparison.OrdinalIgnoreCase)
                .Replace("e", "", StringComparison.OrdinalIgnoreCase)
                .Replace("i", "", StringComparison.OrdinalIgnoreCase)
                .Replace("o", "", StringComparison.OrdinalIgnoreCase)
                .Replace("u", "", StringComparison.OrdinalIgnoreCase);
            return (Random.Shared.Next(0, 2) == 0)
                ? s.Replace("y", "", StringComparison.OrdinalIgnoreCase)
                : s;
                
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
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

        private void Instance_MissingTranslationStringRequested(object sender, MissingTranslationRequestedEventArgs e)
        {
            if (e.Keys != null)
            {
                foreach (var key in e.Keys)
                {
                    MissingTranslations.Add(new(e.LanguageName, key));
                }
            }
            else MissingTranslations.Add(new(e.LanguageName, e.Key!));
        }

        public ObservableCollection<MissingTranslation> MissingTranslations { get; } = new();
    }
}
