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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TsinghuaNet
{
    public sealed partial class SettingsFlyout : Flyout
    {
        public SettingsFlyout()
        {
            this.InitializeComponent();
        }

        private void Flyout_Opening(object sender, object e)
        {
            switch((ElementTheme)Enum.Parse(typeof(ElementTheme), ApplicationData.Current.LocalSettings.Values["Theme"].ToString()))
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
            ApplicationData.Current.LocalSettings.Values["Theme"] = theme.ToString();
        }
    }
}
