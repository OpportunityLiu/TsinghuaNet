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
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(Microsoft.ApplicationInsights.WindowsCollectors.Metadata | Microsoft.ApplicationInsights.WindowsCollectors.Session);
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
            this.Resuming += this.OnResuming;
            Current = this;

            // TODO: background task
            //注册后台任务
            //IBackgroundTaskRegistration task = null;
            //foreach (var cur in BackgroundTaskRegistration.AllTasks)
            //{
            //    if (cur.Value.Name == "RefreshBackgroundTask")
            //    {
            //        task = cur.Value;
            //        break;
            //    }
            //}
            //if (task == null)
            //{
            //    var builder = new BackgroundTaskBuilder();
            //    builder.Name = "RefreshBackgroundTask";
            //    builder.TaskEntryPoint = "Tasks.RefreshBackgroundTask";
            //    builder.SetTrigger(new SystemTrigger(SystemTriggerType.NetworkStateChange, false));
            //    task = builder.Register();
            //}

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

            // 准备Toast通知
            toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            toastTitle = toastXml.CreateTextNode("");
            toastText = toastXml.CreateTextNode("");
            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastTitle);
            stringElements[1].AppendChild(toastText);
        }

        private async void UpdeteTile(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // TODO: new tile
            if(e.PropertyName != "UpdateTime")
                return;
            await Task.Run(() =>
            {
                var text = WebConnect.Current.WebTrafficExact.ToString();
                var manager = TileUpdateManager.CreateTileUpdaterForApplication();
                manager.Clear();
                manager.EnableNotificationQueue(true);

                var squareTile = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text01);
                var longTile = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150Text01);
                var node = squareTile.ImportNode(longTile.GetElementsByTagName("binding").Item(0), true);
                squareTile.GetElementsByTagName("visual").Item(0).AppendChild(node);
                var bindings = squareTile.GetElementsByTagName("binding");
                ((XmlElement)bindings[0]).SetAttribute("branding", "name");
                ((XmlElement)bindings[1]).SetAttribute("branding", "name");
                var tileTexts = squareTile.GetElementsByTagName("text");
                tileTexts[0].InnerText = text;
                tileTexts[4].InnerText = string.Format("已用流量：{0}", text);
                var devices = new WebDevice[5];
                WebConnect.Current.DeviceList.CopyTo(devices, 0);
                foreach(var item in devices)
                {
                    if(item == null)
                        break;
                    tileTexts[1].InnerText = tileTexts[5].InnerText = item.Name;
                    tileTexts[2].InnerText = tileTexts[6].InnerText = item.IPAddress.ToString();
                    tileTexts[3].InnerText = tileTexts[7].InnerText = item.LogOnDateTime.ToString();
                    var tileNotification = new TileNotification(squareTile);
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
        /// 发送 Toast 通知。
        /// </summary>
        /// <param name="title">标题，加粗显示。</param>
        /// <param name="content">内容。</param>
        public void SendToastNotification(string title, string content)
        {
            toastTitle.NodeValue = title;
            toastText.NodeValue = content;
            notifier.Show(new ToastNotification(toastXml));
        }

        private XmlDocument toastXml;
        private XmlText toastTitle, toastText;
        private ToastNotifier notifier = ToastNotificationManager.CreateToastNotifier();

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
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Windows.Foundation.Size(320, 500));

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
