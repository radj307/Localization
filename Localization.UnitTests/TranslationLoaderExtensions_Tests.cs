using Localization.Json;
using Localization.Xml;
using Localization.Yaml;

namespace Localization.UnitTests
{
    public class TranslationLoaderExtensions_Tests
    {
        #region ConflictsWith
        class TestTranslationLoader : ITranslationLoader
        {
            public string[] SupportedFileExtensions { get; } = new string[] { ".xml", ".json", ".yml", ".yaml" };

            public Dictionary<string, Dictionary<string, string>>? Deserialize(string serializedData) => throw new NotImplementedException();
            public string Serialize(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> languageDictionaries) => throw new NotImplementedException();
        }
        [Fact]
        public void ConflictsWith_IsAccurate()
        {
            var testLoader = new TestTranslationLoader();

            Assert.True(new JsonTranslationLoader().ConflictsWith(testLoader, allowPartialConflicts: true));
            Assert.False(testLoader.ConflictsWith(new JsonTranslationLoader(), allowPartialConflicts: true));
            Assert.True(testLoader.ConflictsWith(new JsonTranslationLoader(), allowPartialConflicts: false));

            Assert.True(new YamlTranslationLoader().ConflictsWith(testLoader, allowPartialConflicts: true));
            Assert.False(testLoader.ConflictsWith(new YamlTranslationLoader(), allowPartialConflicts: true));
            Assert.True(testLoader.ConflictsWith(new YamlTranslationLoader(), allowPartialConflicts: false));

            Assert.True(new XmlTranslationLoader().ConflictsWith(testLoader, allowPartialConflicts: true));
            Assert.False(testLoader.ConflictsWith(new XmlTranslationLoader(), allowPartialConflicts: true));
            Assert.True(testLoader.ConflictsWith(new XmlTranslationLoader(), allowPartialConflicts: false));
        }
        [Fact]
        public void ConflictsWith_DefaultAllowsPartialConflicts()
        {
            var testLoader = new TestTranslationLoader();

            Assert.False(testLoader.ConflictsWith(new JsonTranslationLoader()));
            Assert.True(new JsonTranslationLoader().ConflictsWith(testLoader));
        }
        #endregion ConflictsWith
    }
}
