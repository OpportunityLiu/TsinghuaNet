using System;
using System.Threading.Tasks;
using TsinghuaNet.Web;
using Windows.ApplicationModel.Resources;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// “基本页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace TsinghuaNet
{
    /// <summary>
    /// 可独立使用或用于导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static readonly string logOnFailed = ResourceLoader.GetForViewIndependentUse().GetString("ToastFailed");

        public MainPage()
        {
            var resource = ResourceLoader.GetForCurrentView();
            this.InitializeComponent();
            this.dropDialog = new MessageDialog(resource.GetString("DropHintText"));
            dropDialog.Commands.Add(new UICommand(resource.GetString("Ok"), drop_Confirmed));
            dropDialog.Commands.Add(new UICommand(resource.GetString("Cancel")));
            dropDialog.DefaultCommandIndex = 0;
            dropDialog.CancelCommandIndex = 1;
            this.renameDialog = new RenameDialog();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.NavigationMode == NavigationMode.New)
            {
                await Task.Delay(500);
                if(WebConnect.Current == null)
                {
                    await new SignInDialog().ShowAsync();
                    this.DataContext = WebConnect.Current;
                }
                else
                {
                    this.DataContext = WebConnect.Current;
                }
            }
        }

        private WebDevice selectedDevice;

        private async void drop_Confirmed(IUICommand sender)
        {
            await selectedDevice.DropAsync();
            try
            {
                await WebConnect.Current.RefreshAsync();
            }
            catch(LogOnException)
            {
            }
        }

        MessageDialog dropDialog;

        RenameDialog renameDialog;

        private async void Rename_Click(object sender, RoutedEventArgs e)
        {
            renameDialog.NewName = selectedDevice.Name;
            if(await renameDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                selectedDevice.Name = renameDialog.NewName;
            }
        }

        private async void Drop_Click(object sender, RoutedEventArgs e)
        {
            dropDialog.Title = selectedDevice.Name;
            await dropDialog.ShowAsync();
        }

        private void appBarButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            //this.Frame.Navigate(typeof(AboutPage));
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
                await WebConnect.Current.LogOnAsync();
            }
            catch(LogOnException ex)
            {
                App.Current.SendToastNotification(logOnFailed, ex.Message);
            }
            try
            {
                await WebConnect.Current.RefreshAsync();
            }
            catch(LogOnException)
            {
            }
        }

        private void StackPanel_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if(e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                return;
            var s = (FrameworkElement)sender;
            selectedDevice = (WebDevice)s.DataContext;
            FlyoutBase.ShowAttachedFlyout(s);
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
    }
}
