using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Windows.Web.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.Foundation;
using Windows.Web.Http.Headers;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.Data.Json;

namespace Web
{
    /// <summary>
    /// 表示当前认证状态，并提供相关方法的类。
    /// </summary>
    public sealed class WebConnect : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// 检查账户有效性。
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>有效则返回 <c>true</c>，否则为 <c>false</c>。</returns>
        public static IAsyncOperation<bool> CheckAccount(string userName, string password)
        {
            return Run(async token =>
            {
                using(var http = new HttpClient(new Windows.Web.Http.Filters.HttpBaseProtocolFilter()))
                {
                    var result = await http.GetStringAsync(new Uri($"https://learn.tsinghua.edu.cn/MultiLanguage/lesson/teacher/loginteacher.jsp?userid={userName}&userpass={password}"));
                    return !result.Contains("window.alert");
                }
            });
        }

        /// <summary>
        /// 使用凭据创建实例。
        /// </summary>
        /// <param name="account">帐户凭据</param>
        /// <exception cref="ArgumentNullException">参数为 <c>null</c>。</exception>
        /// <exception cref="ArgumentException">参数错误。</exception>
        public WebConnect(PasswordCredential account)
        {
            if(account == null)
                throw new ArgumentNullException(nameof(account));
            this.userName = account.UserName;
            account.RetrievePassword();
            this.password = account.Password;
            this.passwordMd5 = MD5Helper.GetMd5Hash(password);
            this.deviceList = new ObservableCollection<WebDevice>();
            this.DeviceList = new ReadOnlyObservableCollection<WebDevice>(this.deviceList);
        }

        private static WebConnect current;

        public static WebConnect Current
        {
            set
            {
                if(value == null)
                    throw new ArgumentNullException("value");
                current = value;
            }
            get
            {
                return current;
            }
        }

        public IAsyncAction LoadCache()
        {
            return Run(async token =>
            {
                string dataStr;
                try
                {
                    var cacheFolder = ApplicationData.Current.LocalCacheFolder;
                    var cacheFile = await cacheFolder.GetFileAsync("WebConnectCache.json");
                    dataStr = await FileIO.ReadTextAsync(cacheFile);
                }
                catch(Exception) { return; }
                if(string.IsNullOrWhiteSpace(dataStr))
                    return;
                JsonObject data;
                if(!JsonObject.TryParse(dataStr, out data))
                    return;
                Balance = (decimal)data[nameof(Balance)].GetNumber();
                UpdateTime = DateTime.FromBinary((long)data[nameof(UpdateTime)].GetNumber());
                WebTraffic = new Size((ulong)data[nameof(WebTraffic)].GetNumber());
                var devices = from item in data[nameof(DeviceList)].GetArray()
                              let device = item.GetObject()
                              let ip = Ipv4Address.Parse(device[nameof(WebDevice.IPAddress)].GetString())
                              let mac = MacAddress.Parse(device[nameof(WebDevice.Mac)].GetString())
                              let deTraffic = new Size((ulong)device[nameof(WebDevice.WebTraffic)].GetNumber())
                              let deTime = DateTime.FromBinary((long)device[nameof(WebDevice.LogOnDateTime)].GetNumber())
                              let deviceFamily = ((DeviceFamily)(int)device[nameof(WebDevice.DeviceFamily)].GetNumber())
                              select new WebDevice(ip, mac)
                              {
                                  WebTraffic = deTraffic,
                                  LogOnDateTime = deTime,
                                  DeviceFamily = deviceFamily
                              };
                Dispose();
                foreach(var item in devices)
                    deviceList.Add(item);
            });
        }

