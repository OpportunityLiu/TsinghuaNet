using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace Web
{
    /// <summary>
    /// 表示当前认证状态，并提供相关方法的类。
    /// </summary>
    public sealed class WebConnect : ObservableObject, IDisposable
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
                using (var http = new HttpClient(new Windows.Web.Http.Filters.HttpBaseProtocolFilter()).WithHeaders())
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
            if (account == null)
                throw new ArgumentNullException(nameof(account));
            this.userName = account.UserName;
            account.RetrievePassword();
            this.password = account.Password;
            this.passwordMd5 = MD5Helper.GetMd5Hash(this.password);
        }

        private static WebConnect current;

        public static WebConnect Current
        {
            set => current = value ?? throw new ArgumentNullException("value");
            get => current;
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
                catch (Exception) { return; }
                if (string.IsNullOrWhiteSpace(dataStr))
                    return;
                JsonObject data;
                if (!JsonObject.TryParse(dataStr, out data))
                    return;
                this.Balance = (decimal)data[nameof(this.Balance)].GetNumber();
                this.UpdateTime = DateTime.FromBinary((long)data[nameof(this.UpdateTime)].GetNumber());
                this.WebTraffic = new Size((ulong)data[nameof(this.WebTraffic)].GetNumber());
                var devices = from item in data[nameof(this.DeviceList)].GetArray()
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
                this.Dispose();
                foreach (var item in devices)
                    this.deviceList.Add(item);
            });
        }

        public IAsyncAction SaveCache()
        {
            return Run(async token =>
            {
                var data = new JsonObject();
                data[nameof(this.Balance)] = JsonValue.CreateNumberValue((double)this.balance);
                data[nameof(this.UpdateTime)] = JsonValue.CreateNumberValue(this.updateTime.ToBinary());
                data[nameof(this.WebTraffic)] = JsonValue.CreateNumberValue(this.webTraffic.Value);
                var devices = new JsonArray();
                foreach (var item in this.deviceList)
                {
                    var device = new JsonObject();
                    device[nameof(WebDevice.IPAddress)] = JsonValue.CreateStringValue(item.IPAddress.ToString());
                    device[nameof(WebDevice.Mac)] = JsonValue.CreateStringValue(item.Mac.ToString());
                    device[nameof(WebDevice.WebTraffic)] = JsonValue.CreateNumberValue(item.WebTraffic.Value);
                    device[nameof(WebDevice.LogOnDateTime)] = JsonValue.CreateNumberValue(item.LogOnDateTime.ToBinary());
                    device[nameof(WebDevice.DeviceFamily)] = JsonValue.CreateNumberValue((int)item.DeviceFamily);
                    devices.Add(device);
                }
                data[nameof(this.DeviceList)] = devices;
                var cacheFolder = ApplicationData.Current.LocalCacheFolder;
                var cacheFile = await cacheFolder.CreateFileAsync("WebConnectCache.json", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(cacheFile, data.Stringify());
            });
        }

        private readonly string userName, password, passwordMd5;

        public IAsyncAction LogOnAsync(string ip)
        {
            if (ip.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(ip));
            ip = ip.Trim();
            Microsoft.Services.Store.Engagement.StoreServicesCustomEventLogger.GetDefault().Log("Log on with ip requested");
            return Run(async token =>
            {
                HttpClient http = null;
                try
                {
                    if (await HttpHelper.NeedSslVpn())
                        http = new HttpClient(new SslVpnFilter()).WithHeaders();
                    else
                        http = new HttpClient().WithHeaders();
                    IAsyncAction act = null;
                    IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> ope = null;
                    token.Register(() =>
                    {
                        act?.Cancel();
                        ope?.Cancel();
                    });
                    act = LogOnHelper.SignInUsereg(http, this.userName, this.passwordMd5);
                    await act;
                    //获取用户信息
                    ope = http.PostAsync(new Uri("http://usereg.tsinghua.edu.cn/ip_login.php"), new HttpFormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["action"] = "do_login",
                        ["drop"] = "0",
                        ["is_pad"] = "1",
                        ["n"] = "100",
                        ["type"] = "10",
                        ["user_ip"] = ip,
                    }));
                    var res1 = await ope;
                }
                catch (LogOnException) { throw; }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    throw new LogOnException(LogOnExceptionType.ConnectError, ex);
                }
                finally
                {
                    http?.Dispose();
                }
            });
        }

        /// <summary>
        /// 异步登陆网络。
        /// </summary>
        /// <exception cref="WebConnect.LogOnException">在登陆过程中发生错误。</exception>
        /// <returns>
        /// 成功返回 true，已经在线返回 false。
        /// </returns>
        public IAsyncOperation<bool> LogOnAsync()
        {
            Microsoft.Services.Store.Engagement.StoreServicesCustomEventLogger.GetDefault().Log("Log on requested");
            return Run(async token =>
            {
                try
                {
                    IAsyncOperation<bool> boolFunc = null;
                    IAsyncAction voidFunc = null;
                    boolFunc = HttpHelper.NeedSslVpn();
                    token.Register(() =>
                    {
                        boolFunc?.Cancel();
                        voidFunc?.Cancel();
                    });
                    if (await boolFunc)
                        return false;
                    using (var http = new HttpClient().WithHeaders())
                    {
                        boolFunc = LogOnHelper.CheckOnline(http);
                        if (await boolFunc)
                            return false;
                        voidFunc = LogOnHelper.LogOn(http, this.userName, this.passwordMd5);
                        await voidFunc;
                        return true;
                    }
                }
                catch (LogOnException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new LogOnException(LogOnExceptionType.ConnectError, ex);
                }
            });
        }

        private struct DeviceComparer : IEqualityComparer<WebDevice>
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
            Microsoft.Services.Store.Engagement.StoreServicesCustomEventLogger.GetDefault().Log("Refresh requested");
            return Run(async token =>
            {
                var networkFin = false;
                HttpClient http = null;
                try
                {
                    if (await HttpHelper.NeedSslVpn())
                        http = new HttpClient(new SslVpnFilter()).WithHeaders();
                    else
                        http = new HttpClient().WithHeaders();
                    IAsyncAction act = null;
                    IAsyncOperation<string> ope = null;
                    token.Register(() =>
                    {
                        act?.Cancel();
                        ope?.Cancel();
                    });
                    act = LogOnHelper.SignInUsereg(http, this.userName, this.passwordMd5);
                    await act;
                    //获取用户信息
                    ope = http.GetStrAsync(new Uri("http://usereg.tsinghua.edu.cn/user_info.php"));
                    var res1 = await ope;
                    var info1 = Regex.Match(res1, @"证件号.+?(\d+).+?使用流量\(IPV4\).+?(\d+?)\(byte\).+?帐户余额.+?([0-9,.]+)\(元\)", RegexOptions.Singleline).Groups;
                    if (info1.Count != 4)
                    {
                        var ex = new InvalidOperationException("获取到的数据格式错误。");
                        ex.Data.Add("HtmlResponse", res1);
                        throw ex;
                    }
                    Settings.AccountManager.ID = info1[1].Value;
                    this.WebTraffic = new Size(ulong.Parse(info1[2].Value, CultureInfo.InvariantCulture));
                    this.Balance = decimal.Parse(info1[3].Value, CultureInfo.InvariantCulture);
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
                    this.deviceList.FirstOrDefault()?.HttpClient?.Dispose();
                    networkFin = true;
                    this.deviceList.Update(devices, new DeviceComparer(), (o, n) =>
                    {
                        o.DeviceFamily = n.DeviceFamily;
                        o.DropToken = n.DropToken;
                        o.HttpClient = n.HttpClient;
                        o.LogOnDateTime = n.LogOnDateTime;
                        o.WebTraffic = n.WebTraffic;
                    });
                    //全部成功
                    this.UpdateTime = DateTime.Now;
                }
                catch (LogOnException) { throw; }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    throw new LogOnException(LogOnExceptionType.ConnectError, ex);
                }
                finally
                {
                    if (this.deviceList.Count == 0 || !networkFin)
                        http?.Dispose();
                }
            });
        }

        public string UserName => this.userName;

        private readonly ObservableList<WebDevice> deviceList = new ObservableList<WebDevice>();
        /// <summary>
        /// 使用该账户的设备列表。
        /// </summary>
        public ObservableListView<WebDevice> DeviceList => this.deviceList.AsReadOnly();

        /// <summary>
        /// 当前账户余额。
        /// </summary>
        public decimal Balance
        {
            get => this.balance;
            private set => Set(ref this.balance, value);
        }

        private decimal balance;

        /// <summary>
        /// 之前累积的的网络流量（不包括当前在线设备产生的流量）。
        /// </summary>
        public Size WebTraffic
        {
            get => this.webTraffic;
            private set => Set(ref this.webTraffic, value);
        }

        private Size webTraffic;

        /// <summary>
        /// 精确的网络流量（包括当前在线设备产生的流量）。
        /// </summary>
        public Size WebTrafficExact
        {
            get
            {
                if (this.deviceList.IsNullOrEmpty())
                    return this.webTraffic;
                else
                    return this.deviceList.Aggregate(this.webTraffic, (sum, item) => sum + item.WebTraffic);
            }
        }

        private DateTime updateTime;
        /// <summary>
        /// 信息更新的时间。
        /// </summary>
        public DateTime UpdateTime
        {
            get => this.updateTime;
            private set => this.Set(nameof(this.WebTrafficExact), ref this.updateTime, value);
        }

        #region IDisposable Support

        public void Dispose()
        {
            this.deviceList.FirstOrDefault()?.HttpClient?.Dispose();
            this.deviceList.Clear();
        }

        #endregion
    }
}