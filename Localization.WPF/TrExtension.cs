using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Localization.WPF
{
    /// <summary>
    /// Markup extension that provides a data binding for a translated string.
    /// </summary>
    public class TrExtension : MarkupExtension
    {
        #region Static
        /// <summary>
        /// Gets or sets whether exceptions that occur as a result of bindings in <see cref="TrExtension"/> instances are thrown, or trigger an <see cref="BindingThrewException"/> event.
        /// </summary>
        /// <returns><see langword="true"/> when exceptions are caught and trigger a <see cref="BindingThrewException"/> event; otherwise when <see langword="false"/>, the exception is rethrown as a <see cref="TrExtensionBindingException"/>.</returns>
        public static bool CatchBindingExceptions { get; set; } = true;

        /// <summary>
        /// Occurs when <see cref="CatchBindingExceptions"/> is <see langword="true"/> and an exception occurred within a programmer-provided data binding.
        /// </summary>
        public static event EventHandler<TrExtensionBindingException>? BindingThrewException;
        internal static void NotifyBindingThrewException(TrExtension sender, Exception exception)
        {
            var ex = new TrExtensionBindingException(sender, exception);
            if (CatchBindingExceptions)
                BindingThrewException?.Invoke(sender, ex);
            else throw ex;
        }
        #endregion Static

        #region Constructors
        // XAML requires constructor parameter types to match EXACTLY, so a lot
        //  of constructors are required to accept all combinations of bindings

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
        List<(int ArgIndex, BindingBase Binding)>? bindingsInFormatArgs = null;
        #endregion Fields

        #region Properties
        /// <summary>
        /// Gets or sets the key/path of the target translation string.
        /// </summary>
        [ConstructorArgument("key")]
        public string Key { get; set; } = null!;
        /// <summary>
        /// Gets or sets a binding to use for <see cref="Key"/>.
        /// </summary>
        [ConstructorArgument("keyBinding")]
        public BindingBase? KeyBinding { get; set; }
        /// <summary>
        /// Gets or sets the text to return when the key wasn't found.
        /// </summary>
        [ConstructorArgument("defaultText")]
        public string? DefaultText { get; set; }
        /// <summary>
        /// Gets or sets a binding to use for <see cref="DefaultText"/>.
        /// </summary>
        [ConstructorArgument("defaultTextBinding")]
        public BindingBase? DefaultTextBinding { get; set; }
        /// <summary>
        /// Gets or sets the name of the language to get the translation for, or <see langword="null"/> to use the default behavior.
        /// </summary>
        [ConstructorArgument("languageName")]
        public string? LanguageName { get; set; }
        /// <summary>
        /// Gets or sets a binding to use for <see cref="LanguageName"/>.
        /// </summary>
        [ConstructorArgument("languageNameBinding")]
        public BindingBase? LanguageNameBinding { get; set; }
        /// <summary>
        /// Gets or sets the type of string comparison to use when comparing keys.
        /// </summary>
        public StringComparison StringComparison { get; set; } = StringComparison.Ordinal;
        /// <summary>
        /// Gets or sets a binding to use for <see cref="StringComparison"/>.
        /// </summary>
        public BindingBase? StringComparisonBinding { get; set; }
        /// <summary>
        /// Gets or sets a string to prepend to the translated string.
        /// </summary>
        public string Prefix { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets a binding to use for <see cref="Prefix"/>.
        /// </summary>
        public BindingBase? PrefixBinding { get; set; }
        /// <summary>
        /// Gets or sets a string to append to the translated string.
        /// </summary>
        public string Suffix { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets a binding to use for <see cref="Suffix"/>.
        /// </summary>
        public BindingBase? SuffixBinding { get; set; }
        /// <summary>
        /// Gets or sets the format string to use in string.Format, or <see langword="null"/> if you don't want to use string formatting.
        /// The Prefix &amp; Suffix strings are added to the result.<br/>
        /// The translated string is always argument "{0}", the first user-provided format arg is "{1}", and so on.
        /// </summary>
        /// <remarks>
        /// You can use square brackets "[0]" instead of regular brackets "{0}" to avoid issues with XAML strings not allowing '{' as the first character.
        /// </remarks>
        public string? FormatString { get; set; } = null;
        /// <summary>
        /// Gets or sets a binding to use for <see cref="FormatString"/>.
        /// </summary>
        public BindingBase? FormatStringBinding { get; set; }
        /// <summary>
        /// Gets or sets the arguments for string.Format, used in conjunction with the FormatString.<br/>
        /// Elements can be literals of any type, a <see cref="Binding"/>, or a <see cref="MultiBinding"/>.<br/>
        /// Since the translated string is always arg "{0}", the first FormatArgs element (with index 0) is "{1}", the second is "{2}", and so on.
        /// </summary>
        /// <remarks>
        /// You can use the provided <see cref="MakeArrayExtension"/> markup extension to create an array within the markup without using nested XAML elements.
        /// </remarks>
        public object?[]? FormatArgs { get; set; }
        /// <summary>
        /// Gets or sets a binding to use for <see cref="FormatArgs"/>.
        /// </summary>
        public BindingBase? FormatArgsBinding { get; set; }
        #endregion Properties

        #region ProvideValue
        /// <summary>
        /// Provides a binding to the specified translation.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance to use.</param>
        /// <returns>A <see cref="BindingBase"/> instance bound to the specified translation key that automatically updates when any of the bindings update.</returns>
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
                    has_KeyBinding = KeyBinding != null,
                    has_DefaultTextBinding = DefaultTextBinding != null,
                    has_LanguageNameBinding = LanguageNameBinding != null,
                    has_StringComparisonBinding = StringComparisonBinding != null,
                    has_PrefixBinding = PrefixBinding != null,
                    has_SuffixBinding = SuffixBinding != null,
                    has_FormatStringBinding = FormatStringBinding != null,
                    has_FormatArgsBinding = FormatArgsBinding != null,
                    has_BindingsInFormatArgs = !has_FormatArgsBinding && (FormatArgs?.Any(arg => arg is BindingBase) ?? false);
                if (has_KeyBinding || has_DefaultTextBinding || has_LanguageNameBinding
                    || has_StringComparisonBinding || has_PrefixBinding || has_SuffixBinding
                    || has_FormatStringBinding || has_FormatArgsBinding || has_BindingsInFormatArgs)
                { // inject bindings into a multibinding
                    MultiBinding multiBinding = new()
                    {
                        Converter = new InternalMultiBindingConverter(this, translationBindingInst)
                    };

                    void AddPackedBinding(BindingBase binding)
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

                    // pack bindings into the multibinding
                    //  the order must match the unpacking order in the converter

                    if (has_KeyBinding)
                        AddPackedBinding(KeyBinding!);
                    if (has_DefaultTextBinding)
                        AddPackedBinding(DefaultTextBinding!);
                    if (has_LanguageNameBinding)
                        AddPackedBinding(LanguageNameBinding!);
                    if (has_StringComparisonBinding)
                        AddPackedBinding(StringComparisonBinding!);
                    if (has_PrefixBinding)
                        AddPackedBinding(PrefixBinding!);
                    if (has_SuffixBinding)
                        AddPackedBinding(SuffixBinding!);
                    if (has_FormatStringBinding)
                        AddPackedBinding(FormatStringBinding!);
                    if (has_FormatArgsBinding)
                        AddPackedBinding(FormatArgsBinding!);
                    else if (has_BindingsInFormatArgs)
                    {
                        bindingsInFormatArgs = new();
                        for (int i = 0, i_max = FormatArgs!.Length; i < i_max; ++i)
                        {
                            if (FormatArgs[i] is BindingBase formatArgBinding)
                            {
                                bindingsInFormatArgs.Add((i, formatArgBinding));
                                AddPackedBinding(formatArgBinding);
                            }
                        }
                    }

                    // use multiBinding as a shim
                    multiBinding.Bindings.Add(binding);
                    binding = multiBinding;
                }
            } //< end binding init

            if (provideValueTarget.TargetObject is not DependencyObject targetObject) return this;
            var targetProperty = provideValueTarget.TargetProperty as DependencyProperty;

            BindingOperations.SetBinding(targetObject, targetProperty, binding);

            return binding.ProvideValue(serviceProvider);
        }
        #endregion ProvideValue

        #region (Class) InternalMultiBindingConverter
        class InternalMultiBindingConverter : IMultiValueConverter
        {
            #region Constructor
            public InternalMultiBindingConverter(TrExtension trExtension, TranslationBinding translationBinding)
            {
                owner = trExtension;
                translationBindingInst = translationBinding;
            }
            #endregion Constructor

            #region Fields
            private readonly TrExtension owner;
            private readonly TranslationBinding translationBindingInst; //< Only use this to retrieve the translated text
            #endregion Fields

            #region Methods

            #region Convert
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                // unpack values & pass them to their respective converters, and update the translation binding
                int i = 0;

                object? Unpack(BindingBase? binding, Type targetType)
                {
                    if (binding == null) return null;

                    if (binding is MultiBinding mb)
                    {
                        var mb_values = values[i..mb.Bindings.Count];
                        i += mb_values.Length;
                        return mb.Converter.Convert(mb_values, targetType, mb.ConverterParameter, mb.ConverterCulture);
                    }
                    else if (binding is Binding b)
                    {
                        return b.Converter is IValueConverter converter
                            ? converter.Convert(values[i++], targetType, b.ConverterParameter, b.ConverterCulture)
                            : values[i++];
                    }
                    else throw new InvalidOperationException($"{nameof(TrExtension)} properties do not support bindings of type {binding.GetType()}! (Expected {nameof(Binding)} or {nameof(MultiBinding)})");
                }

                // KeyBinding
                if (owner.KeyBinding != null)
                {
                    try
                    {
                        owner.Key = Unpack(owner.KeyBinding, typeof(string))?.ToString() ?? string.Empty;
                    }
                    catch (Exception ex)
                    {
                        NotifyBindingThrewException(owner, ex);
                    }
                }

                // DefaultTextBinding
                if (owner.DefaultTextBinding != null)
                {
                    try
                    {
                        owner.DefaultText = Unpack(owner.DefaultTextBinding, typeof(string))?.ToString();
                    }
                    catch (Exception ex)
                    {
                        NotifyBindingThrewException(owner, ex);
                    }
                }

                // LanguageNameBinding
                if (owner.LanguageNameBinding != null)
                {
                    try
                    {
                        owner.LanguageName = Unpack(owner.LanguageNameBinding, typeof(string))?.ToString();
                    }
                    catch (Exception ex)
                    {
                        NotifyBindingThrewException(owner, ex);
                    }
                }

                // StringComparisonBinding
                if (owner.StringComparisonBinding != null)
                {
                    try
                    {
                        if (Unpack(owner.StringComparisonBinding, typeof(StringComparison)) is object value)
                        {
                            if (value is string str)
                                value = Enum.Parse<StringComparison>(str);
                            owner.StringComparison = (StringComparison)value!;
                        }
                    }
                    catch (Exception ex)
                    {
                        NotifyBindingThrewException(owner, ex);
                    }
                }

                // PrefixBinding
                if (owner.PrefixBinding != null)
                {
                    try
                    {
                        owner.Prefix = Unpack(owner.PrefixBinding, typeof(string))?.ToString() ?? string.Empty;
                    }
                    catch (Exception ex)
                    {
                        NotifyBindingThrewException(owner, ex);
                    }
                }

                // SuffixBinding
                if (owner.SuffixBinding != null)
                {
                    try
                    {
                        owner.Suffix = Unpack(owner.SuffixBinding, typeof(string))?.ToString() ?? string.Empty;
                    }
                    catch (Exception ex)
                    {
                        NotifyBindingThrewException(owner, ex);
                    }
                }

                // FormatStringBinding
                if (owner.FormatStringBinding != null)
                {
                    try
                    {
                        owner.FormatString = Unpack(owner.FormatStringBinding, typeof(string))?.ToString() ?? string.Empty;
                    }
                    catch (Exception ex)
                    {
                        NotifyBindingThrewException(owner, ex);
                    }
                }

                // FormatArgsBinding
                if (owner.FormatArgsBinding != null)
                {
                    try
                    {
                        owner.FormatArgs = (object[]?)Unpack(owner.FormatArgsBinding, typeof(object[]));
                    }
                    catch (Exception ex)
                    {
                        NotifyBindingThrewException(owner, ex);
                    }
                }
                // BindingsInFormatArgs
                else if (owner.bindingsInFormatArgs != null)
                {
                    foreach (var (argIndex, argBinding) in owner.bindingsInFormatArgs)
                    {
                        owner.FormatArgs![argIndex] = Unpack(argBinding, typeof(object));
                    }
                }

                return translationBindingInst.Text;
            }
            #endregion Convert

            #region ConvertBack
            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
                => throw new NotImplementedException($"{nameof(TrExtension)} cannot perform reverse conversions on bindings.");
            #endregion ConvertBack

            #endregion Methods
        }
        #endregion (Class) InternalMultiBindingConverter
    }
}
