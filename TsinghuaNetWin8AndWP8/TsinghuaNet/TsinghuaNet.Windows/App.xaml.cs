using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Web;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.System.Threading;
using System.Linq;

// 有关“空白应用程序”模板的信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=234227

namespace TsinghuaNet
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    public sealed partial class App : Application
    {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
            this.Resuming += this.OnResuming;
            Current = this;

            //注册后台任务
            IBackgroundTaskRegistration task = null;
            foreach(var cur in BackgroundTaskRegistration.AllTasks)
            {
                if(cur.Value.Name == "RefreshBackgroundTask")
                {
                    task = cur.Value;
                    break;
                }
            }
            if(task == null)
            {
                var builder = new BackgroundTaskBuilder();
                builder.Name = "RefreshBackgroundTask";
                builder.TaskEntryPoint = "Tasks.RefreshBackgroundTask";
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
                    var tileNotification = new Windows.UI.Notifications.TileNotification(squareTile);
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

        private static CoreDispatcher currentDispatcher;

        public static System.Threading.Tasks.Task DispatcherRunAnsyc(DispatchedHandler agileCallback)
        {
            return App.currentDispatcher.RunAsync(CoreDispatcherPriority.Normal, agileCallback).AsTask();
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
        private Windows.UI.ViewManagement.AccessibilitySettings accessibilitySettings = new Windows.UI.ViewManagement.AccessibilitySettings();

        public Windows.UI.ViewManagement.AccessibilitySettings AccessibilitySettings
        {
            get
            {
                return accessibilitySettings;
            }
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 当启动应用程序以打开特定的文件或显示搜索结果等操作时，
        /// 将使用其他入口点。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            
            //注册设置项
            Windows.UI.ApplicationSettings.SettingsPane.GetForCurrentView().CommandsRequested += (sp, arg) =>
            {
                var resource = ResourceLoader.GetForViewIndependentUse();
                arg.Request.ApplicationCommands.Add(new Windows.UI.ApplicationSettings.SettingsCommand(1, resource.GetString("AboutMenu"), a => new About().Show()));
                arg.Request.ApplicationCommands.Add(new Windows.UI.ApplicationSettings.SettingsCommand(2, resource.GetString("SettingsMenu"), a => new Settings().Show()));
            };

            Frame rootFrame = Window.Current.Content as Frame;
            currentDispatcher = Window.Current.Dispatcher;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if(rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();
                rootFrame.CacheSize = 2;

                if(e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if(rootFrame.Content == null)
            {
                // 当未还原导航堆栈时，导航到第一页，
                // 并通过将所需信息作为导航参数传入来配置
                // 参数
                if(!rootFrame.Navigate(typeof(MainPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // 确保当前窗口处于活动状态
            Window.Current.Activate();
            await refresh();
        }
        

        /// <summary>
        /// 在将要挂起应用程序执行时调用。    将保存应用程序状态
        /// 将被终止还是恢复的情况下保存应用程序状态，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起的请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            // TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }

        private async void OnResuming(object sender, object e)
        {
            await refresh();
        }

        private async Task refresh()
        {
            if(WebConnect.Current == null)
                return;
            try
            {
                await WebConnect.Current.LogOnAsync();
            }
            catch(LogOnException)
            {
            }
            try
            {
                await WebConnect.Current.RefreshAsync();
            }
            catch(LogOnException)
            {
            }
        }
    }
}