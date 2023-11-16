namespace Localization.UnitTests
{
    public class DictionaryExtensions_Tests
    {
        #region Helpers
        private static Dictionary<TKey, Dictionary<TSubKey, TValue>> GetNestedTestDictionary<TKey, TSubKey, TValue>(Func<TKey> keyFactory, Func<TSubKey> subKeyFactory, Func<TValue> valueFactory, int dictionarySize = 3, int subDictionarySize = 10) where TKey : notnull where TSubKey : notnull where TValue : notnull
        {
            Dictionary<TKey, Dictionary<TSubKey, TValue>> dict = new();
            for (int i = 0; i < dictionarySize; ++i)
            {
                var subDict = GetTestDictionary(subKeyFactory, valueFactory, subDictionarySize);
                for (int count = 0; !dict.TryAdd(keyFactory(), subDict); ++count)
                {
                    if (count > 10)
                        throw new ArgumentException($"Setup failed! (Failed to get a unique dictionary key from the provided {nameof(keyFactory)} after 10 tries!)", nameof(keyFactory));
                }
            }
            return dict;
        }
        private static Dictionary<T, Dictionary<T, TValue>> GetNestedTestDictionary<T, TValue>(Func<T> keyFactory, Func<TValue> valueFactory, int dictionarySize = 3, int subDictionarySize = 10) where T : notnull where TValue : notnull
            => GetNestedTestDictionary(keyFactory, keyFactory, valueFactory, dictionarySize, subDictionarySize);
        private static Dictionary<T, Dictionary<T, T>> GetNestedTestDictionary<T>(Func<T> itemFactory, int dictionarySize = 3, int subDictionarySize = 10) where T : notnull
            => GetNestedTestDictionary(itemFactory, itemFactory, itemFactory, dictionarySize, subDictionarySize);
        private static Dictionary<TKey, TValue> GetTestDictionary<TKey, TValue>(Func<TKey> keyFactory, Func<TValue> valueFactory, int count = 10) where TKey : notnull where TValue : notnull
        {
            var dict = new Dictionary<TKey, TValue>();
            for (int i = 0; i < count; ++i)
            {
                var value = valueFactory();
                for (int tryCount = 0; !dict.TryAdd(keyFactory(), value); ++tryCount)
                {
                    if (tryCount > 10)
                        throw new ArgumentException($"Setup failed! (Failed to get a unique dictionary key from the provided {nameof(keyFactory)} after 10 tries!)", nameof(keyFactory));
                }
            }
            return dict;
        }
        private static Dictionary<T, T> GetTestDictionary<T>(Func<T> itemFactory, int count = 10) where T : notnull
            => GetTestDictionary(itemFactory, itemFactory, count);
        #endregion Helpers

        #region TryGetValue
        [Fact]
        public void TryGetValue_StringComparisonWorks()
        {
            string[] keys = new string[] { "one", "item2", "item3", "item4", "item5", "six", "seven", "eight", "nine", "ten" };
            int i = 0;
            var dict = GetTestDictionary(() => keys[i++], () => new object());

            Assert.True(dict.TryGetValue("SEVEN", StringComparison.OrdinalIgnoreCase, out var value));
            Assert.Same(dict["seven"], value);
        }
        #endregion TryGetValue

        #region ToReadOnlyDictionary
        [Fact]
        public void ToReadOnlyDictionary_NoThrows()
        {
            var dict = GetNestedTestDictionary(() => Random.Shared.Next());

            dict.ToReadOnlyDictionary();
        }
        [Fact]
        public void ToReadOnlyDictionary_IsAccurate()
        {
            var dict = GetNestedTestDictionary(() => Random.Shared.Next());
            var readOnlyDict = dict.ToReadOnlyDictionary();

            Assert.Equal(dict.Count, readOnlyDict.Count);
            Assert.Equivalent(dict, readOnlyDict, strict: true);
        }
        #endregion ToReadOnlyDictionary

        #region GetOrCreateValue
        [Fact]
        public void GetOrCreateValue_GetsExistingValue()
        {
            string[] keys = new string[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" };
            int i = 0;
            var dict = GetTestDictionary(() => keys[i++], () => new object());

            Assert.Same(dict[keys[6]], dict.GetOrCreateValue(keys[6]));
        }
        [Fact]
        public void GetOrCreateValue_CreatesMissingValue()
        {
            string[] keys = new string[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" };
            int i = 0;
            var dict = GetTestDictionary(() => keys[i++], () => new object());

            var size = dict.Count;
            var newValue1 = dict.GetOrCreateValue("someKey");
            Assert.Equal(1 + size, dict.Count);
            Assert.Same(dict["someKey"], newValue1);
        }
        [Fact]
        public void GetOrCreateValue_UsesFactoryItem()
        {
            string[] keys = new string[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" };
            int i = 0;
            var dict = GetTestDictionary(() => keys[i++], () => new object());

            var objInst = new object();
            var size = dict.Count;
            var newValue2 = dict.GetOrCreateValue("someOtherKey", () => objInst);
            Assert.Equal(1 + size, dict.Count);
            Assert.Same(objInst, newValue2);
        }
        #endregion GetOrCreateValue

        #region TryGetFirstValue
        [Fact]
        public void TryGetFirstValue_Works()
        {
            string[] keys = new string[] { "one", "item2", "item3", "item4", "item5", "six", "seven", "eight", "nine", "ten" };
            int i = 0;
            var dict = GetTestDictionary(() => keys[i++], () => new object());

            Assert.True(dict.TryGetFirstValue(keys, out var value));
            Assert.Same(dict["one"], value);
        }
        [Fact]
        public void TryGetFirstValue_RespectsPriority()
        {
            string[] keys = new string[] { "one", "item2", "item3", "item4", "item5", "six", "seven", "eight", "nine", "ten" };
            int i = 0;
            var dict = GetTestDictionary(() => keys[i++], () => new object());

            Assert.True(dict.TryGetFirstValue(new string[] { "item4", "item3" }, out var value));
            Assert.Same(dict["item4"], value);
        }
        #endregion TryGetFirstValue
    }
}
