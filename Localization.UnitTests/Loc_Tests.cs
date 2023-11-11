using Localization.Events;
using Localization.Json;
using System.Diagnostics;

namespace Localization.UnitTests
{
    public class Loc_Tests
    {
        #region ClearLanguages
        [Fact]
        public void ClearLanguages_ClearsAllTranslations()
        {
            Loc.Instance.AddLanguageDictionaries(new Dictionary<string, LanguageDictionary>
            {
                { "en", new() { { "A.B.C", "asdf" } } },
                { "fr", new() { { "A.B.C", "jkl;" } } },
                { "de", new() { { "A.B.C", "1234" } } },
            });

            Loc.Instance.ClearLanguages();

            Assert.Empty(Loc.Instance.Languages);
        }
        [Fact]
        public void ClearLanguages_ClearsCurrentLanguageNameWhenWanted()
        {
            Loc.Instance.CurrentLanguageName = "asdf";

            Loc.Instance.ClearLanguages(true);

            Assert.Empty(Loc.Instance.CurrentLanguageName);
        }
        [Fact]
        public void ClearLanguages_DoesntClearCurrentLanguageNameWhenUnwanted()
        {
            Loc.Instance.CurrentLanguageName = "asdf";

            Loc.Instance.ClearLanguages(false);

            Assert.NotEmpty(Loc.Instance.CurrentLanguageName);
        }
        [Fact]
        public void ClearLanguages_ClearsFallbackLanguageNameWhenWanted()
        {
            Loc.Instance.FallbackLanguageName = "asdf";

            Loc.Instance.ClearLanguages(false, true);

            Assert.Null(Loc.Instance.FallbackLanguageName);
        }
        [Fact]
        public void ClearLanguages_DoesntClearFallbackLanguageNameWhenUnwanted()
        {
            Loc.Instance.FallbackLanguageName = "asdf";

            Loc.Instance.ClearLanguages(false, false);

            Assert.NotNull(Loc.Instance.FallbackLanguageName);
        }
        #endregion ClearLanguages

        #region AddLanguageDictionary
        [Fact]
        public void AddLanguageDictionary_AddsToAvailableLanguageNames()
        {
            Loc.Instance.AddLanguageDictionary("en", new Dictionary<string, string>());

            Assert.Contains("en", Loc.Instance.AvailableLanguageNames);

            Loc.Instance.ClearLanguages();
        }
        [Fact]
        public void AddLanguageDictionary_OverwritesExistingWhenWanted()
        {
            Loc.Instance.AddLanguageDictionary("en", new Dictionary<string, string>
            {
                { "My.Key", "" }
            });
            Loc.Instance.AddLanguageDictionary("en", new Dictionary<string, string>
            {
                { "My.Key", "ASDF" }
            }, overwriteExisting: true);

            Assert.Equal("ASDF", Loc.Instance.Languages["en"]["My.Key"]);

            Loc.Instance.ClearLanguages();
        }
        [Fact]
        public void AddLanguageDictionary_DoesntOverwriteExistingWhenUnwanted()
        {
            Loc.Instance.AddLanguageDictionary("en", new Dictionary<string, string>
            {
                { "My.Key", "" }
            });
            Loc.Instance.AddLanguageDictionary("en", new Dictionary<string, string>
            {
                { "My.Key", "ASDF" }
            }, overwriteExisting: false);

            Assert.Equal("", Loc.Instance.Languages["en"]["My.Key"]);

            Loc.Instance.ClearLanguages();
        }
        [Fact]
        public void AddLanguageDictionary_OverwritesExistingByDefault()
        {
            Loc.Instance.AddLanguageDictionary("en", new Dictionary<string, string>
            {
                { "My.Key", "" }
            });
            Loc.Instance.AddLanguageDictionary("en", new Dictionary<string, string>
            {
                { "My.Key", "ASDF" }
            });

            Assert.Equal("ASDF", Loc.Instance.Languages["en"]["My.Key"]);

            Loc.Instance.ClearLanguages();
        }
        #endregion AddLanguageDictionary

