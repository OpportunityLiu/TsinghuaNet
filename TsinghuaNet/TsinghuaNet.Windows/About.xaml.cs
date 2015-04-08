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
using System.Threading.Tasks;
using Windows.ApplicationModel;
using System.Globalization;
using Windows.ApplicationModel.Resources;

//“设置浮出控件”项模板在 http://go.microsoft.com/fwlink/?LinkId=273769 上有介绍

namespace TsinghuaNet
{
    public sealed partial class About : SettingsFlyout
    {
        public About()
        {
            this.InitializeComponent();
            var version = Package.Current.Id.Version;
            textBlockVersion.Text = string.Format(CultureInfo.CurrentCulture, ResourceLoader.GetForCurrentView().GetString("AppVersionFormat"),
version.Major, version.Minor, version.Build, version.Revision);
        }
    }
}
