using System;
using System.Text.RegularExpressions;
using Web;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace TsinghuaNet
{
    public sealed partial class LogOnDialog : ContentDialog
    {
        public LogOnDialog()
        {
            this.InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await logOn();
        }

        private async System.Threading.Tasks.Task logOn()
        {
            var connect = WebConnect.Current;
            if (connect is null)
                return;
            try
            {
                await connect.LogOnAsync(this.tbInput.Text);
                MainPage.Current.SendHint(LocalizedStrings.Resources.ToastSuccess);
                await connect.RefreshAsync();
            }
            catch (Exception ex)
            {
                MainPage.Current.SendHint(ex.Message);
            }
        }

        private bool isValidIp(string ip)
        {
            return Regex.IsMatch(ip, @"^\s*\b(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\b\s*$");
        }

        private void ContentDialog_Loading(Windows.UI.Xaml.FrameworkElement sender, object args)
        {
            this.tbInput.Text = "";
        }

        private async void tbInput_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (!IsPrimaryButtonEnabled)
                return;
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                Hide();
                await logOn();
            }
        }
    }
}