        #region AddLanguageDictionaries
        [Fact]
        public void AddLanguageDictionaries_AddsToAvailableLanguageNames()
        {
            Loc.Instance.ClearLanguages();
            Loc.Instance.AddLanguageDictionaries(new Dictionary<string, Dictionary<string, string>>
            {
                { "en", new() },
                { "fr", new() },
            });
            Assert.Contains("en", Loc.Instance.AvailableLanguageNames);
            Assert.Contains("fr", Loc.Instance.AvailableLanguageNames);

            Loc.Instance.ClearLanguages(true, true);
        }
        [Fact]
        public void AddLanguageDictionaries_AddsAllLanguages()
        {
            Loc.Instance.AddLanguageDictionaries(new Dictionary<string, Dictionary<string, string>>
            {
                { "en", new()
                    {
                        { "ASDF", "jk;l" }
                    }
                },
                { "fr", new()
                    {
                        { "ASDF", "asdf" }
                    }
                },
            });

            Assert.Equal("jk;l", Loc.Instance.Translate("ASDF", null, "en"));
            Assert.Equal("asdf", Loc.Instance.Translate("ASDF", null, "fr"));

            Loc.Instance.ClearLanguages(true, true);
        }
        #endregion AddLanguageDictionaries

        #region Translate
        [Fact]
        public void Translate_ExceptionWhenThrowOnMissingTranslationEnabled()
        {
            Assert.True(Loc.Instance.ThrowOnMissingTranslation = true, "Setup failed");

            Assert.Throws<MissingTranslationException>(() => Loc.Instance.Translate("A.B.C"));
            Assert.Throws<MissingTranslationException>(() => Loc.Instance.Translate("A.B.C", StringComparison.OrdinalIgnoreCase, "default"));
            Assert.Throws<MissingTranslationException>(() => Loc.Instance.Translate("A.B.C", null, ""));
            Assert.Throws<MissingTranslationException>(() => Loc.Instance.Translate("A.B.C", StringComparison.OrdinalIgnoreCase, "default", ""));

            Loc.Instance.ThrowOnMissingTranslation = false; //< cleanup
        }
        [Fact]
        public void Translate_NoExceptionWhenThrowOnMissingTranslationDisabled()
        {
            Assert.False(Loc.Instance.ThrowOnMissingTranslation = false, "Setup failed");

            Loc.Instance.Translate("A.B.C", null);
            Loc.Instance.Translate("A.B.C", StringComparison.OrdinalIgnoreCase, "");
            Loc.Instance.Translate("A.B.C", null, "");
            Loc.Instance.Translate("A.B.C", StringComparison.OrdinalIgnoreCase, "", "");
        }
        [Fact]
        public void Translate_CanUseCaseInsensitiveCompare()
        {
            Loc.Instance.AddLanguageDictionary("en", new Dictionary<string, string> { { "Aa.Bb.Cc", "Hello World!" } });
            Loc.Instance.CurrentLanguageName = "en";

            Assert.Equal("Hello World!", Loc.Instance.Translate("aA.bB.cC", StringComparison.OrdinalIgnoreCase));
            Assert.Equal("Hello World!", Loc.Instance.Translate("aA.bB.cC", StringComparison.OrdinalIgnoreCase, null, "en"));

            Loc.Instance.ClearLanguages(true, true);
        }
        [Fact]
        public void Translate_UsesDefaultText()
        {
            Assert.False(Loc.Instance.ThrowOnMissingTranslation, "setup failed!"); //< unit test interaction error

            Assert.Equal("default", Loc.Instance.Translate("A.B.C", "default"));
            Assert.Equal("default", Loc.Instance.Translate("A.B.C", StringComparison.OrdinalIgnoreCase, "default"));
            Assert.Equal("default", Loc.Instance.Translate("A.B.C", "default", ""));
            Assert.Equal("default", Loc.Instance.Translate("A.B.C", StringComparison.OrdinalIgnoreCase, "default", ""));

            Loc.Instance.ClearLanguages(true, true);
        }
        [Fact]
        public void Translate_ReturnsBlankWhenNotUsingStringPath()
        {
            Loc.Instance.UseStringPathAsFallback = false;
            Assert.Same("", Loc.Instance.Translate("jkl;"));
            Loc.Instance.UseStringPathAsFallback = true;
            Assert.Same("jkl;", Loc.Instance.Translate("jkl;"));
        }
        #endregion Translate

