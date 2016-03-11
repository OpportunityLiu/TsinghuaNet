using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static Settings.SettingsHelper;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TsinghuaNet
{
    public sealed partial class SettingsFlyout : Flyout
    {
        public SettingsFlyout()
        {
            this.InitializeComponent();
            this.Placement = FlyoutPlacementMode.Top;
        }

        public event EventHandler<string> SettingsChanged;

        private void raiseEvent(string settingsName)
        {
            var temp = SettingsChanged;
            if(temp != null)
            {
                temp(this, settingsName);
            }
        }

        private void Flyout_Opening(object sender, object e)
        {
            toggleSwitchLogOn.IsOn = GetLocal("AutoLogOn", true);

            switch((ElementTheme)Enum.Parse(typeof(ElementTheme), GetLocal("Theme", "Default")))
            {
            case ElementTheme.Default:
                comboBoxTheme.SelectedIndex = 0;
                break;
            case ElementTheme.Light:
                comboBoxTheme.SelectedIndex = 1;
                break;
            case ElementTheme.Dark:
                comboBoxTheme.SelectedIndex = 2;
                break;
            }
        }

        private void comboBoxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var theme = (ElementTheme)comboBoxTheme.SelectedIndex;
            SetLocal("Theme", theme.ToString());
            raiseEvent("Theme");
            //((FrameworkElement)Window.Current.Content).RequestedTheme = theme;
        }

        private void toggleSwitchLogOn_Toggled(object sender, RoutedEventArgs e)
        {
            SetLocal("AutoLogOn", toggleSwitchLogOn.IsOn);
            raiseEvent("AutoLogOn");
        }
    }
}
