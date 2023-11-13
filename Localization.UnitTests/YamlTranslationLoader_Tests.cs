using Localization.Yaml;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Localization.UnitTests
{
    public class YamlTranslationLoader_Tests
    {
        #region Deserialize
        [Fact]
        public void Deserialize_NoThrowOnValidYaml()
        {
            var loader = new YamlTranslationLoader();
            loader.Deserialize(@"""Root"":
              ""SubNode1"":
                ""lang"": ""Value""
              ""SubNode2"":
                ""lang"": ""Hello World!""");
        }
        [Fact]
        public void Deserialize_ThrowsOnInvalidYaml()
        {
            var loader = new YamlTranslationLoader();
            Assert.Throws<SemanticErrorException>(() => loader.Deserialize(@"""Node1"": ::::::::::"));
            Assert.Throws<SyntaxErrorException>(() => loader.Deserialize(@"""Node1: "));
        }
        [Fact]
        public void Deserialize_IsCorrect()
        {
            var loader = new YamlTranslationLoader();
            var dict = loader.Deserialize(@"""Root"":
              ""SubNode1"":
                ""lang"": ""Value""
              ""SubNode2"":
                ""lang"": ""Hello World!""");
            Assert.NotNull(dict);
            Assert.NotEmpty(dict);
            Assert.True(dict.ContainsKey("lang"));
            Assert.Equal("Value", dict["lang"]["Root.SubNode1"]);
            Assert.Equal("Hello World!", dict["lang"]["Root.SubNode2"]);
        }
        #endregion Deserialize

        #region Serialize
        [Fact]
        public void Serialize_NoThrow()
        {
            Dictionary<string, IReadOnlyDictionary<string, string>> dict = new() { { "lang", new Dictionary<string, string>() { { "A.B.C", "asdf" } } } };

            var loader = new YamlTranslationLoader();
            // test method impl
            loader.Serialize(dict, new Serializer());
            // test interface impl
            loader.Serialize(dict);
        }
        [Fact]
        public void Serialize_IsCorrect()
        {
            string yaml = @"Root:
  SubNode1:
    lang: Value
  SubNode2:
    lang: Hello World!
"; //<^ DO NOT CHANGE INDENTATION

            var loader = new YamlTranslationLoader();
            var dict = loader.Deserialize(yaml);
            Assert.Equal(yaml, loader.Serialize(dict!));
        }
        #endregion Serialize
    }
}
