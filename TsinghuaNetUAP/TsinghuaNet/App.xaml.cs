using System;
using Web;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Windows.UI;
using Microsoft.HockeyApp;

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
#if !DEBUG
            HockeyClient.Current.Configure("42bdf568c96e4ae1ab90a8835c48a88c", new TelemetryConfiguration()
            {
                Collectors = WindowsCollectors.Metadata | WindowsCollectors.Session | WindowsCollectors.UnhandledException
            }).SetExceptionDescriptionLoader(ex =>
            {
                var sb = new System.Text.StringBuilder();
                do
                {
                    sb.AppendLine($"Type: {ex.GetType()}");
                    sb.AppendLine($"HResult: {ex.HResult}");
                    sb.AppendLine($"Message: {ex.Message}");
                    sb.AppendLine();
                    sb.AppendLine("Data:");
                    foreach(var item in ex.Data.Keys)
                    {
                        sb.AppendLine($"    {item}: {ex.Data[item]}");
                    }
                    sb.AppendLine("Stacktrace:");
                    sb.AppendLine(ex.StackTrace);
                    ex = ex.InnerException;
                    sb.AppendLine("_____________________");
                } while(ex != null);
                return sb.ToString();
            });
#endif
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

            switch((ElementTheme)Enum.Parse(typeof(ElementTheme), Settings.SettingsHelper.GetLocal("Theme", "Default")))
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

        public static new App Current
        {
            get;
            private set;
        }

        private void launch(IActivatedEventArgs e, bool prelaunch)
        {

#if DEBUG
            if(System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            if(!prelaunch && ApplicationData.Current.Version < 2)
            {
                var ignore = ApplicationData.Current.SetVersionAsync(2, args =>
                {
                    var d = ApplicationData.Current.RoamingSettings;
                    d.Values.Remove("Password");
                    d.Values.Remove("UserName");
                    d.Values.Remove("PasswordMD5");
                });
            }

            if(Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var sb = StatusBar.GetForCurrentView();
                sb.BackgroundColor = (Color)Resources["SystemChromeMediumColor"];
                sb.BackgroundOpacity = 1;
                sb.ForegroundColor = (Color)Resources["SystemBaseMediumHighColor"];
            }

            var view = ApplicationView.GetForCurrentView();
            view.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
            view.SetPreferredMinSize(new Windows.Foundation.Size(320, 400));
            view.TitleBar.BackgroundColor = (Color)Resources["SystemChromeMediumLowColor"];
            view.TitleBar.ButtonBackgroundColor = (Color)Resources["SystemChromeMediumLowColor"];
            view.TitleBar.InactiveBackgroundColor = (Color)Resources["SystemChromeMediumLowColor"];
            view.TitleBar.ButtonInactiveBackgroundColor = (Color)Resources["SystemChromeMediumLowColor"];

            var currentWindow = Window.Current;
            if(currentWindow.Content == null)
            {
                var f = new Frame();
                f.Navigate(typeof(MainPage), prelaunch);
                currentWindow.Content = f;
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
            NotificationService.NotificationService.HandleLaunching(e.TileId);
            launch(e, e.PrelaunchActivated);
        }

        protected override void OnActivated(IActivatedEventArgs e)
        {
            NotificationService.NotificationService.HandleLaunching((e as ToastNotificationActivatedEventArgs)?.Argument);
            launch(e, false);
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
            if(WebConnect.Current != null)
            {
                var def = e.SuspendingOperation.GetDeferral();
                var sc = WebConnect.Current.SaveCache();
                await sc;
                def.Complete();
            }
        }
    }
}
