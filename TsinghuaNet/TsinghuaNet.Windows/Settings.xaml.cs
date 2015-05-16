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

//“设置浮出控件”项模板在 http://go.microsoft.com/fwlink/?LinkId=273769 上有介绍

namespace TsinghuaNet
{
    public sealed partial class Settings :SettingsFlyout
    {
        public Settings()
        {
            this.InitializeComponent();
        }

        private void comboBoxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch(comboBoxTheme.SelectedIndex)
            {
            case 0:
                MainPage.Current.RequestedTheme = ElementTheme.Dark;
                break;
            case 1:
                MainPage.Current.RequestedTheme = ElementTheme.Light;
                break;
            default:
                break;
            }
        }

        private void SettingsFlyout_Unloaded(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values["Theme"] = MainPage.Current.RequestedTheme.ToString();
        }

        private void SettingsFlyout_Loaded(object sender, RoutedEventArgs e)
        {
            switch(MainPage.Current.RequestedTheme)
            {
            case ElementTheme.Dark:
                comboBoxTheme.SelectedIndex = 0;
                break;
            case ElementTheme.Light:
                comboBoxTheme.SelectedIndex = 1;
                break;
            default:
                comboBoxTheme.SelectedIndex = -1;
                break;
            }
        }
    }
}
