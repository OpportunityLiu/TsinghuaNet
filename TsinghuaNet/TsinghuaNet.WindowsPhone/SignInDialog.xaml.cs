using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TsinghuaNet.Web;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers.Provider;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace TsinghuaNet
{
    public sealed partial class SignInDialog : ContentDialog
    {
        public SignInDialog()
        {
            this.InitializeComponent();
        }

        MessageDialog emptyUserName = new MessageDialog((string)App.Current.Resources["StringErrorUserName"], (string)App.Current.Resources["StringError"]);
        MessageDialog emptyPassword = new MessageDialog((string)App.Current.Resources["StringErrorPassword"], (string)App.Current.Resources["StringError"]);

        private async void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            switch(args.Result)
            {
                case ContentDialogResult.None:
                    args.Cancel = true;
                    break;
                case ContentDialogResult.Primary:
                    if(!await SignIn())
                        await sender.ShowAsync();
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
                await WebConnect.Current.RefreshUsageAnsyc();
                return true;
            }
            else
            {
                await new MessageDialog(excep.Message, (string)App.Current.Resources["StringError"]).ShowAsync();
                switch(excep.ExceptionType)
                {
                    case LogOnExceptionType.UserNameError:
                        textBoxUserName.Text = "";
                        passwordBoxPassword.Password = "";
                        break;
                    case LogOnExceptionType.PasswordError:
                        passwordBoxPassword.Password = "";
                        break;
                    default:
                        break;
                }
                return false;
            }
        }

        private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if(new Windows.UI.ViewManagement.AccessibilitySettings().HighContrast)
                App.Current.StatusBar.BackgroundOpacity = 0;
            else
                App.Current.StatusBar.BackgroundOpacity = 1;
        }
    }
}
