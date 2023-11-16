using Localization;
using System.Windows;

namespace XamlHotReloadTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Loc.Instance.AddTranslations("English", new() { { "MainWindow.Text", "english text" } });
            Loc.Instance.AddTranslations("French", new() { { "MainWindow.Text", "french text" } });

            Loc.Instance.CurrentLanguageName = "English";

            InitializeComponent();
        }
    }
}
