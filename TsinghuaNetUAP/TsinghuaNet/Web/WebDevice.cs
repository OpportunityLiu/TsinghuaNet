using System;
using Windows.Web.Http;
using System.Threading.Tasks;
using Windows.Storage;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.UI.Xaml;

namespace TsinghuaNet.Web
{
    /// <summary>
    /// 表示连入网络的设备。
    /// </summary>
    public sealed class WebDevice : INotifyPropertyChanged,IDisposable
    {
        /// <summary>
        /// 初始化 <see cref="TsinghuaNet.WebDevice"/> 的实例并设置相关信息。
        /// </summary>
        /// <param name="ip">IP 地址。</param>
        /// <param name="mac">Mac 地址。</param>
        public WebDevice(Ipv4Address ip, MacAddress mac)
        {
            this.IPAddress = ip;
            this.Mac = mac;
            deviceDictChanged += WebDevice_deviceDictChanged;
        }

        private void WebDevice_deviceDictChanged()
        {
            this.PropertyChanging("Name");
        }

        public string DropToken
        {
            get;
            set;
        }
        
        public HttpClient HttpClient
        {
            get;
            set;
        }

        /// <summary>
        /// 获取 IP 地址。
        /// </summary>
        public Ipv4Address IPAddress
        {
            get;
            private set;
        }

        private Size traffic;

        /// <summary>
        /// 获取设备登陆以来的流量。
        /// </summary>
        public Size WebTraffic
        {
            get
            {
                return traffic;
            }
            set
            {
                traffic = value;
                PropertyChanging();
            }
        }

        /// <summary>
        /// 获取 Mac 地址。
        /// </summary>
        public MacAddress Mac
        {
            get;
            private set;
        }

        private DateTime logOn;

        /// <summary>
        /// 获取登陆的时间。
        /// </summary>
        public DateTime LogOnDateTime
        {
            get
            {
                return logOn;
            }
            set
            {
                logOn = value;
                PropertyChanging();
            }
        }

        private static event Action deviceDictChanged;

        private static DeviceNameDictionary deviceDict = initDeviceDict();

        private static DeviceNameDictionary initDeviceDict()
        {
            //挂起时保存列表
            App.Current.Suspending += (sender, e) =>
                ApplicationData.Current.RoamingSettings.Values["DeviceDict"] = deviceDict.Serialize();
            //同步时更新列表，并通知所有实例更新 Name 属性。
            ApplicationData.Current.DataChanged += (sender, args) =>
            {
                if (!sender.RoamingSettings.Values.ContainsKey("DeviceDict"))
                {
                    sender.RoamingSettings.Values.Add("DeviceDict", "");
                    deviceDict = new DeviceNameDictionary();
                }
                else
                    try
                    {
                        deviceDict = new DeviceNameDictionary((string)sender.RoamingSettings.Values["DeviceDict"]);
                    }
                    catch (ArgumentException)
                    {
                        deviceDict = new DeviceNameDictionary();
                    }
                if (deviceDictChanged != null)
                    deviceDictChanged();
            };
            //恢复列表
            if (!ApplicationData.Current.RoamingSettings.Values.ContainsKey("DeviceDict"))
            {
                ApplicationData.Current.RoamingSettings.Values.Add("DeviceDict", "");
                return new DeviceNameDictionary();
            }
            else
                try
                {
                    return new DeviceNameDictionary((string)ApplicationData.Current.RoamingSettings.Values["DeviceDict"]);
                }
                catch(ArgumentException)
                {
                    return new DeviceNameDictionary();
                }
        }

        /// <summary>
        /// 获取或设置当前设备的名称。
        /// </summary>
        /// <exception cref="System.InvalidOperationException">不能为未知设备设置名称。</exception>
        public string Name
        {
            get
            {
                if(this.Mac == MacAddress.Unknown)
                    return LocalizedStrings.UnknownDevice;
                else
                {
                    string r;
                    if(deviceDict.TryGetValue(this.Mac,out r))
                        return r;
                    else if(this.Mac.IsCurrent)
                        return LocalizedStrings.CurrentDevice;
                    else
                        return this.Mac.ToString();
                }
            }
            set
            {
                if(!CanRename)
                    throw new InvalidOperationException("不能为未知设备设置名称");
                if(string.IsNullOrWhiteSpace(value))
                {
                    deviceDict.Remove(this.Mac);
                }
                else
                {
                    deviceDict[this.Mac] = value;
                }
                this.PropertyChanging();
            }
        }

        /// <summary>
        /// 获取设备是否可以重命名的信息。
        /// </summary>
        public bool CanRename
        {
            get
            {
                return this.Mac != MacAddress.Unknown;
            }
        }

        private static readonly Uri dropUri = new Uri("https://usereg.tsinghua.edu.cn/online_user_ipv4.php");

        /// <summary>
        /// 异步执行使该设备下线的操作。
        /// </summary>
        /// <returns>
        /// <c>true</c> 表示成功，<c>false</c> 表示失败，请刷新设备列表后再试。
        /// </returns>
        public IAsyncOperation<bool> DropAsync()
        {
            return Run(async token =>
            {
                try
                {
                    var post = HttpClient.PostStrAsync(dropUri, "action=drop&user_ip=" + IPAddress + "&checksum=" + DropToken);
                    token.Register(() => post.Cancel());
                    return await post == "ok";
                }
                catch(OperationCanceledException)
                {
                    throw;
                }
                catch(Exception)
                {
                    return false;
                }
            });
        }

        #region INotifyPropertyChanged 成员

        public event PropertyChangedEventHandler PropertyChanged;

        private async void PropertyChanging([CallerMemberName] string propertyName = "")
        {
            if(PropertyChanged != null)
                foreach(var item in PropertyChanged.GetInvocationList())
                {
                    var t = item.Target as DependencyObject;
                    if(t != null)
                        await t.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => PropertyChanged(this, new PropertyChangedEventArgs(propertyName)));
                    else
                        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
        }

        #endregion

        #region IDisposable Support

        public void Dispose()
        {
            deviceDictChanged -= this.WebDevice_deviceDictChanged;
        }

        #endregion
    }
}
