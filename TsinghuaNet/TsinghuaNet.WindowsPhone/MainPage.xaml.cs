using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;
using TsinghuaNet.Web;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace TsinghuaNet
{
    /// <summary>
    /// 可独立使用或用于导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: 准备此处显示的页面。

            // TODO: 如果您的应用程序包含多个页面，请确保
            // 通过注册以下事件来处理硬件“后退”按钮:
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed 事件。
            // 如果使用由某些模板提供的 NavigationHelper，
            // 则系统会为您处理该事件。
        }

        public MainPage()
        {
            this.InitializeComponent();
            appBarButtonRename.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            pivot.Items.Remove(pivotItemStart);
            pivot.Items.Remove(pivotItemHistory);
            pivot.Items.Remove(pivotItemState);
            if(WebConnect.Current == null)
            {
                pivot.Items.Add(pivotItemStart);
                appBarButtonChangeUser.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                this.DataContext = WebConnect.Current;
                pivot.Items.Add(pivotItemHistory);
                pivot.Items.Add(pivotItemState);
                pivot.SelectedItem = pivotItemState;
                WebConnect.Current.RefreshUsageAnsyc();
            }
        }

        MessageDialog emptyUserName = new MessageDialog((string)App.Current.Resources["StringErrorUserName"], (string)App.Current.Resources["StringError"]);
        MessageDialog emptyPassword = new MessageDialog((string)App.Current.Resources["StringErrorPassword"], (string)App.Current.Resources["StringError"]);

        private async void logOn_Click(object sender, RoutedEventArgs e)
        {
            var userName = textBoxUserName.Text;
            if(string.IsNullOrEmpty(userName))
            {
                await emptyUserName.ShowAsync();
                return;
            }
            var password = passwordBoxPassword.Password;
            if(string.IsNullOrEmpty(password))
            {
                await emptyPassword.ShowAsync();
                return;
            }
            var passMD5 = MD5.MDString(password);
            ApplicationData.Current.LocalSettings.Values["UserName"] = userName;
            ApplicationData.Current.LocalSettings.Values["PasswordMD5"] = passMD5;
            new WebConnect(userName, passMD5);
            var t = WebConnect.Current.RefreshUsageAnsyc();
            pivot.Items.Remove(pivotItemStart);
            pivot.Items.Add(pivotItemHistory);
            pivot.Items.Add(pivotItemState);
            pivot.SelectedItem = pivotItemState;
            this.DataContext = WebConnect.Current;
            appBarButtonChangeUser.Visibility = Windows.UI.Xaml.Visibility.Visible;
            await t;
        }

        private void changeUser_Click(object sender, RoutedEventArgs e)
        {
            pivot.Items.Remove(pivotItemState);
            pivot.Items.Remove(pivotItemHistory);
            pivot.Items.Add(pivotItemStart);
            textBoxUserName.Text = "";
            passwordBoxPassword.Password = "";
            appBarButtonChangeUser.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void rename_Click(object sender, RoutedEventArgs e)
        {
            appBarButtonRename.Flyout.Hide();
            ((WebDevice)listViewOnlineDevices.SelectedItem).Name = textBoxRename.Text;
        }

        private async void drop_Click(object sender, RoutedEventArgs e)
        {
            appBarButtonDrop.Flyout.Hide();
            await ((WebDevice)listViewOnlineDevices.SelectedItem).DropAsync();
            await WebConnect.Current.RefreshAsync();
        }

        private async void refresh_Click(object sender, RoutedEventArgs e)
        {
            //commandBar.IsOpen = false;
            if((DateTime.Now - WebConnect.Current.UpdateTime).Ticks > 100000000)//10秒
                await WebConnect.Current.RefreshAsync();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(listViewOnlineDevices.SelectedItem != null)
            {
                appBarButtonRename.DataContext = listViewOnlineDevices.SelectedItem;
                appBarButtonRename.IsEnabled = true;
            }
        }

        private void BarSeries_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var selected = ((BarSeries)sender).SelectedItem;
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
                if(textBoxUserName.Text == "")
                    textBoxUserName.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                else if(passwordBoxPassword.Password == "")
                    passwordBoxPassword.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                else
                    logOn_Click(sender, e);
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(pivot.SelectedItem == pivotItemHistory)
                appBarButtonSync.Visibility = Windows.UI.Xaml.Visibility.Visible;
            else
                appBarButtonSync.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            if(pivot.SelectedItem == pivotItemState)
                appBarButtonRename.Visibility = Windows.UI.Xaml.Visibility.Visible;
            else
                appBarButtonRename.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
    }
}
