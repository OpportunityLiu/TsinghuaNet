using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static Settings.SettingsHelper;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TsinghuaNet
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        public SettingsDialog()
        {
            this.InitializeComponent();
        }

        public event EventHandler<string> SettingsChanged;

        private void raiseEvent(string settingsName)
        {
            SettingsChanged?.Invoke(this, settingsName);
        }

        private void comboBoxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var theme = (ElementTheme)this.comboBoxTheme.SelectedIndex;
            SetLocal("Theme", theme.ToString());
            this.raiseEvent("Theme");
            //((FrameworkElement)Window.Current.Content).RequestedTheme = theme;
        }

        private void toggleSwitchLogOn_Toggled(object sender, RoutedEventArgs e)
        {
            SetLocal("AutoLogOn", this.toggleSwitchLogOn.IsOn);
            this.raiseEvent("AutoLogOn");
        }

        private void ContentDialog_Loading(FrameworkElement sender, object args)
        {
            this.toggleSwitchLogOn.IsOn = GetLocal("AutoLogOn", true);

            switch((ElementTheme)Enum.Parse(typeof(ElementTheme), GetLocal("Theme", "Default")))
            {
            case ElementTheme.Default:
                this.comboBoxTheme.SelectedIndex = 0;
                break;
            case ElementTheme.Light:
                this.comboBoxTheme.SelectedIndex = 1;
                break;
            case ElementTheme.Dark:
                this.comboBoxTheme.SelectedIndex = 2;
                break;
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }
    }
}