        public IAsyncAction SaveCache()
        {
            return Run(async token =>
            {
                var data = new JsonObject();
                data[nameof(Balance)] = JsonValue.CreateNumberValue((double)this.balance);
                data[nameof(UpdateTime)] = JsonValue.CreateNumberValue(this.updateTime.ToBinary());
                data[nameof(WebTraffic)] = JsonValue.CreateNumberValue(this.webTraffic.Value);
                var devices = new JsonArray();
                foreach(var item in this.deviceList)
                {
                    var device = new JsonObject();
                    device[nameof(WebDevice.IPAddress)] = JsonValue.CreateStringValue(item.IPAddress.ToString());
                    device[nameof(WebDevice.Mac)] = JsonValue.CreateStringValue(item.Mac.ToString());
                    device[nameof(WebDevice.WebTraffic)] = JsonValue.CreateNumberValue(item.WebTraffic.Value);
                    device[nameof(WebDevice.LogOnDateTime)] = JsonValue.CreateNumberValue(item.LogOnDateTime.ToBinary());
                    device[nameof(WebDevice.DeviceFamily)] = JsonValue.CreateNumberValue((int)item.DeviceFamily);
                    devices.Add(device);
                }
                data[nameof(DeviceList)] = devices;
                var cacheFolder = ApplicationData.Current.LocalCacheFolder;
                var cacheFile = await cacheFolder.CreateFileAsync("WebConnectCache.json", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(cacheFile, data.Stringify());
            });
        }

        private readonly string userName, password, passwordMd5;

        /// <summary>
        /// 异步登陆网络。
        /// </summary>
        /// <exception cref="WebConnect.LogOnException">在登陆过程中发生错误。</exception>
        /// <returns>
        /// 成功返回 true，已经在线返回 false。
        /// </returns>
        public IAsyncOperation<bool> LogOnAsync()
        {
            return Run(async token =>
            {
                IAsyncOperation<bool> boolFunc = null;
                IAsyncAction voidFunc = null;
                boolFunc = HttpHelper.NeedSslVpn();
                token.Register(() =>
                {
                    boolFunc?.Cancel();
                    voidFunc?.Cancel();
                });
                try
                {
                    if(await boolFunc)
                        return false;
                }
                catch(Exception ex)
                {
                    throw new LogOnException(LogOnExceptionType.ConnectError, ex);
                }
                using(var http = new HttpClient())
                {
                    http.DefaultRequestHeaders.UserAgent.Add(new HttpProductInfoHeaderValue("Mozilla", "5.0"));
                    if(Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                    {
                        http.DefaultRequestHeaders.UserAgent.Add(new HttpProductInfoHeaderValue("Windows Phone 10.0"));
                    }
                    else
                    {
                        http.DefaultRequestHeaders.UserAgent.Add(new HttpProductInfoHeaderValue("Windows NT 10.0"));
                    }
                    boolFunc = LogOnHelper.CheckOnline(http);
                    if(await boolFunc)
                        return false;
                    voidFunc = LogOnHelper.LogOn(http, userName, passwordMd5);
                    await voidFunc;
                    return true;
                }
            });
        }

        private struct deviceComparer : IEqualityComparer<WebDevice>
        {
            #region IEqualityComparer<WebDevice> 成员

            public bool Equals(WebDevice x, WebDevice y)
            {
                return x.IPAddress == y.IPAddress && x.Mac == y.Mac;
            }

            public int GetHashCode(WebDevice obj)
            {
                return obj.IPAddress.GetHashCode();
            }

            #endregion
        }

        /// <summary>
        /// 异步请求更新状态。
        /// </summary>
        public IAsyncAction RefreshAsync()
        {
            return Run(async token =>
            {
                var networkFin = false;
                HttpClient http = null;
                try
                {
                    if(await HttpHelper.NeedSslVpn())
                        http = new HttpClient(new SslVpnFilter());
                    else
                        http = new HttpClient();
                    IAsyncAction act = null;
                    IAsyncOperation<string> ope = null;
                    token.Register(() =>
                    {
                        act?.Cancel();
                        ope?.Cancel();
                    });
                    act = LogOnHelper.SignInUsereg(http, userName, passwordMd5);
                    await act;
                    //获取用户信息
                    ope = http.GetStrAsync(new Uri("http://usereg.tsinghua.edu.cn/user_info.php"));
                    var res1 = await ope;
                    var info1 = Regex.Match(res1, @"证件号.+?(\d+).+?使用流量\(IPV4\).+?(\d+?)\(byte\).+?帐户余额.+?([0-9.]+)\(元\)", RegexOptions.Singleline).Groups;
                    if(info1.Count != 4)
                    {
                        var ex = new InvalidOperationException("获取到的数据格式错误。");
                        ex.Data.Add("HtmlResponse", res1);
                        throw ex;
                    }
                    Settings.AccountManager.ID = info1[1].Value;
                    WebTraffic = new Size(ulong.Parse(info1[2].Value, CultureInfo.InvariantCulture));
                    Balance = decimal.Parse(info1[3].Value, CultureInfo.InvariantCulture);
                    //获取登录信息
                    var res2 = await http.GetStrAsync(new Uri("http://usereg.tsinghua.edu.cn/online_user_ipv4.php"));
                    var info2 = Regex.Matches(res2, "<tr align=\"center\">.+?</tr>", RegexOptions.Singleline);
                    var devices = (from Match r in info2
                                   let details = Regex.Matches(r.Value, "(?<=\\<td class=\"maintd\"\\>)(.*?)(?=\\</td\\>)")
                                   let ip = Ipv4Address.Parse(details[0].Value)
                                   let mac = MacAddress.Parse(details[6].Value)
                                   let tra = Size.Parse(details[2].Value)
                                   let device = details[10].Value
                                   let logOnTime = DateTime.ParseExact(details[1].Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                                   let devToken = Regex.Match(r.Value, @"(?<=value="")(\d+)(?="")").Value
                                   select new WebDevice(ip, mac, device)
                                   {
                                       WebTraffic = tra,
                                       LogOnDateTime = logOnTime,
                                       DropToken = devToken,
                                       HttpClient = http
                                   }).ToArray();
                    deviceList.FirstOrDefault()?.HttpClient?.Dispose();
                    networkFin = true;
                    var common = devices.Join(deviceList, n => n, o => o, (n, o) =>
                    {
                        o.DeviceFamily = n.DeviceFamily;
                        o.DropToken = n.DropToken;
                        o.HttpClient = n.HttpClient;
                        o.LogOnDateTime = n.LogOnDateTime;
                        o.WebTraffic = n.WebTraffic;
                        return o;
                    }, new deviceComparer());
                    for(int i = 0; i < deviceList.Count;)
                    {
                        if(!common.Contains(deviceList[i]))
                        {
                            deviceList[i].Dispose();
                            deviceList.RemoveAt(i);
                        }
                        else
                            i++;
                    }
                    foreach(var item in devices)
                    {
                        if(!common.Contains(item, new deviceComparer()))
                            deviceList.Add(item);
                        else
                            item.Dispose();
                    }
                    //全部成功
                    UpdateTime = DateTime.Now;
                }
                catch(LogOnException) { throw; }
                catch(OperationCanceledException) { throw; }
                catch(Exception ex)
                {
                    throw new LogOnException(LogOnExceptionType.ConnectError, ex);
                }
                finally
                {
                    if(deviceList.Count == 0 || !networkFin)
                        http?.Dispose();
                }
            });
        }

        public string UserName
        {
            get
            {
                return this.userName;
            }
        }

        private ObservableCollection<WebDevice> deviceList;

        /// <summary>
        /// 使用该账户的设备列表。
        /// </summary>
        public ReadOnlyObservableCollection<WebDevice> DeviceList
        {
            get;
            private set;
        }

        /// <summary>
        /// 当前账户余额。
        /// </summary>
        public decimal Balance
        {
            get
            {
                return balance;
            }
            private set
            {
                balance = value;
                propertyChanging();
            }
        }

        private decimal balance;

        /// <summary>
        /// 之前累积的的网络流量（不包括当前在线设备产生的流量）。
        /// </summary>
        public Size WebTraffic
        {
            get
            {
                return webTraffic;
            }
            private set
            {
                webTraffic = value;
                propertyChanging();
            }
        }

        private Size webTraffic;

        /// <summary>
        /// 精确的网络流量（包括当前在线设备产生的流量）。
        /// </summary>
        public Size WebTrafficExact
        {
            get
            {
                if(deviceList == null || deviceList.Count == 0)
                    return webTraffic;
                else
                    return deviceList.Aggregate(webTraffic, (sum, item) => sum + item.WebTraffic);
            }
        }

        private DateTime updateTime;

        /// <summary>
        /// 信息更新的时间。
        /// </summary>
        public DateTime UpdateTime
        {
            get
            {
                return updateTime;
            }
            private set
            {
                updateTime = value;
                propertyChanging();
                propertyChanging("WebTrafficExact");
            }
        }

        #region INotifyPropertyChanged 成员

        /// <summary>
        /// 属性更改时引发。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 引发 <see cref="PropertyChanged"/> 事件。
        /// </summary>
        /// <param name="propertyName">更改的属性名，默认值表示调用方名称。</param>
        private async void propertyChanging([CallerMemberName] string propertyName = "")
        {
            await DispatcherHelper.Run(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }

        #endregion

        #region IDisposable Support

        public void Dispose()
        {
            deviceList.FirstOrDefault()?.HttpClient?.Dispose();
            foreach(var item in deviceList)
                item.Dispose();
            deviceList.Clear();
        }

        #endregion
    }
}