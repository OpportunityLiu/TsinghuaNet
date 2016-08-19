using System;
using System.Threading.Tasks;
using Web;
using Windows.ApplicationModel.Resources;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.ApplicationModel;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using Windows.System;
using Windows.UI.ViewManagement;
using System.Threading;
using System.Linq;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// “基本页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace TsinghuaNet
{
    /// <summary>
    /// 可独立使用或用于导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent("MainPage created.");
        }

        private bool autoLogOn;// = Settings.SettingsHelper.GetLocal("AutoLogOn", false);

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SettingsFlyout_SettingsChanged(null, "AutoLogOn");
            if(e.NavigationMode == NavigationMode.New)
            {
                var prelaunch = (bool)e.Parameter;
                var account = Settings.AccountManager.Account;
                if(account == null)
                {
                    // 未找到储存的密码
                    changeUser_Click(null, null);
                }
                else
                {
                    WebConnect.Current = new WebConnect(account);
                    //准备磁贴更新
                    WebConnect.Current.PropertyChanged += NotificationService.NotificationService.UpdateTile;
                }

                if(WebConnect.Current != null)
                {
                    this.DataContext = WebConnect.Current;
                    try
                    {
                        await WebConnect.Current.LoadCache();
                        this.progressBarUsage.Value = WebConnect.Current.WebTrafficExact.TotalGB;
                    }
                    catch(Exception)
                    {
                    }
                    if(!prelaunch)
                        refresh(autoLogOn);
                }
                App.Current.Resuming += (s, args) => refresh(this.autoLogOn);
            }
            else
            {
                if(WebConnect.Current != null)
                {
                    refresh(autoLogOn);
                }
            }
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if(WebConnect.Current != null)
            {
                await WebConnect.Current.SaveCache();
            }
        }

        private WebDevice selectedDevice;

        DropDialog dropDialog;

        RenameDialog renameDialog;

        SignInDialog signInDialog;

        private async void Rename_Click(object sender, RoutedEventArgs e)
        {
            var renameDialog = LazyInitializer.EnsureInitialized(ref this.renameDialog);
            renameDialog.NewName = selectedDevice.Name;
            if(await renameDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                selectedDevice.Name = renameDialog.NewName;
            }
        }

        private async void Drop_Click(object sender, RoutedEventArgs e)
        {
            var dropDialog = LazyInitializer.EnsureInitialized(ref this.dropDialog);
            dropDialog.Title = selectedDevice.Name;
            if(await dropDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                if(await selectedDevice.DropAsync())
                    sendHint(LocalizedStrings.Resources.DropSuccess);
                else
                    sendHint(LocalizedStrings.Resources.DropFailed);
                refresh(false);
            }
        }

        public async void refresh(bool logOn)
        {
            var current = WebConnect.Current;
            this.DataContext = current;
            if(current == null)
                return;
            this.progressBarUsage.IsIndeterminate = true;
            try
            {
                if(logOn && await current.LogOnAsync())
                    sendHint(LocalizedStrings.Resources.ToastSuccess);
                await current.RefreshAsync();
            }
            catch(LogOnException ex)
            {
                sendHint(ex.Message);
            }
            finally
            {
                this.progressBarUsage.IsIndeterminate = false;
                this.progressBarUsage.Value = current.WebTrafficExact.TotalGB;
            }
        }

        private async void changeUser_Click(object sender, RoutedEventArgs e)
        {
            var signIn = LazyInitializer.EnsureInitialized(ref this.signInDialog, () =>
            {
                var s = new SignInDialog();
                s.Closed += (_, args) => refresh(autoLogOn);
                return s;
            });
            await signIn.ShowAsync();
        }

        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            refresh(autoLogOn);
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if(e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                return;
            var s = (FrameworkElement)sender;
            selectedDevice = (WebDevice)s.DataContext;
            var p = e.GetPosition(s);
            p.Y = s.ActualHeight;
            ((MenuFlyout)FlyoutBase.GetAttachedFlyout(s)).ShowAt(s, p);
        }

        private void Grid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            switch(e.HoldingState)
            {
            case HoldingState.Canceled:
                FlyoutBase.GetAttachedFlyout((FrameworkElement)sender).Hide();
                break;
            case HoldingState.Started:
                var s = (FrameworkElement)sender;
                selectedDevice = (WebDevice)s.DataContext;
                var p = e.GetPosition(s);
                p.X = p.X > 100 ? p.X - 100 : 0;
                p.Y = s.ActualHeight;
                ((MenuFlyout)FlyoutBase.GetAttachedFlyout(s)).ShowAt(s, p);
                break;
            }
        }

        private void appBarButtonSites_Click(object sender, RoutedEventArgs e)
        {
            if(Frame.CanGoForward)
            {
                Frame.GoForward();
            }
            else
            {
                Frame.Navigate(typeof(WebPage));
            }
        }

        private Queue<string> hintQueue = new Queue<string>();

        private void sendHint(string message)
        {
            if(textBlockHint == null)
                FindName("borderHint");
            hintQueue.Enqueue(message ?? "");
            if(showHint.GetCurrentState() != Windows.UI.Xaml.Media.Animation.ClockState.Active)
            {
                showHint_Completed(showHint, null);
            }
            else
            {
                showHint.SpeedRatio = 3;
            }
        }

        private void showHint_Completed(object sender, object e)
        {
            if(hintQueue.Count <= 1)
                showHint.SpeedRatio = 1;
            else
                showHint.SpeedRatio = 3;
            if(hintQueue.Count == 0)
            {
                textBlockHint.Text = "";
                return;
            }
            textBlockHint.Text = hintQueue.Dequeue();
            showHint.Begin();
        }

        private void Flyout_Opening(object sender, object e)
        {
            FindName("textBlockAbout");
            var version = Package.Current.Id.Version;
            runVersion.Text = string.Format(CultureInfo.CurrentCulture, LocalizedStrings.Resources.AppVersionFormat, version.Major, version.Minor, version.Build, version.Revision);
        }

        private void appBarButtonLogOn_Click(object sender, RoutedEventArgs e)
        {
            refresh(true);
        }

        private void SettingsFlyout_SettingsChanged(object sender, string e)
        {
            if(e == "AutoLogOn")
            {
                autoLogOn = Settings.SettingsHelper.GetLocal("AutoLogOn", true);
                if(autoLogOn)
                    appBarButtonLogOn.Visibility = Visibility.Collapsed;
                else
                    appBarButtonLogOn.Visibility = Visibility.Visible;
            }
        }

        private async void ReviewLink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9NBLGGGZ5Q4J"));
        }
    }

    class NumberVisbilityConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((int)value) > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class DeviceImageConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var va = (DeviceFamily)value;
            return new BitmapImage(new Uri($"ms-appx:///Images/{va.ToString()}-{App.Current.RequestedTheme.ToString()}.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
