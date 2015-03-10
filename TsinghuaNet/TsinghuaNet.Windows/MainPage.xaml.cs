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
using Windows.UI.Popups;
using TsinghuaNet.Web;
using TsinghuaNet.Common;

namespace TsinghuaNet
{
    /// <summary>
    /// 可独立使用或用于导航至 Frame 内部的空白页。
    /// </summary>
    public partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
            appBarButtonRename.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            hub.Sections.Remove(hubSectionStart);
            hub.Sections.Remove(hubSectionState);
            hub.Sections.Remove(hubSectionHistory);
        }

        PasswordBox passwordBoxPassword;
        TextBox textBoxUserName;
        MessageDialog emptyUserName = new MessageDialog((string)App.Current.Resources["StringErrorUserName"], (string)App.Current.Resources["StringError"]);
        MessageDialog emptyPassword = new MessageDialog((string)App.Current.Resources["StringErrorPassword"], (string)App.Current.Resources["StringError"]);

        private async void logOn_Click(object sender, RoutedEventArgs e)
        {
            var userName = textBoxUserName.Text;
            if(string.IsNullOrEmpty(userName))
            {
                textBoxUserName.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                await emptyUserName.ShowAsync();
                return;
            }
            var password = passwordBoxPassword.Password;
            if(string.IsNullOrEmpty(password))
            {
                passwordBoxPassword.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                await emptyPassword.ShowAsync();
                return;
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
                hub.Sections.Remove(hubSectionStart);
                hub.DataContext = WebConnect.Current;
                hub.Sections.Add(hubSectionState);
                hub.Sections.Add(hubSectionHistory);
                appBarButtonChangeUser.Visibility = Windows.UI.Xaml.Visibility.Visible;
                ApplicationData.Current.RoamingSettings.Values["UserName"] = userName;
                ApplicationData.Current.RoamingSettings.Values["PasswordMD5"] = passMD5;
                await WebConnect.Current.RefreshUsageAnsyc();
            }
            else
            {
                await new MessageDialog(excep.Message, (string)App.Current.Resources["StringError"]).ShowAsync();
                switch(excep.ExceptionType)
                {
                    case LogOnExceptionType.UserNameError:
                        textBoxUserName.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                        textBoxUserName.SelectAll();
                        break;
                    case LogOnExceptionType.PasswordError:
                        passwordBoxPassword.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                        passwordBoxPassword.SelectAll();
                        break;
                    default:
                        break;
                }
            }
        }

        private void page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            hubSectionPic.Width = e.NewSize.Width - 200;
            if(e.NewSize.Width == 500)
            {
                hubSectionPic.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                hubSectionHistory.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                hubSectionPic.Visibility = Windows.UI.Xaml.Visibility.Visible;
                hubSectionHistory.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        private void changeUser_Click(object sender, RoutedEventArgs e)
        {
            hub.ScrollToSection(hubSectionPic);
            hub.Sections.Remove(hubSectionState);
            hub.Sections.Remove(hubSectionHistory);
            if(textBoxUserName != null)
            {
                textBoxUserName.Text = "";
                passwordBoxPassword.Password = "";
            }
            hub.Sections.Add(hubSectionStart);
            commandBar.IsOpen = false;
            appBarButtonChangeUser.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void rename_Click(object sender, RoutedEventArgs e)
        {
            appBarButtonRename.Flyout.Hide();
            commandBar.IsOpen = false;
            ((WebDevice)listViewOnlineDevices.SelectedItem).Name = textBoxRename.Text;
        }

        private async void drop_Click(object sender, RoutedEventArgs e)
        {
            appBarButtonDrop.Flyout.Hide();
            commandBar.IsOpen = false;
            await ((WebDevice)listViewOnlineDevices.SelectedItem).DropAsync();
            await WebConnect.Current.RefreshAsync();
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
                        App.Current.SendToastNotification("登陆失败", ex.Message);
                    }
            }
            catch(LogOnException)
            {
            }
        }

        ListView listViewOnlineDevices;

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listViewOnlineDevices = (ListView)sender;
            if(listViewOnlineDevices.SelectedItem != null)
            {
                appBarButtonRename.Visibility = Windows.UI.Xaml.Visibility.Visible;
                appBarButtonRename.DataContext = listViewOnlineDevices.SelectedItem;
                BottomAppBar.IsOpen = true;
            }
            else
            {
                appBarButtonRename.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if(textBoxUserName != null)
                return;
            var stackPanel = (FrameworkElement)sender;
            textBoxUserName = (TextBox)stackPanel.FindName("textBoxUserName");
            passwordBoxPassword = (PasswordBox)stackPanel.FindName("passwordBoxPassword");
        }

        private void ColumnSeries_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var selected = ((ColumnSeries)sender).SelectedItem;
            if(selected != null)
                Frame.Navigate(typeof(SingleMonthData), selected);
        }

        private void refreshUsage_Click(object sender, RoutedEventArgs e)
        {
            WebConnect.Current.RefreshUsageAnsyc();
        }

        private void textBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Enter)
                if(string.IsNullOrEmpty(textBoxUserName.Text))
                    textBoxUserName.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                else if(string.IsNullOrEmpty(passwordBoxPassword.Password))
                    passwordBoxPassword.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                else
                    logOn_Click(sender, e);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.NavigationMode == NavigationMode.New)
            {
                await Task.Delay(500);
                await App.DispatcherRunAnsyc(async () =>
                {
                    if(WebConnect.Current == null)
                    {
                        hub.Sections.Add(hubSectionStart);
                        appBarButtonChangeUser.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                    else
                    {
                        hub.DataContext = WebConnect.Current;
                        hub.Sections.Add(hubSectionState);
                        hub.Sections.Add(hubSectionHistory);
                        try
                        {
                            await WebConnect.Current.LogOnAsync();
                            await WebConnect.Current.RefreshUsageAnsyc();
                        }
                        catch(LogOnException ex)
                        {
                            App.Current.SendToastNotification("登陆错误", ex.Message);
                        }
                    }
                });
            }
        }

        private void textBoxRename_Loaded(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void textBoxRename_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                rename_Click(sender, e);
            }
        }
    }
}
