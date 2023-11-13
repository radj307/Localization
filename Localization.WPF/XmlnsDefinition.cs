using System.Windows.Markup;

// Creates a global xmlns definition for the Localization.WPF namespace.
//  This allows TrExtension to be used anywhere without explicitly specifying the namespace.
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Localization.WPF")]
