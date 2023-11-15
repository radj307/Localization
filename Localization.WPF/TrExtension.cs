using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace Localization.WPF
{
    /// <summary>
    /// Markup extension that enables translations from within XAML.
    /// </summary>
    public class TrExtension : MarkupExtension
    {
        #region Constructors

        #region ()
        public TrExtension() { }
        #endregion ()

        #region (Key)
        public TrExtension(string key)
        {
            Key = key;
        }
        #endregion (Key)

        #region (Key, DefaultText)
        public TrExtension(string key, string defaultText)
        {
            Key = key;
            DefaultText = defaultText;
        }
        #endregion (Key, DefaultText)

        #region (Key, DefaultText, LanguageName)
        public TrExtension(string key, string defaultText, string languageName)
        {
            Key = key;
            DefaultText = defaultText;
            LanguageName = languageName;
        }
        #endregion (Key, DefaultText, LanguageName)

        #region (KeyBinding)
        public TrExtension(Binding keyBinding) { KeyBinding = keyBinding; }
        public TrExtension(MultiBinding keyBinding) { KeyBinding = keyBinding; }
        #endregion (KeyBinding)

        #region (KeyBinding, DefaultTextBinding)
        public TrExtension(Binding keyBinding, Binding defaultTextBinding) { KeyBinding = keyBinding; DefaultTextBinding = defaultTextBinding; }
        public TrExtension(MultiBinding keyBinding, Binding defaultTextBinding) { KeyBinding = keyBinding; DefaultTextBinding = defaultTextBinding; }
        public TrExtension(Binding keyBinding, MultiBinding defaultTextBinding) { KeyBinding = keyBinding; DefaultTextBinding = defaultTextBinding; }
        public TrExtension(MultiBinding keyBinding, MultiBinding defaultTextBinding) { KeyBinding = keyBinding; DefaultTextBinding = defaultTextBinding; }
        #endregion (KeyBinding, DefaultTextBinding)

        #region (KeyBinding, DefaultTextBinding, LanguageNameBinding)
        public TrExtension(Binding keyBinding, Binding defaultTextBinding, Binding languageNameBinding) { KeyBinding = keyBinding; DefaultTextBinding = defaultTextBinding; LanguageNameBinding = languageNameBinding; }
        public TrExtension(MultiBinding keyBinding, Binding defaultTextBinding, Binding languageNameBinding) { KeyBinding = keyBinding; DefaultTextBinding = defaultTextBinding; LanguageNameBinding = languageNameBinding; }
        public TrExtension(Binding keyBinding, MultiBinding defaultTextBinding, Binding languageNameBinding) { KeyBinding = keyBinding; DefaultTextBinding = defaultTextBinding; LanguageNameBinding = languageNameBinding; }
        public TrExtension(MultiBinding keyBinding, MultiBinding defaultTextBinding, Binding languageNameBinding) { KeyBinding = keyBinding; DefaultTextBinding = defaultTextBinding; LanguageNameBinding = languageNameBinding; }
        public TrExtension(Binding keyBinding, Binding defaultTextBinding, MultiBinding languageNameBinding) { KeyBinding = keyBinding; DefaultTextBinding = defaultTextBinding; LanguageNameBinding = languageNameBinding; }
        public TrExtension(MultiBinding keyBinding, Binding defaultTextBinding, MultiBinding languageNameBinding) { KeyBinding = keyBinding; DefaultTextBinding = defaultTextBinding; LanguageNameBinding = languageNameBinding; }
        public TrExtension(Binding keyBinding, MultiBinding defaultTextBinding, MultiBinding languageNameBinding) { KeyBinding = keyBinding; DefaultTextBinding = defaultTextBinding; LanguageNameBinding = languageNameBinding; }
        public TrExtension(MultiBinding keyBinding, MultiBinding defaultTextBinding, MultiBinding languageNameBinding) { KeyBinding = keyBinding; DefaultTextBinding = defaultTextBinding; LanguageNameBinding = languageNameBinding; }
        #endregion (KeyBinding, DefaultTextBinding, LanguageNameBinding)

        #endregion Constructors

        #region Fields
        private BindingBase binding = null!;
        #endregion Fields

        #region Properties
        /// <summary>
        /// Gets or sets the key of the target translation.
        /// </summary>
        [ConstructorArgument("key")]
        public string Key { get; set; } = null!;
        [ConstructorArgument("keyBinding")]
        public BindingBase? KeyBinding { get; set; }
        /// <summary>
        /// Gets or sets the text to return when the key wasn't found.
        /// </summary>
        [ConstructorArgument("defaultText")]
        public string? DefaultText { get; set; }
        [ConstructorArgument("defaultTextBinding")]
        public BindingBase? DefaultTextBinding { get; set; }
        /// <summary>
        /// Gets or sets the name of the language to get the translation for, or <see langword="null"/> to use the default behavior.
        /// </summary>
        [ConstructorArgument("languageName")]
        public string? LanguageName { get; set; }
        [ConstructorArgument("languageNameBinding")]
        public BindingBase? LanguageNameBinding { get; set; }
        /// <summary>
        /// Gets or sets the type of string comparison to use when comparing keys.
        /// </summary>
        public StringComparison StringComparison { get; set; } = StringComparison.Ordinal;
        public BindingBase? StringComparisonBinding { get; set; }
        /// <summary>
        /// Gets or sets a string to prepend to the translated string.
        /// </summary>
        public string Prefix { get; set; } = string.Empty;
        public BindingBase? PrefixBinding { get; set; }
        /// <summary>
        /// Gets or sets a string to append to the translated string.
        /// </summary>
        public string Suffix { get; set; } = string.Empty;
        public BindingBase? SuffixBinding { get; set; }
        #endregion Properties

        #region ProvideValue
        /// <summary>
        /// Provides a binding to the specified translation.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance to use.</param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget provideValueTarget)
                return this;

            if (binding == null)
            { // init binding
                var translationBindingInst = new TranslationBinding(this);
                // create the basic Text binding
                binding = new Binding(nameof(TranslationBinding.Text))
                {
                    Source = translationBindingInst
                };

                // check if a multibinding shim is required
                bool
                    hasKeyBinding = KeyBinding != null,
                    hasDefaultTextBinding = DefaultTextBinding != null,
                    hasLanguageNameBinding = LanguageNameBinding != null,
                    hasStringComparisonBinding = StringComparisonBinding != null,
                    hasPrefixBinding = PrefixBinding != null,
                    hasSuffixBinding = SuffixBinding != null;
                if (hasKeyBinding || hasDefaultTextBinding || hasLanguageNameBinding
                    || hasStringComparisonBinding || hasPrefixBinding || hasSuffixBinding)
                { // inject bindings into a multibinding
                    MultiBinding multiBinding = new()
                    {
                        Converter = new InternalMultiBindingConverter(this, translationBindingInst)
                    };

                    void AddBinding(BindingBase binding)
                    {
                        if (binding is MultiBinding mb)
                        {
                            foreach (var b in mb.Bindings)
                            {
                                multiBinding.Bindings.Add(b);
                            }
                        }
                        else
                        {
                            multiBinding.Bindings.Add(binding);
                        }
                    }

                    // order of adding bindings must match value processing order in the converter

                    if (hasKeyBinding)
                        AddBinding(KeyBinding!);

                    if (hasDefaultTextBinding)
                        AddBinding(DefaultTextBinding!);

                    if (hasLanguageNameBinding)
                        AddBinding(LanguageNameBinding!);

                    if (hasStringComparisonBinding)
                        AddBinding(StringComparisonBinding!);

                    if (hasPrefixBinding)
                        AddBinding(PrefixBinding!);

                    if (hasSuffixBinding)
                        AddBinding(SuffixBinding!);

                    // use multiBinding as a shim
                    multiBinding.Bindings.Add(binding);
                    binding = multiBinding;
                }
            }

            if (provideValueTarget.TargetObject is not DependencyObject targetObject) return this;
            var targetProperty = provideValueTarget.TargetProperty as DependencyProperty;

            BindingOperations.SetBinding(targetObject, targetProperty, binding);

            return binding.ProvideValue(serviceProvider);
        }
        #endregion ProvideValue

        #region (Class) InternalMultiBindingConverter
        class InternalMultiBindingConverter : IMultiValueConverter
        {
            public InternalMultiBindingConverter(TrExtension trExtension, TranslationBinding translationBinding)
            {
                owner = trExtension;
                translationBindingInst = translationBinding;
            }

            private readonly TrExtension owner; //< NOT for accessing non-bindings
            private readonly TranslationBinding translationBindingInst;
            private BindingBase? KeyBinding => owner.KeyBinding;
            private BindingBase? DefaultTextBinding => owner.DefaultTextBinding;
            private BindingBase? LanguageNameBinding => owner.LanguageNameBinding;
            private BindingBase? StringComparisonBinding => owner.StringComparisonBinding;
            private BindingBase? PrefixBinding => owner.PrefixBinding;
            private BindingBase? SuffixBinding => owner.SuffixBinding;


            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                try
                {
                    int i = 0;

                    // update the translation binding's properties so it refreshes the text

                    // key
                    if (KeyBinding is MultiBinding keyMb)
                    {
                        var keyMb_values = values[i..keyMb.Bindings.Count];
                        owner.Key = keyMb.Converter.Convert(keyMb_values, typeof(string), keyMb.ConverterParameter, keyMb.ConverterCulture)?.ToString() ?? string.Empty;
                        i += keyMb_values.Length;
                    }
                    else if (KeyBinding is Binding)
                    {
                        owner.Key = values[i++]?.ToString() ?? string.Empty;
                    }

                    // default text
                    if (DefaultTextBinding is MultiBinding defTextMb)
                    {
                        var defTextMb_values = values[i..defTextMb.Bindings.Count];
                        owner.DefaultText = defTextMb.Converter.Convert(defTextMb_values, typeof(string), defTextMb.ConverterParameter, defTextMb.ConverterCulture)?.ToString();
                        i += defTextMb_values.Length;
                    }
                    else if (DefaultTextBinding is Binding)
                    {
                        owner.DefaultText = values[i++]?.ToString();
                    }

                    // lang name
                    if (LanguageNameBinding is MultiBinding langNameMb)
                    {
                        var langNameMb_values = values[i..langNameMb.Bindings.Count];
                        owner.LanguageName = langNameMb.Converter.Convert(langNameMb_values, typeof(string), langNameMb.ConverterParameter, langNameMb.ConverterCulture)?.ToString();
                        i += langNameMb_values.Length;
                    }
                    else if (LanguageNameBinding is Binding)
                    {
                        owner.LanguageName = values[i++]?.ToString();
                    }

                    // string comp
                    if (StringComparisonBinding is MultiBinding stringCompMb)
                    {
                        var stringCompMb_values = values[i..stringCompMb.Bindings.Count];
                        owner.StringComparison = (StringComparison)stringCompMb.Converter.Convert(stringCompMb_values, typeof(StringComparison), stringCompMb.ConverterParameter, stringCompMb.ConverterCulture);
                        i += stringCompMb_values.Length;
                    }
                    else if (StringComparisonBinding is Binding)
                    {
                        owner.StringComparison = (StringComparison)values[i++];
                    }

                    // prefix
                    if (PrefixBinding is MultiBinding prefixBindingMb)
                    {
                        var prefixBindingMb_values = values[i..prefixBindingMb.Bindings.Count];
                        owner.Prefix = prefixBindingMb.Converter.Convert(prefixBindingMb_values, typeof(StringComparison), prefixBindingMb.ConverterParameter, prefixBindingMb.ConverterCulture)?.ToString() ?? string.Empty;
                        i += prefixBindingMb_values.Length;
                    }
                    else if (PrefixBinding is Binding)
                    {
                        owner.Prefix = values[i++]?.ToString() ?? string.Empty;
                    }

                    // suffix
                    if (SuffixBinding is MultiBinding suffixBindingMb)
                    {
                        var suffixBindingMb_values = values[i..suffixBindingMb.Bindings.Count];
                        owner.Suffix = suffixBindingMb.Converter.Convert(suffixBindingMb_values, typeof(StringComparison), suffixBindingMb.ConverterParameter, suffixBindingMb.ConverterCulture)?.ToString() ?? string.Empty;
                        i += suffixBindingMb_values.Length;
                    }
                    else if (SuffixBinding is Binding)
                    {
                        owner.Suffix = values[i++]?.ToString() ?? string.Empty;
                    }
                }
                catch { }

                return translationBindingInst.Text;
            }
            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }
        #endregion (Class) InternalMultiBindingConverter
    }
}
