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

//“设置浮出控件”项模板在 http://go.microsoft.com/fwlink/?LinkId=273769 上有介绍

namespace TsinghuaNet
{
    public sealed partial class Settings :SettingsFlyout
    {
        public Settings()
        {
            this.InitializeComponent();
            
            switch(MainPage.Current.RequestedTheme)
            {
            case ElementTheme.Dark:
                comboBoxTheme.SelectedIndex = 1;
                break;
            case ElementTheme.Light:
                comboBoxTheme.SelectedIndex = 2;
                break;
            default:
                comboBoxTheme.SelectedIndex = 0;
                break;
            }
        }

        private void comboBoxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch(comboBoxTheme.SelectedIndex)
            {
            case 0:
                MainPage.Current.RequestedTheme = ElementTheme.Default;
                break;
            case 1:
                MainPage.Current.RequestedTheme = ElementTheme.Dark;
                break;
            case 2:
                MainPage.Current.RequestedTheme = ElementTheme.Light;
                break;
            default:
                break;
            }
        }
    }
}
