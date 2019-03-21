using Settings;
using System;
using System.Linq;
using Web;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace TsinghuaNet
{
    public sealed partial class SignInDialog : ContentDialog
    {
        public SignInDialog()
        {
            this.InitializeComponent();
        }

        private async void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            switch (args.Result)
            {
            case ContentDialogResult.Primary:
                //验证
                var d = args.GetDeferral();
                if (!await this.SignIn())
                    args.Cancel = true;
                d.Complete();
                break;
            default:
                //当前未登录且点击取消
                if (WebConnect.Current == null)
                    App.Current.Exit();
                break;
            }
        }

        private async System.Threading.Tasks.Task<bool> SignIn()
        {
            this.textBlockHint.Text = "";
            var userName = this.textBoxUserName.Text;
            if (string.IsNullOrEmpty(userName))
            {
                this.textBoxUserName.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                this.textBlockHint.Text = LocalizedStrings.Errors.EmptyUserName;
                return false;
            }
            var password = this.passwordBoxPassword.Password;
            if (string.IsNullOrEmpty(password))
            {
                this.passwordBoxPassword.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                this.textBlockHint.Text = LocalizedStrings.Errors.EmptyPassword;
                return false;
            }
            this.progressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            try
            {
                if (!(await WebConnect.CheckAccount(userName, password) is AccountInfo accountInfo))
                {
                    this.passwordBoxPassword.SelectAll();
                    this.passwordBoxPassword.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                    this.progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.textBlockHint.Text = LocalizedStrings.Errors.AuthError;
                    return false;
                }
                var connect = new WebConnect(accountInfo, password);
                AccountManager.Save(accountInfo, password);
                WebConnect.Current = connect;
                WebConnect.Current.PropertyChanged += NotificationService.NotificationService.UpdateTile;
                return true;
            }
            catch (Exception)
            {
                this.textBlockHint.Text = LocalizedStrings.Errors.ConnectError;
                this.progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                return false;
            }
        }

        private void textChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
        }

        private void ContentDialog_Loading(Windows.UI.Xaml.FrameworkElement sender, object args)
        {
            this.textBlockHint.Text = "";
            this.progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.textBoxUserName.Text = "";
            this.passwordBoxPassword.Password = "";
        }
    }
}
