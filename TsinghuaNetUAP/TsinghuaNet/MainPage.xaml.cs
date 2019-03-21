using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Web;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

// “基本页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace TsinghuaNet
{
    /// <summary>
    /// 可独立使用或用于导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current { get; private set; }

        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
        }

        private bool autoLogOn;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.SettingsFlyout_SettingsChanged(null, "AutoLogOn");
            if (e.NavigationMode == NavigationMode.New)
            {
                var prelaunch = (bool)e.Parameter;
                var account  = Settings.AccountManager.Load();
                if (account is null)
                {
                    // 未找到储存的密码
                    this.changeUser_Click(null, null);
                }
                else
                {
                    WebConnect.Current = new WebConnect(account.Item1, account.Item2);
                    //准备磁贴更新
                    WebConnect.Current.PropertyChanged += NotificationService.NotificationService.UpdateTile;
                }

                if (WebConnect.Current != null)
                {
                    this.DataContext = WebConnect.Current;
                    try
                    {
                        await WebConnect.Current.LoadCache();
                        this.progressBarUsage.Value = WebConnect.Current.WebTrafficExact.TotalGB;
                    }
                    catch (Exception)
                    {
                    }
                    if (!prelaunch)
                        this.refresh(this.autoLogOn);
                }
                App.Current.Resuming += (s, args) => this.refresh(this.autoLogOn);
            }
            else
            {
                if (WebConnect.Current != null)
                {
                    this.refresh(this.autoLogOn);
                }
            }
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (WebConnect.Current != null)
            {
                await WebConnect.Current.SaveCache();
            }
        }

        private DropDialog dropDialog;
        private RenameDialog renameDialog;
        private SignInDialog signInDialog;
        private SettingsDialog settingsDialog;
        private LogOnDialog logOnDialog;

        private async void Rename_Click(object sender, RoutedEventArgs e)
        {
            var selectedDevice = (WebDevice)((FrameworkElement)sender).DataContext;
            var renameDialog = LazyInitializer.EnsureInitialized(ref this.renameDialog);
            renameDialog.NewName = selectedDevice.Name;
            await renameDialog.ShowAsync();
            if (renameDialog.ChangeName)
            {
                selectedDevice.Name = renameDialog.NewName;
            }
        }

        private async void Drop_Click(object sender, RoutedEventArgs e)
        {
            var dropDialog = LazyInitializer.EnsureInitialized(ref this.dropDialog);
            var selectedDevice = (WebDevice)((FrameworkElement)sender).DataContext;
            dropDialog.Title = selectedDevice.Name;
            if (await dropDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                if (await selectedDevice.DropAsync())
                    this.SendHint(LocalizedStrings.Resources.DropSuccess);
                else
                    this.SendHint(LocalizedStrings.Resources.DropFailed);
                this.refresh(false);
            }
        }

        public async void refresh(bool logOn)
        {
            var current = WebConnect.Current;
            this.DataContext = current;
            if (current == null)
                return;
            this.progressBarUsage.IsIndeterminate = true;
            try
            {
                try
                {
                    if (logOn && await current.LogOnAsync())
                        this.SendHint(LocalizedStrings.Resources.ToastSuccess);
                }
                catch (LogOnException ex)
                {
                    this.SendHint(ex.Message);
                }
                try
                {
                    await current.RefreshAsync();
                }
                catch (LogOnException ex)
                {
                    this.SendHint(ex.Message);
                }
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
                s.Closed += (_, args) => this.refresh(this.autoLogOn);
                return s;
            });
            await signIn.ShowAsync();
        }

        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            this.refresh(this.autoLogOn);
        }

        private void appBarButtonSites_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoForward)
            {
                this.Frame.GoForward();
            }
            else
            {
                this.Frame.Navigate(typeof(WebPage));
            }
        }

        private Queue<string> hintQueue = new Queue<string>();

        public void SendHint(string message)
        {
            if (this.textBlockHint == null)
                this.FindName("borderHint");
            this.hintQueue.Enqueue(message ?? "");
            if (this.showHint.GetCurrentState() != Windows.UI.Xaml.Media.Animation.ClockState.Active)
            {
                this.showHint_Completed(this.showHint, null);
            }
            else
            {
                this.showHint.SpeedRatio = 3;
            }
        }

        private void showHint_Completed(object sender, object e)
        {
            if (this.hintQueue.Count <= 1)
                this.showHint.SpeedRatio = 1;
            else
                this.showHint.SpeedRatio = 3;
            if (this.hintQueue.Count == 0)
            {
                this.textBlockHint.Text = "";
                return;
            }
            this.textBlockHint.Text = this.hintQueue.Dequeue();
            this.showHint.Begin();
        }

        private void Flyout_Opening(object sender, object e)
        {
            this.FindName("textBlockAbout");
            var version = Package.Current.Id.Version;
            this.runVersion.Text = string.Format(CultureInfo.CurrentCulture, LocalizedStrings.Resources.AppVersionFormat, version.Major, version.Minor, version.Build, version.Revision);
        }

        private void appBarButtonLogOn_Click(object sender, RoutedEventArgs e)
        {
            this.refresh(true);
        }

        private void SettingsFlyout_SettingsChanged(object sender, string e)
        {
            if (e == "AutoLogOn")
            {
                this.autoLogOn = Settings.SettingsHelper.GetLocal("AutoLogOn", true);
                if (this.autoLogOn)
                    this.appBarButtonLogOn.Visibility = Visibility.Collapsed;
                else
                    this.appBarButtonLogOn.Visibility = Visibility.Visible;
            }
        }

        private async void ReviewLink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9NBLGGGZ5Q4J"));
        }

        private async void appBarButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            if (this.settingsDialog == null)
            {
                this.settingsDialog = new SettingsDialog();
                this.settingsDialog.SettingsChanged += this.SettingsFlyout_SettingsChanged;
            }
            await this.settingsDialog.ShowAsync();
        }

        private void listViewOnlineDevices_ItemClick(object sender, ItemClickEventArgs e)
        {
            var c = (ListViewItem)this.listViewOnlineDevices.ContainerFromItem(e.ClickedItem);
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)c.ContentTemplateRoot);
        }

        private async void appBarButtonLogOnOther_Click(object sender, RoutedEventArgs e)
        {
            if (this.logOnDialog == null)
            {
                this.logOnDialog = new LogOnDialog();
            }
            await this.logOnDialog.ShowAsync();
        }
    }

    internal class NumberVisbilityConverter : Windows.UI.Xaml.Data.IValueConverter
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

    internal class DeviceImageConverter : Windows.UI.Xaml.Data.IValueConverter
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
