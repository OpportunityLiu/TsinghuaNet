using System.Globalization;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace TsinghuaNet
{
    public sealed partial class AboutDialog : ContentDialog
    {
        public AboutDialog()
        {
            this.InitializeComponent();
            var version = Package.Current.Id.Version;
            textBlockVersion.Text = string.Format(CultureInfo.CurrentCulture, LocalizedStrings.AppVersionFormat, version.Major, version.Minor, version.Build, version.Revision);
        }
    }
}