        #region CurrentLanguageName
        [Fact]
        public void CurrentLanguageName_SetsCurrentLanguageDictionary()
        {
            Loc.Instance.AddLanguageDictionary("en", new Dictionary<string, string> { { "Aa.Bb.Cc", "Hello World!" } });

            // setup
            Assert.Empty(Loc.Instance.CurrentLanguageName);
            Assert.Null(Loc.Instance.CurrentLanguageDictionary);

            // test setting a language
            Loc.Instance.CurrentLanguageName = "en";
            Assert.True(Loc.Instance.CurrentLanguageName.Length > 0); //< not blocked by Changing event
            Assert.NotNull(Loc.Instance.CurrentLanguageDictionary);

            // cleanup
            Loc.Instance.ClearLanguages(true, true);
        }
        [Fact]
        public void CurrentLanguageName_UnsetsCurrentLanguageDictionary()
        {
            Loc.Instance.AddLanguageDictionary("en", new Dictionary<string, string> { { "Aa.Bb.Cc", "Hello World!" } });

            // setup
            Assert.Empty(Loc.Instance.CurrentLanguageName);
            Assert.Null(Loc.Instance.CurrentLanguageDictionary);

            // test unsetting a language
            Loc.Instance.CurrentLanguageName = "en";
            Loc.Instance.CurrentLanguageName = "";
            Assert.True(Loc.Instance.CurrentLanguageName.Length == 0); //< not blocked by Changing event
            Assert.Null(Loc.Instance.CurrentLanguageDictionary);

            // cleanup
            Loc.Instance.ClearLanguages(true, true);
        }
        #endregion CurrentLanguageName

        #region FallbackLanguageName
        [Fact]
        public void FallbackLanguageName_SetsFallbackLanguageDictionary()
        {
            Loc.Instance.AddLanguageDictionary("en", new Dictionary<string, string> { { "Aa.Bb.Cc", "Hello World!" } });

            // setup
            Assert.Null(Loc.Instance.FallbackLanguageName);
            Assert.Null(Loc.Instance.FallbackLanguageDictionary);

            // test setting a fallback
            Loc.Instance.FallbackLanguageName = "en";
            Assert.NotNull(Loc.Instance.FallbackLanguageName);
            Assert.NotNull(Loc.Instance.FallbackLanguageDictionary);

            // cleanup
            Loc.Instance.ClearLanguages(true, true);
        }
        [Fact]
        public void FallbackLanguageName_UnsetsFallbackLanguageDictionary()
        {
            Loc.Instance.AddLanguageDictionary("en", new Dictionary<string, string> { { "Aa.Bb.Cc", "Hello World!" } });

            // setup
            Assert.Null(Loc.Instance.FallbackLanguageName);
            Assert.Null(Loc.Instance.FallbackLanguageDictionary);

            Loc.Instance.FallbackLanguageName = "en";

            // test unsetting fallback (empty)
            Loc.Instance.FallbackLanguageName = "";
            Assert.Empty(Loc.Instance.FallbackLanguageName);
            Assert.Null(Loc.Instance.FallbackLanguageDictionary);

            // test unsetting fallback (null)
            Loc.Instance.FallbackLanguageName = null;
            Assert.Null(Loc.Instance.FallbackLanguageName);
            Assert.Null(Loc.Instance.FallbackLanguageDictionary);

            // cleanup
            Loc.Instance.ClearLanguages(true, true);
        }
        #endregion FallbackLanguageName

        #region UseStringPathAsFallback
        [Fact]
        public void UseStringPathAsFallback_Works()
        {
            Loc.Instance.UseStringPathAsFallback = false;

            Assert.Empty(Loc.Instance.Translate("asdf"));

            Loc.Instance.UseStringPathAsFallback = true;

            Assert.Same("asdf", Loc.Instance.Translate("asdf"));
        }
        #endregion UseStringPathAsFallback

        #region AddTranslationLoader
        [Fact]
        public void AddTranslationLoader_Works()
        {
            Loc.Instance.AddTranslationLoader(new JsonTranslationLoader());

            Assert.NotEmpty(Loc.Instance.TranslationLoaders);

            Loc.Instance.TranslationLoaders.Clear();
        }
        #endregion AddTranslationLoader

