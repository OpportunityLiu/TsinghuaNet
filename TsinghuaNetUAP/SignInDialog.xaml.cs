using System;
using TsinghuaNet.Web;
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
            var resources = ResourceLoader.GetForCurrentView();
            error = resources.GetString("Error");
            emptyUserName = new MessageDialog(resources.GetString("EmptyUserName"), error);
            emptyPassword = new MessageDialog(resources.GetString("EmptyPassword"), error);
        }

        MessageDialog emptyUserName;
        MessageDialog emptyPassword;
        string error;

        private async void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            switch(args.Result)
            {
            case ContentDialogResult.None:
                args.Cancel = true;
                break;
            case ContentDialogResult.Primary:
                var d = args.GetDeferral();
                if(!await SignIn())
                    args.Cancel = true;
                d.Complete();
                break;
            case ContentDialogResult.Secondary:
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
                await emptyUserName.ShowAsync();
                return false;
            }
            var password = passwordBoxPassword.Password;
            if(string.IsNullOrEmpty(password))
            {
                passwordBoxPassword.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                await emptyPassword.ShowAsync();
                return false;
            }
            var passMD5 = MD5.MDString(password);
            LogOnException excep = null;
            try
            {
                WebConnect.Current = new WebConnect(userName, passMD5);
                await WebConnect.Current.RefreshAsync();
            }
            catch(LogOnException ex)
            {
                excep = ex;
            }
            if(excep == null)
            {
                ApplicationData.Current.RoamingSettings.Values["UserName"] = userName;
                ApplicationData.Current.RoamingSettings.Values["PasswordMD5"] = passMD5;
                return true;
            }
            else
            {
                await new MessageDialog(excep.Message, error).ShowAsync();
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
                return false;
            }
        }
    }
}
