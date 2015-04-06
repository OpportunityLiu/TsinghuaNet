using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TsinghuaNet.Web
{
    /// <summary>
    /// 表示连入网络的设备。
    /// </summary>
    public class WebDevice : INotifyPropertyChanged
    {
        private static readonly string current = App.ResourceString("StringCurrent");
        private static readonly string unknown = App.ResourceString("StringUnknown");

        /// <summary>
        /// 初始化 <see cref="TsinghuaNet.WebDevice"/> 的实例并设置相关信息。
        /// </summary>
        /// <param name="ip">IP 地址。</param>
        /// <param name="webTraffic">设备登陆以来的流量。</param>
        /// <param name="mac">Mac 地址。</param>
        /// <param name="logOnDateTime">登陆的时间。</param>
        /// <param name="dropToken">下线操作使用的令牌。</param>
        /// <param name="http">下线操作使用的连接。</param>
        /// <exception cref="System.ArgumentNullException">参数为 <c>null</c>。</exception>
        public WebDevice(Ipv4Address ip, Size webTraffic, MacAddress mac, DateTime logOnDateTime, string dropToken, HttpClient http)
        {
            if(string.IsNullOrEmpty(dropToken))
                throw new ArgumentNullException("dropToken");
            if(http == null)
                throw new ArgumentNullException("http");
            this.dropToken = "action=drop&user_ip=" + ip + "&checksum=" + dropToken;
            this.IPAddress = ip;
            this.Mac = mac;
            this.WebTraffic = webTraffic;
            this.LogOnDateTime = logOnDateTime;
            this.http = http;
            deviceDictChanged += (sender, args) => this.PropertyChanging("Name");
        }

        private HttpClient http;

        private string dropToken;

        /// <summary>
        /// 获取 IP 地址。
        /// </summary>
        public Ipv4Address IPAddress
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取设备登陆以来的流量。
        /// </summary>
        public Size WebTraffic
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取 Mac 地址。
        /// </summary>
        public MacAddress Mac
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取登陆的时间。
        /// </summary>
        public DateTime LogOnDateTime
        {
            get;
            private set;
        }

        private static event EventHandler deviceDictChanged;

        private static DeviceNameDictionary deviceDict = initDeviceDict();

        private static DeviceNameDictionary initDeviceDict()
        {
            //挂起时保存列表
            App.Current.Suspending += (sender, e) =>
            {
                var deferral = e.SuspendingOperation.GetDeferral();
                ApplicationData.Current.RoamingSettings.Values["DeviceDict"] = deviceDict.Stringify();
                deferral.Complete();
            };

            //同步时更新列表，并通知所有实例更新 Name 属性。
            ApplicationData.Current.DataChanged += (sender, args) =>
            {
                App.DispatcherRunAnsyc(() =>
                {
                    if(!sender.RoamingSettings.Values.ContainsKey("DeviceDict"))
                    {
                        sender.RoamingSettings.Values.Add("DeviceDict", "");
                        deviceDict = new DeviceNameDictionary();
                    }
                    else
                        try
                        {
                            deviceDict = new DeviceNameDictionary((string)sender.RoamingSettings.Values["DeviceDict"]);
                        }
                        catch(ArgumentException)
                        {
                            deviceDict = new DeviceNameDictionary();
                        }
                    if(deviceDictChanged != null)
                        deviceDictChanged(deviceDict, null);
                });
            };
            //恢复列表
            if(!ApplicationData.Current.RoamingSettings.Values.ContainsKey("DeviceDict"))
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
                    return unknown;
                else if(deviceDict.ContainsKey(this.Mac))
                    return deviceDict[this.Mac];
                else if(this.Mac.IsCurrent)
                    return current;
                else
                    return this.Mac.ToString();
            }
            set
            {
                if(this.Mac == MacAddress.Unknown)
                    throw new InvalidOperationException("不能为未知设备设置名称");
                if(string.IsNullOrWhiteSpace(value))
                {
                    if(deviceDict.ContainsKey(this.Mac))
                        deviceDict.Remove(this.Mac);
                }
                else
                {
                    if(deviceDict.ContainsKey(this.Mac))
                        deviceDict[this.Mac] = value;
                    else
                        deviceDict.Add(this.Mac, value);
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

        /// <summary>
        /// 异步执行使该设备下线的操作。
        /// </summary>
        /// <returns>
        /// <c>true</c> 表示成功，<c>false</c> 表示失败，请刷新设备列表后再试。
        /// </returns>
        public Task<bool> DropAsync()
        {
            return Task<bool>.Run(() =>
            {
                try
                {
                    lock(http)
                        return http.Post("https://usereg.tsinghua.edu.cn/online_user_ipv4.php", this.dropToken) == "ok";
                }
                catch(AggregateException)
                {
                    return false;
                }
            });
        }

        #region INotifyPropertyChanged 成员

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void PropertyChanging([CallerMemberName] string propertyName = "")
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
