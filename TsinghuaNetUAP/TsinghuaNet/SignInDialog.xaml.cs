using System;
using System.Linq;
using Web;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Settings;

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
            switch(args.Result)
            {
            case ContentDialogResult.None:
                args.Cancel = true;
                break;
            case ContentDialogResult.Primary:
                //验证
                var d = args.GetDeferral();
                if(!await SignIn())
                    args.Cancel = true;
                d.Complete();
                break;
            case ContentDialogResult.Secondary:
                //当前未登录且点击取消
                if(WebConnect.Current == null)
                    App.Current.Exit();
                break;
            }
        }

        private async System.Threading.Tasks.Task<bool> SignIn()
        {
            textBlockHint.Text = "";
            var userName = textBoxUserName.Text;
            if(string.IsNullOrEmpty(userName) || userName.All(c => char.IsDigit(c)))
            {
                textBoxUserName.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                textBlockHint.Text = LocalizedStrings.Errors.EmptyUserName;
                return false;
            }
            var password = passwordBoxPassword.Password;
            if(string.IsNullOrEmpty(password))
            {
                passwordBoxPassword.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                textBlockHint.Text = LocalizedStrings.Errors.EmptyPassword;
                return false;
            }
            progressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            try
            {
                if(!await WebConnect.CheckAccount(userName, password))
                {
                    passwordBoxPassword.SelectAll();
                    passwordBoxPassword.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                    progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    textBlockHint.Text = LocalizedStrings.Errors.AuthError;
                    return false;
                }
            }
            catch(Exception)
            {
                textBlockHint.Text =LocalizedStrings.Errors.ConnectError;
                progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                return false;
            }
            var account = AccountManager.CreateAccount(userName, password);
            var connect = new WebConnect(account);
            AccountManager.Account = account;
            WebConnect.Current = connect;
            WebConnect.Current.PropertyChanged += NotificationService.NotificationService.UpdateTile;
            return true;
        }

        private void textChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
        }

        private void ContentDialog_Loading(Windows.UI.Xaml.FrameworkElement sender, object args)
        {
            textBlockHint.Text = "";
            progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            textBoxUserName.Text = "";
            passwordBoxPassword.Password = "";
        }
    }
}
