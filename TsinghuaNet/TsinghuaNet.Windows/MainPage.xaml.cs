using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.Threading.Tasks;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

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
            textboxUserName.Text = (string)ApplicationData.Current.LocalSettings.Values["Username"];
            if(string.IsNullOrEmpty((string)ApplicationData.Current.LocalSettings.Values["PasswordMD5"]))
            {
                passwordboxPassword.Password = new string('*', (int)ApplicationData.Current.LocalSettings.Values["PasswordLength"]);
                useSavedPassword = true;
            }
            this.sharedUI = (App.Current as App).SharedUI;
            if(sharedUI.Connect != null)
                listviewDevices.ItemsSource = sharedUI.Connect.DeviceList;
            //((ColumnSeries)this.MixedChart.Series[0]).ItemsSource = sharedUI.Connect.GetUsageAnsyc().Result.traffic;
        }

        private SharedUI sharedUI;

        private bool useSavedPassword;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    if(sharedUI.Connect == null)
                    {
                        this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            ApplicationData.Current.LocalSettings.Values["UserName"] = textboxUserName.Text;
                            ApplicationData.Current.LocalSettings.Values["PasswordLength"] = passwordboxPassword.Password.Length;
                            ApplicationData.Current.LocalSettings.Values["PasswordMD5"] = MD5.MDString(passwordboxPassword.Password);
                            sharedUI.Connect = new WebConnect((string)ApplicationData.Current.LocalSettings.Values["UserName"], (string)ApplicationData.Current.LocalSettings.Values["PasswordMD5"]);
                            listviewDevices.ItemsSource = sharedUI.Connect.DeviceList;
                        }).AsTask().Wait();
                    }
                    sharedUI.Connect.LogOnAsync().Wait();
                    sharedUI.Connect.RefreshAsync().Wait();
                    sharedUI.SendToastNotification("登陆成功", "已用流量：", sharedUI.Connect.WebTrafficExact.ToString());
                }
                catch(AggregateException ex)
                {
                    this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => new Windows.UI.Popups.MessageDialog(ex.InnerException.Message, "登陆错误").ShowAsync());
                }
                catch(ArgumentException ex)
                {
                    this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => new Windows.UI.Popups.MessageDialog(ex.Message, "登陆错误").ShowAsync());
                }
            });
        }

        private void passwordboxPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            useSavedPassword = false;
        }

        private void passwordboxPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            if(useSavedPassword)
            {
                ((PasswordBox)sender).SelectAll();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var btnok = (Button)sender;
            var btn = (Button)btnok.Tag;
            btn.Flyout.Hide();
            var newname = ((btnok.Parent as StackPanel).Children[0] as TextBox).Text;
            var device = (WebDevice)btn.Tag;
            try
            {
                device.Name = newname;
            }
            catch(InvalidOperationException)
            {
            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var btn = (sender as Button).Tag as Button;
            btn.Flyout.Hide();
            (btn.Tag as WebDevice).DropAsync();
            sharedUI.Connect.RefreshAsync();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if(sharedUI.Connect != null)
                sharedUI.Connect.RefreshAsync();
        }

        private void test(object sender, RoutedEventArgs e)
        {
            var l = sharedUI.Connect.GetUsageAnsyc().Result;
            
        }
    }
}
