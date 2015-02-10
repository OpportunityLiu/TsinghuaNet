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
            hub.Sections.Remove(hubSectionStart);
            hub.Sections.Remove(hubSectionState);
            hubSectionState.DataContext = new WebDevice[] { new WebDevice(new Ipv4Address(), new Size(111), new MacAddress(), DateTime.Now, "ss", new System.Net.Http.HttpClient()) };
            Task.Run(async() =>
            {
                await Task.Delay(1000);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => hub.Sections.Add(hubSectionStart));
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            hub.Sections.Remove(hubSectionStart);
            hub.Sections.Add(hubSectionState);
        }

        private void page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            hubSectionPic.Width = e.NewSize.Width - 200;
        }
    }
}
