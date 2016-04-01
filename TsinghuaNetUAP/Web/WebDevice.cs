using System;
using Windows.Web.Http;
using System.Threading.Tasks;
using Windows.Storage;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.UI.Xaml;
using Windows.ApplicationModel.Resources;
using System.Collections.Generic;

namespace Web
{
    public enum DeviceFamily
    {
        Unknown = 0,
        WindowsPhone,
        Windows,
        iPad,
        iPhone,
        Android,
        Linux,
        MacOS
    }

    /// <summary>
    /// 表示连入网络的设备。
    /// </summary>
    public sealed class WebDevice : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// 初始化 <see cref="WebDevice"/> 的实例并设置相关信息。
        /// </summary>
        /// <param name="ip">IP 地址。</param>
        /// <param name="mac">Mac 地址。</param>
        public WebDevice(Ipv4Address ip, MacAddress mac)
        {
            this.IPAddress = ip;
            this.Mac = mac;
            deviceDictChanged += WebDevice_deviceDictChanged;
        }

        /// <summary>
        /// 初始化 <see cref="WebDevice"/> 的实例并设置相关信息。
        /// </summary>
        /// <param name="ip">IP 地址。</param>
        /// <param name="mac">Mac 地址。</param>
        /// <param name="deviceFamilyDescription">设备描述。</param>
        public WebDevice(Ipv4Address ip, MacAddress mac, string deviceFamilyDescription)
            : this(ip, mac)
        {
            SetDeviceFimaly(deviceFamilyDescription);
        }

        private DeviceFamily deviceFamily;

        public DeviceFamily DeviceFamily
        {
            get
            {
                return deviceFamily;
            }
            set
            {
                deviceFamily = value;
                PropertyChanging();
            }
        }

        private static Dictionary<string, DeviceFamily> deviceFamilyDictionary = new Dictionary<string, DeviceFamily>()
        {
            ["Windows Phone"] = DeviceFamily.WindowsPhone,
            ["Windows"] = DeviceFamily.Windows,
            ["Linux"] = DeviceFamily.Linux,
            ["Mac"] = DeviceFamily.MacOS,
            ["Android"] = DeviceFamily.Android,
            ["iPad"] = DeviceFamily.iPad,
            ["iPhone"] = DeviceFamily.iPhone,
            
        };

        public void SetDeviceFimaly(string deviceFamilyDescription)
        {
            if(string.IsNullOrWhiteSpace(deviceFamilyDescription))
            {
                DeviceFamily = DeviceFamily.Unknown;
                return;
            }
            deviceFamilyDescription = deviceFamilyDescription.Trim();
            foreach(var item in deviceFamilyDictionary)
            {
                if(deviceFamilyDescription.StartsWith(item.Key))
                {
                    DeviceFamily = item.Value;
                    return;
                }
            }

            DeviceFamily = DeviceFamily.Unknown;
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
            //同步时更新列表，并通知所有实例更新 Name 属性。
            ApplicationData.Current.DataChanged += (sender, args) =>
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

        private static void saveDeviceList()
        {
            ApplicationData.Current.RoamingSettings.Values["DeviceDict"] = deviceDict.Serialize();
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
                    return LocalizedStrings.Resources.UnknownDevice;
                else
                {
                    string r;
                    if(deviceDict.TryGetValue(this.Mac,out r))
                        return r;
                    else if(this.Mac.IsCurrent)
                        return LocalizedStrings.Resources.CurrentDevice;
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
                saveDeviceList();
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

        private static readonly Uri dropUri = new Uri("http://usereg.tsinghua.edu.cn/online_user_ipv4.php");

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
                if(HttpClient == null)
                    return false;
                try
                {
                    var post = HttpClient.PostStrAsync(dropUri, $"action=drops&user_ip={DropToken}");
                    token.Register(() => post.Cancel());
                    var ans = await post;
                    return ans == "下线请求已发送";
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
            await DispatcherHelper.Run(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
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
