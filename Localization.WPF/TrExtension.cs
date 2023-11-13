using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace Localization.WPF
{
    /// <summary>
    /// Markup extension that enables translations from within XAML.
    /// </summary>
    public class TrExtension : MarkupExtension
    {
        #region Constructors
        /// <summary>
        /// Creates a new <see cref="TrExtension"/> instance without a key.
        /// </summary>
        public TrExtension()
        {
            Key = null!;
        }
        /// <summary>
        /// Creates a new <see cref="TrExtension"/> instance with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the target translation.</param>
        public TrExtension(string key)
        {
            Key = key;
        }
        /// <summary>
        /// Creates a new <see cref="TrExtension"/> instance with the specified <paramref name="key"/> and <paramref name="defaultText"/>.
        /// </summary>
        /// <param name="key">The key of the target translation.</param>
        /// <param name="defaultText">The text to return when the <paramref name="key"/> wasn't found.</param>
        public TrExtension(string key, string? defaultText)
        {
            Key = key;
            DefaultText = defaultText;
        }
        #endregion Constructors

        #region Fields
        private TranslationBinding translationBindingInst = null!;
        private BindingBase binding = null!;
        #endregion Fields

        #region Properties
        /// <summary>
        /// Gets or sets the key of the target translation.
        /// </summary>
        [ConstructorArgument("key")]
        public string Key { get; set; }
        /// <summary>
        /// Gets or sets the text to return when the key wasn't found.
        /// </summary>
        [ConstructorArgument("defaultText")]
        public string? DefaultText { get; set; }
        /// <summary>
        /// Gets or sets the name of the language to get the translation for, or <see langword="null"/> to use the default behavior.
        /// </summary>
        public string? LanguageName { get; set; }
        /// <summary>
        /// Gets or sets the type of string comparison to use when comparing keys.
        /// </summary>
        public StringComparison StringComparison { get; set; } = StringComparison.Ordinal;
        /// <summary>
        /// Gets or sets a string to prepend to the translated string.
        /// </summary>
        public string Prefix { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets a string to append to the translated string.
        /// </summary>
        public string Suffix { get; set; } = string.Empty;
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

            // setup bindings
            if (translationBindingInst == null)
            {
                translationBindingInst = new TranslationBinding(this);
                binding = new Binding(nameof(TranslationBinding.Text))
                {
                    Source = translationBindingInst,
                };
            }

            if (provideValueTarget.TargetObject is not DependencyObject targetObject) return this;
            var targetProperty = provideValueTarget.TargetProperty as DependencyProperty;

            BindingOperations.SetBinding(targetObject, targetProperty, binding);

            return binding.ProvideValue(serviceProvider);
        }
        #endregion ProvideValue
    }
}
