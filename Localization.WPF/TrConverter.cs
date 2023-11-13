using System;
using System.Globalization;
using System.Windows.Data;

namespace Localization.WPF
{
    /// <summary>
    /// An <see cref="IValueConverter"/> that gets the translation for the specified key.
    /// </summary>
    [ValueConversion(typeof(string), typeof(string))]
    public class TrConverter : IValueConverter
    {
        #region Properties
        /// <summary>
        /// Gets or sets whether an exception is thrown on an invalid argument type. Defaults to <see langword="true"/>.
        /// </summary>
        public bool ThrowOnInvalidInput { get; set; } = true;
        /// <summary>
        /// Gets or sets the name of the language to get translations for. Uses the current language when <see langword="null"/>.
        /// </summary>
        public string? LanguageName { get; set; } = null;
        /// <summary>
        /// Gets or sets the string comparison mode to use for comparisons. Setting this to anything other than <see cref="StringComparison.Ordinal"/> will impact performance.
        /// </summary>
        public StringComparison StringComparison { get; set; } = StringComparison.Ordinal;
        #endregion Properties

        #region Methods
        private string GetTranslatedString(string key, string? defaultText)
        {
            if (StringComparison != StringComparison.Ordinal)
            {
                if (LanguageName == null)
                    return Loc.Tr(key, StringComparison, defaultText);
                return Loc.Tr(key, StringComparison, defaultText, LanguageName);
            }
            else
            {
                if (LanguageName == null)
                    return Loc.Tr(key, defaultText);
                return Loc.Tr(key, defaultText, LanguageName);
            }
        }
        #endregion Methods

        #region IValueConverter
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string key)
            {
                return GetTranslatedString(key, parameter as string);
            }
            else if (ThrowOnInvalidInput)
                throw new ArgumentException($"Expected type \"{typeof(string)}\", received type \"{value.GetType()}\"", nameof(value));
            else return null!;
        }
        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
        #endregion IValueConverter
    }
}