        #region GetTranslationLoaderForFile
        [Fact]
        public void GetTranslationLoaderForFile_Works()
        {
            Assert.True(Loc.Instance.AddTranslationLoader(new JsonTranslationLoader()));

            Assert.NotNull(Loc.Instance.GetTranslationLoaderForFile("en.loc.json"));
            Assert.Null(Loc.Instance.GetTranslationLoaderForFile("en.loc.yaml"));

            Loc.Instance.TranslationLoaders.Clear();
        }
        [Fact]
        public void GetTranslationLoaderForFile_GenericWorks()
        {
            Assert.NotNull(Loc.Instance.AddTranslationLoader<JsonTranslationLoader>());

            Assert.NotNull(Loc.Instance.GetTranslationLoaderForFile("en.loc.json"));
            Assert.Null(Loc.Instance.GetTranslationLoaderForFile("en.loc.yaml"));

            Loc.Instance.TranslationLoaders.Clear();
        }
        #endregion GetTranslationLoaderForFile

        #region LoadFromFile
        [Fact]
        public void LoadFromFile_Works()
        {
            var tempFile = Path.GetTempFileName();
            File.Move(tempFile, tempFile = Path.ChangeExtension(tempFile, ".loc.json"));
            File.WriteAllText(tempFile, @"{""ShowDebugInfo"": {
                  ""ConfigPath"": {
                    ""Content"": {
                      ""English (US/CA)"": ""hello""
                    },
                    ""Tooltip"": {
                      ""English (US/CA)"": ""world""
                    }
                  },
                  ""Header"": {
                    ""Content"": {
                      ""English (US/CA)"": ""Show Debug Info""
                    }
                  }
                }}");

            Loc.Instance.AddTranslationLoader<JsonTranslationLoader>();

            Assert.True(Loc.Instance.LoadFromFile(tempFile));
            Assert.NotEmpty(Loc.Instance.Languages);
            Assert.Equal("world", Loc.Instance.Translate("ShowDebugInfo.ConfigPath.Tooltip", null, "English (US/CA)"));

            Loc.Instance.ClearLanguages();
            File.Delete(tempFile);
        }
        #endregion LoadFromFile

        #region LoadFromDirectory
        [Fact]
        public void LoadFromDirectory_Works()
        {
            var tempFile = Path.GetTempFileName();
            File.Move(tempFile, tempFile = Path.ChangeExtension(tempFile, ".loc.json"));
            File.WriteAllText(tempFile, @"{""ShowDebugInfo"": {
                  ""ConfigPath"": {
                    ""Content"": {
                      ""English (US/CA)"": ""hello""
                    },
                    ""Tooltip"": {
                      ""English (US/CA)"": ""world""
                    }
                  },
                  ""Header"": {
                    ""Content"": {
                      ""English (US/CA)"": ""Show Debug Info""
                    }
                  }
                }}");

            Loc.Instance.AddTranslationLoader<JsonTranslationLoader>();

            var secondTempFile = Path.GetTempFileName(); //< make sure dir isnt empty

            var result = Loc.Instance.LoadFromDirectory(Path.GetDirectoryName(tempFile)!);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.NotEmpty(Loc.Instance.Languages);
            Assert.Equal("world", Loc.Instance.Translate("ShowDebugInfo.ConfigPath.Tooltip", null, "English (US/CA)"));

            Loc.Instance.ClearLanguages();
            File.Delete(tempFile);
            File.Delete(secondTempFile);
        }
        #endregion LoadFromDirectory

        [Fact]
        public void MissingTranslationStringRequested_Fires()
        {
            bool set = false;
            var handler = new MissingTranslationStringRequestedEventHandler((_, _) =>
            {
                set = true;
            });
            Loc.Instance.MissingTranslationStringRequested += handler;

            Loc.Instance.Translate("A.B.C");
            Assert.True(set);
        }
        [Fact]
        public void MissingTranslationStringRequested_IsAccurate()
        {
            object? sender = null;
            MissingTranslationStringRequestedEventArgs? eventArgs = null;
            var handler = new MissingTranslationStringRequestedEventHandler((s, e) =>
            {
                sender = s;
                eventArgs = e;
            });
            Loc.Instance.MissingTranslationStringRequested += handler;

            Loc.Instance.Translate("A.B.C");

            // test sender
            Assert.NotNull(sender);
            Assert.Same(Loc.Instance, sender);

            // test event args
            Assert.NotNull(eventArgs);
            Assert.Equal("A.B.C", eventArgs.StringPath);
            Assert.Equal("", eventArgs.LanguageName);
        }
    }
}