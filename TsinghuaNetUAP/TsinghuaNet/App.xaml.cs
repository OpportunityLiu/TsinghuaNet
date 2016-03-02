using Microsoft.ApplicationInsights;
using System;
using System.Linq;
using Web;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.System;
using System.IO;

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
            WindowsAppInitializer.InitializeAsync(WindowsCollectors.Metadata | WindowsCollectors.Session |
                WindowsCollectors.PageView | WindowsCollectors.UnhandledException);
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
            this.Resuming += this.OnResuming;
            Current = this;

            //注册后台任务
            IBackgroundTaskRegistration BackgroundLogOnTask = null;
            foreach(var cur in BackgroundTaskRegistration.AllTasks)
            {
                if(cur.Value.Name == "BackgroundLogOnTask")
                {
                    BackgroundLogOnTask = cur.Value;
                    continue;
                }
            }
            if(BackgroundLogOnTask == null)
            {
                var builder = new BackgroundTaskBuilder();
                builder.Name = "BackgroundLogOnTask";
                builder.TaskEntryPoint = "BackgroundLogOnTask.Task";
                builder.SetTrigger(new SystemTrigger(SystemTriggerType.NetworkStateChange, false));
                BackgroundLogOnTask = builder.Register();
            }

            object theme;
            if(ApplicationData.Current.LocalSettings.Values.TryGetValue("Theme", out theme))
            {
                switch((ElementTheme)Enum.Parse(typeof(ElementTheme), theme.ToString()))
                {
                case ElementTheme.Light:
                    Current.RequestedTheme = ApplicationTheme.Light;
                    break;
                case ElementTheme.Dark:
                    Current.RequestedTheme = ApplicationTheme.Dark;
                    break;
                default:
                    break;
                }
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values["Theme"] = ElementTheme.Default.ToString();
            }
        }

        public static new App Current
        {
            get;
            private set;
        }

        private void launch(IActivatedEventArgs e)
        {

#if DEBUG
            if(System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            if(ApplicationData.Current.Version < 1)
            {
                var ignore = ApplicationData.Current.SetVersionAsync(1, args =>
                {
                    var d = ApplicationData.Current.RoamingSettings;
                    d.Values.Remove("Password");
                    d.Values.Remove("UserName");
                    d.Values.Remove("PasswordMD5");
                });
            }

            var view = ApplicationView.GetForCurrentView();
            view.SetPreferredMinSize(new Windows.Foundation.Size(320, 400));
            if(e.PreviousExecutionState == ApplicationExecutionState.NotRunning)
                view.TryResizeView(new Windows.Foundation.Size(320, 600));

            var currentWindow = Window.Current;
            if(currentWindow.Content == null)
            {
                currentWindow.Content = new MainPage();
            }

            currentWindow.Activate();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            launch(e);
        }

        protected override void OnActivated(IActivatedEventArgs e)
        {
            launch(e);
        }

        private void OnResuming(object sender, object e)
        {
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var def = e.SuspendingOperation.GetDeferral();
            var size = Window.Current.Bounds;
            var s = new Windows.Foundation.Size(size.Width, size.Height);
            ApplicationData.Current.LocalSettings.Values["MainViewSize"] = s;

            await WebConnect.Current.SaveCache();
            def.Complete();
        }
    }
}
