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
            var userName = textBoxUserName.Text;
            if(string.IsNullOrEmpty(userName))
            {
                textBoxUserName.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                textBlockHint.Text = LocalizedStrings.EmptyUserName;
                return false;
            }
            var password = passwordBoxPassword.Password;
            if(string.IsNullOrEmpty(password))
            {
                passwordBoxPassword.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                textBlockHint.Text = LocalizedStrings.EmptyPassword;
                return false;
            }
            try
            {
                progressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                var connect = new WebConnect(userName, password);
                await connect.RefreshAsync();
                var passVault = new Windows.Security.Credentials.PasswordVault();
                try
                {
                    var oldPass = passVault.FindAllByResource("TsinghuaAllInOne").First();
                    passVault.Remove(oldPass);
                }
                // 未找到储存的密码
                catch(Exception ex) when (ex.HResult == -2147023728)
                {
                }
                var pass = new Windows.Security.Credentials.PasswordCredential("TsinghuaAllInOne", userName, password);
                passVault.Add(pass);

                WebConnect.Current = connect;
                WebConnect.Current.PropertyChanged += async (sender, e) =>
                {
                    if(e.PropertyName != nameof(WebConnect.UpdateTime))
                        return;
                    await TileUpdater.Updater.UpdateTile((WebConnect)sender);
                };
                return true;
            }
            catch(LogOnException excep)
            {
                textBlockHint.Text = excep.Message;
                switch(excep.ExceptionType)
                {
                case LogOnExceptionType.UserNameError:
                    textBoxUserName.SelectAll();
                    textBoxUserName.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                    break;
                case LogOnExceptionType.PasswordError:
                    passwordBoxPassword.SelectAll();
                    passwordBoxPassword.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                    break;
                default:
                    break;
                }
                progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                return false;
            }
        }

        private void textChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            textBlockHint.Text = "";
        }
    }
}
