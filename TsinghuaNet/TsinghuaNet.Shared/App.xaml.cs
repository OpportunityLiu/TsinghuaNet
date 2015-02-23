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
using TsinghuaNet.Web;

// 有关“空白应用程序”模板的信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=234227

namespace TsinghuaNet
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    public sealed partial class App : Application
    {
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif

        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
            this.Resuming += this.OnResuming;
            App.Current = this;

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
                builder.SetTrigger(new SystemTrigger(SystemTriggerType.InternetAvailable, false));
                task = builder.Register();
            }
            task.Completed += refreshOnCompleted;

            //初始化信息存储区
            try
            {
                ApplicationData.Current.LocalSettings.Values.Add("UserName", "");
                ApplicationData.Current.LocalSettings.Values.Add("PasswordMD5", "");
            }
            catch(ArgumentException)
            {
                //已经添加字段
                if(!string.IsNullOrEmpty((string)ApplicationData.Current.LocalSettings.Values["UserName"]))
                    WebConnect.SetCurrent(new WebConnect((string)ApplicationData.Current.LocalSettings.Values["UserName"], (string)ApplicationData.Current.LocalSettings.Values["PasswordMD5"]));
            }

            // 准备Toast通知
            toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText04);
            toastTitle = toastXml.CreateTextNode("");
            toastText1 = toastXml.CreateTextNode("");
            toastText2 = toastXml.CreateTextNode("");
            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastTitle);
            stringElements[1].AppendChild(toastText1);
            stringElements[2].AppendChild(toastText2);
        }

        public static new App Current
        {
            get;
            private set;
        }

        private static CoreDispatcher currentDispatcher;

        public static Windows.Foundation.IAsyncAction DispatcherRunAnsyc(DispatchedHandler agileCallback)
        {
            return App.currentDispatcher.RunAsync(CoreDispatcherPriority.Normal, agileCallback);
        }

        private void refreshOnCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            SendToastNotification("Internet Connected", "", "");
        }

        /// <summary>
        /// 发送 Toast 通知。
        /// </summary>
        /// <param name="title">标题，加粗显示。</param>
        /// <param name="text1">第一行内容。</param>
        /// <param name="text2">第二行内容。</param>
        public void SendToastNotification(string title, string text1, string text2)
        {
            toastTitle.NodeValue = title;
            toastText1.NodeValue = text1;
            toastText2.NodeValue = text2;
            notifier.Show(new ToastNotification(toastXml));
        }

        private XmlDocument toastXml;
        private XmlText toastTitle, toastText1, toastText2;
        private ToastNotifier notifier = ToastNotificationManager.CreateToastNotifier();

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 当启动应用程序以打开特定的文件或显示搜索结果等操作时，
        /// 将使用其他入口点。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            //注册设置项
#if WINDOWS_APP
            Windows.UI.ApplicationSettings.SettingsPane.GetForCurrentView().CommandsRequested += (sp, arg) =>
            {
                arg.Request.ApplicationCommands.Add(new Windows.UI.ApplicationSettings.SettingsCommand(1, (string)this.Resources["StringAbout"], a => new About().Show()));
            };
#endif


            Frame rootFrame = Window.Current.Content as Frame;
            currentDispatcher = Window.Current.Dispatcher;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();
                rootFrame.CacheSize = 2;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // 删除用于启动的旋转门导航。
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                // 当未还原导航堆栈时，导航到第一页，
                // 并通过将所需信息作为导航参数传入来配置
                // 参数
                if (!rootFrame.Navigate(typeof(MainPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // 确保当前窗口处于活动状态
            Window.Current.Activate();
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// 启动应用程序后还原内容转换。
        /// </summary>
        /// <param name="sender">附加了处理程序的对象。</param>
        /// <param name="e">有关导航事件的详细信息。</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

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

        private void OnResuming(object sender, object e)
        {

        }
    }
}