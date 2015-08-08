using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TsinghuaNet.Web;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=402347&clcid=0x409

namespace TsinghuaNet
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            WindowsAppInitializer.InitializeAsync(WindowsCollectors.Metadata | WindowsCollectors.Session | WindowsCollectors.UnhandledException);
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
            this.Resuming += this.OnResuming;
            Current = this;
            
            //注册后台任务
            IBackgroundTaskRegistration task = null;
            foreach(var cur in BackgroundTaskRegistration.AllTasks)
            {
                if(cur.Value.Name == "BackgroundLogOnTask")
                {
                    task = cur.Value;
                    break;
                }
            }
            if(task == null)
            {
                var builder = new BackgroundTaskBuilder();
                builder.Name = "BackgroundLogOnTask";
                builder.TaskEntryPoint = "BackgroundLogOnTask.Task";
                builder.SetTrigger(new SystemTrigger(SystemTriggerType.NetworkStateChange, false));
                task = builder.Register();
            }

            //初始化信息存储区
            try
            {
                var passVault = new Windows.Security.Credentials.PasswordVault();
                var pass = passVault.FindAllByResource("TsinghuaAccount").First();
                pass.RetrievePassword();
                var userName = pass.UserName;
                var passwordMD5 = pass.Password;
                //已经添加字段
                if(!string.IsNullOrEmpty(userName) && !string.IsNullOrWhiteSpace(passwordMD5))
                {
                    WebConnect.Current = new WebConnect(userName, passwordMD5);
                    //准备磁贴更新
                    WebConnect.Current.PropertyChanged += UpdeteTile;
                }
            }
            // 未找到储存的密码
            catch(Exception ex) when (ex.HResult == -2147023728)
            {
            }
        }

        private async void UpdeteTile(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName != nameof(WebConnect.UpdateTime))
                return;
            await Task.Run(() =>
            {
                var text = WebConnect.Current.WebTrafficExact.ToString();
                var manager = TileUpdateManager.CreateTileUpdaterForApplication();
                manager.Clear();
                manager.EnableNotificationQueue(true);
                var devices = WebConnect.Current.DeviceList.ToArray();
                if(devices.Length==0)
                {
                    XmlDocument tile = new XmlDocument();
                    tile.LoadXml($@"
<tile>
    <visual branding='name'>
        <binding template='TileMedium'>
            <text hint-style='body'>{text}</text>
            <text hint-style='caption' hint-wrap='true'>{LocalizedStrings.TileNoDevices}</text>
        </binding>
        <binding template='TileWide'>
            <text hint-style='body'>{LocalizedStrings.TileUsage}{text}</text>
            <text hint-style='caption' hint-wrap='true'>{LocalizedStrings.TileNoDevices}</text>
        </binding>
    </visual>
</tile>");
                    var tileNotification = new TileNotification(tile);
                    tileNotification.ExpirationTime = new DateTimeOffset(DateTime.Now.AddDays(1));
                    manager.Update(tileNotification);
                    return;
                }
                foreach(var item in devices)
                {
                    XmlDocument tile = new XmlDocument();
                    tile.LoadXml($@"
<tile>
    <visual branding='name'>
        <binding template='TileMedium'>
            <text hint-style='body'>{text}</text>
            <text hint-style='caption'>{item.Name}</text>
            <text hint-style='captionsubtle'>{item.LogOnDateTime.TimeOfDay}</text>
            <text hint-style='captionsubtle'>{item.IPAddress}</text>
        </binding>
        <binding template='TileWide'>
            <text hint-style='body'>{LocalizedStrings.TileUsage}{text}</text>
            <text hint-style='caption'>{item.Name}</text>
            <text hint-style='captionsubtle'>{item.LogOnDateTime}</text>
            <text hint-style='captionsubtle'>{item.IPAddress}</text>
        </binding>
    </visual>
</tile>");
                    var tileNotification = new TileNotification(tile);
                    tileNotification.ExpirationTime = new DateTimeOffset(DateTime.Now.AddDays(1));
                    manager.Update(tileNotification);
                }
            });
        }

        public static new App Current
        {
            get;
            private set;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if(System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            var view = ApplicationView.GetForCurrentView();
            view.SetPreferredMinSize(new Windows.Foundation.Size(320, 500));
            if(e.PreviousExecutionState == ApplicationExecutionState.NotRunning)
                view.TryResizeView(new Windows.Foundation.Size(320, 600));

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if(rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                //if(e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                //{
                //    // Load state from previously suspended application
                //}

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if(rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        private void OnResuming(object sender, object e)
        {
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
        }
    }
}
