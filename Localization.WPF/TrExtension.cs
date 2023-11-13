using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.TextFormatting;

namespace Localization.WPF
{
    public class TrExtension : MarkupExtension
    {
        #region Constructors
        public TrExtension()
        {
            Path = null!;
        }
        public TrExtension(string stringPath)
        {
            Path = stringPath;
        }
        public TrExtension(string stringPath, string? defaultText)
        {
            Path = stringPath;
            DefaultText = defaultText;
        }
        #endregion Constructors

        #region Fields
        private TranslationBinding translationBindingInst = null!;
        private BindingBase binding = null!;
        #endregion Fields

        #region Properties
        [ConstructorArgument("stringPath")]
        public string Path { get; set; }
        [ConstructorArgument("defaultText")]
        public string? DefaultText { get; set; }
        public string? LanguageName { get; set; }
        public StringComparison StringComparison { get; set; } = StringComparison.Ordinal;
        public string Prefix { get; set; } = string.Empty;
        public string Suffix { get; set; } = string.Empty;
        #endregion Properties

        #region Methods

        #region ProvideValue
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget provideValueTarget)
                return this;

            // setup bindings
            if (translationBindingInst == null)
            {
                translationBindingInst ??= new TranslationBinding(Path)
                {
                    DefaultText = DefaultText,
                    LanguageName = LanguageName,
                    StringComparison = StringComparison,
                    Prefix = Prefix,
                    Suffix = Suffix
                };
                binding = new Binding(nameof(TranslationBinding.Text))
                {
                    Source = translationBindingInst,
                };
            }

            var targetObject = provideValueTarget.TargetObject as DependencyObject;
            if (targetObject == null) return this;
            var targetProperty = provideValueTarget.TargetProperty as DependencyProperty;

            BindingOperations.SetBinding(targetObject, targetProperty, binding);
            var path = (string)targetObject!.GetValue(targetProperty);

            return binding.ProvideValue(serviceProvider);
        }
        #endregion ProvideValue

        #endregion Methods
    }
}
