using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Localization.WPF
{
    public class TrExtension : MarkupExtension
    {
        #region Constructors
        public TrExtension()
        {
            Key = null!;
        }
        public TrExtension(string key)
        {
            Key = key;
        }
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
        [ConstructorArgument("key")]
        public string Key { get; set; }
        [ConstructorArgument("defaultText")]
        public string? DefaultText { get; set; }
        public string? LanguageName { get; set; }
        public StringComparison StringComparison { get; set; } = StringComparison.Ordinal;
        public string Prefix { get; set; } = string.Empty;
        public string Suffix { get; set; } = string.Empty;
        #endregion Properties

        #region ProvideValue
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
