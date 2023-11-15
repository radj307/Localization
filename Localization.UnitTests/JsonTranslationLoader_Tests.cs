using Localization.Json;
using Newtonsoft.Json;

namespace Localization.UnitTests
{
    public class JsonTranslationLoader_Tests
    {
        #region Deserialize
        [Fact]
        public void Deserialize_NoThrowOnValidJson()
        {
            string json = @"{""ShowDebugInfo"": {
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
            }}";

            var loader = new JsonTranslationLoader();
            loader.Deserialize(json);
        }
        [Fact]
        public void Deserialize_ThrowOnInvalidJson()
        {
            string json = @"{""ShowDebugInfo"": {
                  ""ConfigPath"": {
                    ""Content"": {
                      ""English (US/CA)"": ""hello""
                    },}";

            var loader = new JsonTranslationLoader();
            Assert.Throws<JsonSerializationException>(() => loader.Deserialize(json));
        }
        [Fact]
        public void Deserialize_IsCorrect()
        {
            string json = @"{""ShowDebugInfo"": {
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
                }}";

            var loader = new JsonTranslationLoader();
            var dict = loader.Deserialize(json);
            Assert.NotNull(dict);
            Assert.True(dict.ContainsKey("English (US/CA)"));
            Assert.Equal("hello", dict["English (US/CA)"]["ShowDebugInfo.ConfigPath.Content"]);
        }
        #endregion Deserialize

        #region Serialize
        [Fact]
        public void Serialize_NoThrow()
        {
            Loc.Instance.AddTranslations("en", new Dictionary<string, string>
            {
                {  "A.B.C.D", "!!!" },
                {  "Root.Sub1.Sub2.Content1", "asdf" },
                {  "Root.Sub1.Sub2.Content2", "jkl;" },
            });

            var loader = new JsonTranslationLoader();
            // test method impl
            loader.Serialize(Loc.Instance.Languages["en"].ToLanguageDictionaries("en"), Formatting.None);
            // test interface impl
            loader.Serialize(Loc.Instance.Languages["en"].ToLanguageDictionaries("en"));

            Loc.Instance.ClearLanguages();
        }
        [Fact]
        public void Serialize_IsCorrect()
        {
            string json = @"{""A"":{""B"":{""lang"":""asdf""},""C"":{""lang"":""jkl""}}}";
            var loader = new JsonTranslationLoader();
            var dict = loader.Deserialize(json);
            Assert.Equal(json, loader.Serialize(dict!.AsReadOnlyDictionary(), Formatting.None));
        }
        #endregion Serialize

        #region SupportedFileExtensions
        [Fact]
        public void SupportedFileExtensions_IsCorrect()
        {
            var loader = new JsonTranslationLoader();
            Assert.Contains(".json", loader.SupportedFileExtensions);
        }
        [Fact]
        public void SupportedFileExtensions_CanLoadJsonFile()
        {
            var loader = new JsonTranslationLoader();
            Assert.True(loader.CanLoadFile("test.loc.json"));
            Assert.True(loader.CanLoadFile("C:/Users/Username/AppData/Local/this/path/is/kinda/long/test.loc.json"));
            Assert.True(loader.CanLoadFile("test.asdf.jkl.loc.json"));
        }
        [Fact]
        public void SupportedFileExtensions_CantLoadOtherFile()
        {
            var loader = new JsonTranslationLoader();
            Assert.False(loader.CanLoadFile("test.loc.yaml"));
            Assert.False(loader.CanLoadFile("C:/Users/Username/AppData/Local/this/path/is/kinda/long/test.loc.sjno"));
            Assert.False(loader.CanLoadFile("test.asdf.jkl.loc.jsn"));
        }
        #endregion SupportedFileExtensions
    }
}