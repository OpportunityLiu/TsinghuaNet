﻿using System;
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
        }

        private bool autoLogOn;

        private async void Page_Loaded(object sender, RoutedEventArgs args)
        {
            //初始化信息存储区
            try
            {
                var passVault = new Windows.Security.Credentials.PasswordVault();
                var pass = passVault.FindAllByResource("TsinghuaAllInOne").First();
                //已经添加字段
                WebConnect.Current = new WebConnect(pass);
                //准备磁贴更新
                WebConnect.Current.PropertyChanged += NotificationService.NotificationService.UpdateTile;
            }
            // 未找到储存的密码
            catch(Exception ex) when (ex.HResult == -2147023728)
            {
                changeUser_Click(null, null);
                return;
            }

            this.DataContext = WebConnect.Current;
            try
            {
                await WebConnect.Current.LoadCache();
                this.progressBarUsage.Value = WebConnect.Current.WebTrafficExact.TotalGB;
            }
            catch(Exception)
            {
            }
            SettingsFlyout_SettingsChanged(null, "AutoLogOn");
            refresh(autoLogOn);
            App.Current.Resuming += (s, e) => refresh(autoLogOn);
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
                    sendHint("下线成功");
                else
                    sendHint("下线失败，请刷新后再试");
                refresh(false);
            }
        }

        private IAsyncAction currentAction;

        public void refresh(bool logOn = true)
        {
            var current = WebConnect.Current;
            this.DataContext = current;
            if(current == null)
                return;
            currentAction?.Cancel();
            this.currentAction = Run(async token =>
            {
                //防止进度条闪烁
                var progressTokens = new CancellationTokenSource();
                var progressToken = progressTokens.Token;
                var progress = Task.Delay(1000, progressToken).ContinueWith(async task =>
                {
                    if(!progressToken.IsCancellationRequested && currentAction?.Status != AsyncStatus.Completed)
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            if(!progressToken.IsCancellationRequested && currentAction?.Status != AsyncStatus.Completed)
                                this.progressBarUsage.IsIndeterminate = true;
                        });
                }, progressToken);
                IAsyncInfo action = null;
                token.Register(() =>
                {
                    progressTokens.Cancel();
                    action?.Cancel();
                });
                if(logOn)
                {
                    action = current.LogOnAsync();
                    try
                    {
                        if(await (IAsyncOperation<bool>)action)
                            sendHint(LocalizedStrings.Resources.ToastSuccess);
                    }
                    catch(LogOnException ex)
                    {
                        sendHint(ex.Message);
                    }
                }
                action = current.RefreshAsync();
                try
                {
                    await (IAsyncAction)action;
                }
                catch(LogOnException ex)
                {
                    sendHint(ex.Message);
                }
                progressTokens.Cancel();
                this.progressBarUsage.IsIndeterminate = false;
                this.progressBarUsage.Value = current.WebTrafficExact.TotalGB;
            });
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

        private void TextBlock_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if(e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                return;
            var s = (FrameworkElement)sender;
            selectedDevice = (WebDevice)s.DataContext;
            var p = e.GetPosition(s);
            p.Y = s.ActualHeight;
            ((MenuFlyout)FlyoutBase.GetAttachedFlyout(s)).ShowAt(s, p);
        }

        private void TextBlock_Holding(object sender, HoldingRoutedEventArgs e)
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

        private async void Hyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9NBLGGGZ5Q4J"));
        }

        private async void appBarButtonSites_Click(object sender, RoutedEventArgs e)
        {
            await WebPage.Launch();
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
                return;
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
}
