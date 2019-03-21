using Newtonsoft.Json;
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
    public class AccountInfo
    {
        [JsonConstructor]
        internal AccountInfo(long userId, string userName, string realName, string department)
        {
            UserId = userId;
            UserName = userName;
            RealName = realName;
            Department = department;
        }

        public long UserId { get; }
        public string UserName { get; }
        public string RealName { get; set; }
        public string Department { get; set; }
    }


    /// <summary>
    /// 表示当前认证状态，并提供相关方法的类。
    /// </summary>
    public sealed class WebConnect : ObservableObject, IDisposable
    {
        // M$ wants a test account, make them happy.
        private const string TEST_USER_NAME = "Test";
        private const string TEST_PASSWORD = "123456";

        internal static readonly List<WebDevice> TestDeviceList = new List<WebDevice>
        {
            new WebDevice(new Ipv4Address(166,111,21,24), default, "Windows")
            {
                LogOnDateTime = new DateTime(2018,9,18,15,25,11),
                WebTraffic = new Size(11234591),
            },
            new WebDevice(new Ipv4Address(59,66,99,177), new MacAddress(122,21,31,66,201,154), "Android")
            {
                LogOnDateTime = new DateTime(2018,9,19,15,25,11),
                WebTraffic = new Size(383356321),
            },
        };

        private class IdObject
        {
            public Ss ss { get; set; }

            public class Ss
            {
                public Account account { get; set; }

                public class Account
                {
                    public string userId { get; set; }
                    public string username { get; set; }
                    public string realName { get; set; }
                    public string deptString { get; set; }
                }
            }
        }

        /// <summary>
        /// 检查账户有效性。
        /// </summary>
        /// <param name="userNameOrUserId">用户名或 ID</param>
        /// <param name="password">密码</param>
        /// <returns>有效则返回用户信息，否则为 <see langword="null"/>。</returns>
        public static IAsyncOperation<AccountInfo> CheckAccount(string userNameOrUserId, string password)
        {
            return Run(async token =>
            {
                if (userNameOrUserId == TEST_USER_NAME && password == TEST_PASSWORD)
                {
                    await Task.Delay(1000);
                    return new AccountInfo(0, userNameOrUserId, "测试", "测试");
                }
                using (var http = new HttpClient(new Windows.Web.Http.Filters.HttpBaseProtocolFilter()).WithHeaders())
                {
                    var result = await http.PostAsync(new Uri($"https://id.tsinghua.edu.cn/security_check"),
                        new HttpFormUrlEncodedContent(new[]{
                            new KeyValuePair<string,string>("username", userNameOrUserId),
                            new KeyValuePair<string,string>("password", password),
                        }));
                    var response = await result.Content.ReadAsStringAsync();
                    if (response.Contains("认证失败"))
                        return null;
                    var data = Regex.Match(response, @"\$\.extend\(\s*uidm\s*,\s*({.+?""ss"".+?})\s*\)");
                    if (!data.Success)
                        return null;
                    var info = JsonConvert.DeserializeObject<IdObject>(data.Groups[1].Value).ss.account;
                    return new AccountInfo(long.Parse(info.userId), info.username, info.realName, info.deptString);
                }
            });
        }

        /// <summary>
        /// 使用凭据创建实例。
        /// </summary>
        /// <param name="accountInfo">帐户信息</param>
        /// <param name="password">密码</param>
        /// <exception cref="ArgumentNullException">参数为 <c>null</c>。</exception>
        /// <exception cref="ArgumentException">参数错误。</exception>
        public WebConnect(AccountInfo accountInfo, string password)
        {
            if (accountInfo is null)
                throw new ArgumentNullException(nameof(accountInfo));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            AccountInfo = accountInfo;
            _Password = password;
            _PasswordMd5 = MD5Helper.GetMd5Hash(this._Password);
            IsTestAccount = accountInfo.UserName == TEST_USER_NAME;
        }

        internal readonly bool IsTestAccount;

        public AccountInfo AccountInfo { get; }

        internal string Password => _Password;

        private readonly string _Password, _PasswordMd5;

        private readonly ObservableList<WebDevice> _DeviceList = new ObservableList<WebDevice>();
        /// <summary>
        /// 使用该账户的设备列表。
        /// </summary>
        public ObservableListView<WebDevice> DeviceList => _DeviceList.AsReadOnly();

        private static WebConnect _Current;

        public static WebConnect Current
        {
            set => _Current = value ?? throw new ArgumentNullException("value");
            get => _Current;
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
                    this._DeviceList.Add(item);
            });
        }

        public IAsyncAction SaveCache()
        {
            return Run(async token =>
            {
                var data = new JsonObject();
                data[nameof(this.Balance)] = JsonValue.CreateNumberValue((double)this._Balance);
                data[nameof(this.UpdateTime)] = JsonValue.CreateNumberValue(this._UpdateTime.ToBinary());
                data[nameof(this.WebTraffic)] = JsonValue.CreateNumberValue(this._WebTraffic.Value);
                var devices = new JsonArray();
                foreach (var item in this._DeviceList)
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
                    if (this.IsTestAccount)
                    {
                        await Task.Delay(1500);
                        var ipadd = Ipv4Address.Parse(ip);
                        if (TestDeviceList.Any(d => d.IPAddress == ipadd))
                            return;
                        TestDeviceList.Add(new WebDevice(ipadd, default, "Windows")
                        {
                            LogOnDateTime = DateTime.Now
                        });
                        return;
                    }
                    IAsyncAction act = null;
                    IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> ope = null;
                    token.Register(() =>
                    {
                        act?.Cancel();
                        ope?.Cancel();
                    });
                    act = LogOnHelper.SignInUsereg(http, AccountInfo.UserName, _PasswordMd5);
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
                        voidFunc = LogOnHelper.LogOn(http, AccountInfo.UserName, _PasswordMd5);
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
                    if (this.IsTestAccount)
                    {
                        this._DeviceList.Update(TestDeviceList, new DeviceComparer(), updateDevice);
                        this.UpdateTime = DateTime.Now;
                        return;
                    }
                    act = LogOnHelper.SignInUsereg(http, AccountInfo.UserName, _PasswordMd5);
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
                    this._DeviceList.FirstOrDefault()?.HttpClient?.Dispose();
                    networkFin = true;
                    this._DeviceList.Update(devices, new DeviceComparer(), updateDevice);
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
                    if (this._DeviceList.Count == 0 || !networkFin)
                        http?.Dispose();
                }
            });

            void updateDevice(WebDevice o, WebDevice n)
            {
                o.DeviceFamily = n.DeviceFamily;
                o.DropToken = n.DropToken;
                o.HttpClient = n.HttpClient;
                o.LogOnDateTime = n.LogOnDateTime;
                o.WebTraffic = n.WebTraffic;
            }
        }

        /// <summary>
        /// 当前账户余额。
        /// </summary>
        public decimal Balance
        {
            get => this.IsTestAccount ? (decimal)12.01 : this._Balance;
            private set => Set(ref this._Balance, value);
        }

        private decimal _Balance;

        /// <summary>
        /// 之前累积的的网络流量（不包括当前在线设备产生的流量）。
        /// </summary>
        public Size WebTraffic
        {
            get => this.IsTestAccount ? new Size(2591263496) : this._WebTraffic;
            private set => Set(ref this._WebTraffic, value);
        }

        private Size _WebTraffic;

        /// <summary>
        /// 精确的网络流量（包括当前在线设备产生的流量）。
        /// </summary>
        public Size WebTrafficExact
        {
            get
            {
                if (this._DeviceList.IsNullOrEmpty())
                    return this.WebTraffic;
                else
                    return this._DeviceList.Aggregate(this.WebTraffic, (sum, item) => sum + item.WebTraffic);
            }
        }

        private DateTime _UpdateTime;
        /// <summary>
        /// 信息更新的时间。
        /// </summary>
        public DateTime UpdateTime
        {
            get => this._UpdateTime;
            private set => this.Set(nameof(this.WebTrafficExact), ref this._UpdateTime, value);
        }

        #region IDisposable Support

        public void Dispose()
        {
            this._DeviceList.FirstOrDefault()?.HttpClient?.Dispose();
            this._DeviceList.Clear();
        }

        #endregion
    }
}