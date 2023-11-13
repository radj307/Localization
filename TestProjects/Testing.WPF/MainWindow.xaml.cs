﻿using Localization;
using Localization.Json;
using System.Windows;
using System.Windows.Controls;

namespace Testing.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var s in System.Windows.Forms.Screen.AllScreens)
            {
                if (s.Bounds.Contains((int)Left + 1920, (int)Top))
                { // get the window off of the main monitor
                    Left += 1920;
                    break;
                }
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;

            scrollViewer.ScrollToBottom();
        }

        private void ClearLanguagesButton_Click(object sender, RoutedEventArgs e)
        {
            Loc.Instance.ClearLanguages();
        }

        private void AddLanguages_Click(object sender, RoutedEventArgs e)
        {
            var loader = Loc.Instance.GetTranslationLoader<JsonTranslationLoader>();
            foreach (var name in ResourceHelper.ResourceNames)
            {
                if (!name.EndsWith(".loc.json"))
                    continue;
                Loc.Instance.LoadFromString(loader!, ResourceHelper.GetManifestResourceString(name)!);
            }
        }
    }
}
