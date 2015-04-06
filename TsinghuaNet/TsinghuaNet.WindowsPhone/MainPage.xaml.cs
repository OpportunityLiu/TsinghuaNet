using TsinghuaNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using TsinghuaNet.Web;
using Windows.UI.Input;
using Windows.UI.Popups;

// “基本页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace TsinghuaNet
{
    /// <summary>
    /// 可独立使用或用于导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static readonly string logOnFailed = (string)App.Current.Resources["StringLogOnFailed"];

        public MainPage()
        {
            this.InitializeComponent();
            this.dropDialog = initDropDialog();
            this.renameDialog = initRenameDialog(out textBoxRename);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.NavigationMode == NavigationMode.New)
            {
                await App.DispatcherRunAnsyc(async () =>
                {
                    App.Current.StatusBar.BackgroundColor = (Windows.UI.Color)App.Current.Resources["StatusBarColor"];
                    if(new AccessibilitySettings().HighContrast)
                        App.Current.StatusBar.BackgroundOpacity = 0;
                    else
                        App.Current.StatusBar.BackgroundOpacity = 1;
                    if(WebConnect.Current == null)
                    {
                        await new SignInDialog().ShowAsync();
                        this.DataContext = WebConnect.Current;
                    }
                    else
                    {
                        this.DataContext = WebConnect.Current;
                        try
                        {
                            await WebConnect.Current.LogOnAsync();
                        }
                        catch(LogOnException ex)
                        {
                            App.Current.SendToastNotification(logOnFailed, ex.Message);
                        }
                    }
                });
            }
        }

        private WebDevice selectedDevice;

        private async void drop_Confirmed(IUICommand sender)
        {
            await selectedDevice.DropAsync();
            await WebConnect.Current.RefreshAsync();
        }

        private void StackPanel_Holding(object sender, HoldingRoutedEventArgs e)
        {
            switch(e.HoldingState)
            {
                case HoldingState.Canceled:
                    FlyoutBase.GetAttachedFlyout((FrameworkElement)sender).Hide();
                    break;
                case HoldingState.Started:
                    var s = (FrameworkElement)sender;
                    selectedDevice = (WebDevice)s.DataContext;
                    FlyoutBase.ShowAttachedFlyout(s);
                    break;
            }
        }

        MessageDialog dropDialog;

        private MessageDialog initDropDialog()
        {
            var dialog = new MessageDialog((string)App.Current.Resources["StringDropHint"]);
            dialog.Commands.Add(new UICommand((string)App.Current.Resources["StringOk"], drop_Confirmed));
            dialog.Commands.Add(new UICommand((string)App.Current.Resources["StringCancel"]));
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            return dialog;
        }

        ContentDialog renameDialog;

        TextBox textBoxRename;

        private ContentDialog initRenameDialog(out TextBox textBox)
        {
            textBox=new TextBox();
            textBox.PlaceholderText = (string)App.Current.Resources["StringNewName"];
            var dialog = new ContentDialog();
            dialog.PrimaryButtonText=(string)App.Current.Resources["StringOk"];
            dialog.SecondaryButtonText=(string)App.Current.Resources["StringCancel"];
            dialog.Content = textBox;
            dialog.PrimaryButtonClick += (sender, args) =>
            {
                selectedDevice.Name = textBoxRename.Text;
            };
            dialog.Loaded += (sender, args) =>
            {
                textBoxRename.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                textBoxRename.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                textBoxRename.SelectAll();
            };
            return dialog;
        }

        private async void Rename_Click(object sender, RoutedEventArgs e)
        {
            textBoxRename.Text = selectedDevice.Name;
            renameDialog.Title = selectedDevice.Name;
            await renameDialog.ShowAsync();
        }

        private async void Drop_Click(object sender, RoutedEventArgs e)
        {
            dropDialog.Title = selectedDevice.Name;
            await dropDialog.ShowAsync();
        }

        private void appBarButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AboutPage));
        }

        private async void changeUser_Click(object sender, RoutedEventArgs e)
        {
            commandBar.IsOpen = false;
            var signIn = new SignInDialog();
            var t = signIn.ShowAsync();
            signIn.Closed += (s, args) => this.DataContext = WebConnect.Current;
            await t;
        }

        private async void refresh_Click(object sender, RoutedEventArgs e)
        {
            commandBar.IsOpen = false;
            try
            {
                await WebConnect.Current.RefreshAsync();
                if(!WebConnect.Current.IsOnline)
                    try
                    {
                        await WebConnect.Current.LogOnAsync();
                    }
                    catch(LogOnException ex)
                    {
                        App.Current.SendToastNotification(logOnFailed, ex.Message);
                    }
            }
            catch(LogOnException)
            {
            }
        }
    }
}
