using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Html;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace TsinghuaNet
{
    /// <summary>
    /// 表示当前认证状态，并提供相关方法的类。
    /// </summary>
    public sealed class WebConnect : IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        /// 使用用户名和加密后的密码创建新实例。
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="passwordMD5">MD5 加密后的密码，请使用 <see cref="TsinghuaNet.MD5.MDString(string)"/> 方法进行加密。</param>
        /// <exception cref="System.ArgumentNullException">参数为 <c>null</c> 或 <see cref="string.Empty"/>。</exception>
        public WebConnect(string userName, string passwordMD5)
        {
            if(string.IsNullOrEmpty(userName))
                throw new ArgumentNullException("userName");
            if(string.IsNullOrEmpty(passwordMD5))
                throw new ArgumentNullException("passwordMD5");
            this.userName = userName;
            this.passwordMd5 = passwordMD5;
            this.http = new HttpClient();
            this.http.BaseAddress = new Uri("https://usereg.tsinghua.edu.cn");
            this.deviceList = new ObservableCollection<WebDevice>();
            this.DeviceList = new ReadOnlyObservableCollection<WebDevice>(this.deviceList);
            this.RefreshAsync();
        }

        private string userName, passwordMd5;

        private HttpClient http;

        /// <summary>
        /// 异步登陆网络。
        /// </summary>
        /// <exception cref="TsinghuaNet.WebConnect.LogOnException">在登陆过程中发生错误。</exception>
        public Task LogOnAsync()
        {
            return Task.Run(() =>
            {
                var toPost = "username=" + userName + "&password=" + passwordMd5 + "&mac=" + MacAddress.Current + "&drop=0&type=1&n=100";
                string res;
                try
                {
                    res = httpPost("http://net.tsinghua.edu.cn/cgi-bin/do_login", toPost);
                }
                catch(AggregateException ex)
                {
                    throw new LogOnException("连接错误。", ex);
                }
                if(Regex.IsMatch(res, @"^\d+,"))
                {
                    var a = res.Split(',');
                    this.webTraffic.Value = ulong.Parse(a[2], System.Globalization.CultureInfo.InvariantCulture);
                    this.IsOnline = true;
                    return;
                }
                this.IsOnline = false;
                if((Regex.IsMatch(res, @"^password_error@\d+")))
                    throw new LogOnException("密码错误或会话失效");
                else if(logOnErrorDict.ContainsKey(res))
                    throw new LogOnException(logOnErrorDict[res]);
                else
                    throw new LogOnException("未知错误。");
            });
        }

        private static Dictionary<string, string> logOnErrorDict = initLogOnErrorDict();

        private static Dictionary<string, string> initLogOnErrorDict()
        {
            var dict = new Dictionary<string, string>();
            dict.Add("username_error", "用户名错误");
            dict.Add("password_error", "密码错误");
            dict.Add("user_tab_error", "认证程序未启动");
            dict.Add("user_group_error", "您的计费组信息不正确");
            dict.Add("non_auth_error", "您无须认证，可直接上网");
            dict.Add("status_error", "用户已欠费，请尽快充值。");
            dict.Add("available_error", "您的帐号已停用");
            dict.Add("delete_error", "您的帐号已删除");
            dict.Add("ip_exist_error", "IP已存在，请稍后再试。");
            dict.Add("usernum_error", "用户数已达上限");
            dict.Add("online_num_error", "该帐号的登录人数已超过限额，请登录https://usereg.tsinghua.edu.cn断开不用的连接。");
            dict.Add("mode_error", "系统已禁止WEB方式登录，请使用客户端");
            dict.Add("time_policy_error", "当前时段不允许连接");
            dict.Add("flux_error", "您的流量已超支");
            dict.Add("minutes_error", "您的时长已超支");
            dict.Add("ip_error", "您的 IP 地址不合法");
            dict.Add("mac_error", "您的 MAC 地址不合法");
            dict.Add("sync_error", "您的资料已修改，正在等待同步，请 2 分钟后再试。");
            dict.Add("ip_alloc", "您不是这个地址的合法拥有者，IP 地址已经分配给其它用户。");
            dict.Add("ip_invaild", "您是区内地址，无法使用。");
            return dict;
        }

        /// <summary>
        /// 异步退出登录。
        /// </summary>
        /// <exception cref="System.Net.Http.HttpRequestException">发生连接错误。</exception>
        public Task LogOffAsync()
        {
            return Task.Run(() =>
            {
                using(var con = new StringContent(""))
                {
                    try
                    {
                        var request = http.PostAsync("http://net.tsinghua.edu.cn/cgi-bin/do_login", con);
                        request.Wait();
                        var response = request.Result.Content.ReadAsStringAsync();
                        response.Wait();
                    }
                    catch(AggregateException ex)
                    {
                        throw new HttpRequestException("无法连接。", ex);
                    }
                }
            });
        }

        private string httpPost(string uri, string request)
        {
            using(var re = new StringContent(request))
            {
                re.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
                using(var get = http.PostAsync(uri, re).Result)
                {
                    return get.Content.ReadAsStringAsync().Result;
                }
            }
        }

        private string cookie;

        private void logOnUsereg()
        {
            Action logOn = () =>
            {
                //获取登陆页以获得 cookie
                using(var get = http.GetAsync("https://usereg.tsinghua.edu.cn/login.php").Result)
                {
                    try
                    {
                        cookie = get.Headers.GetValues("Set-Cookie").First().Split(";".ToCharArray())[0];
                    }
                    catch(InvalidOperationException)
                    {
                        //cookie 尚未过期。
                    }
                }
                http.DefaultRequestHeaders.Remove("Cookie");
                http.DefaultRequestHeaders.Add("Cookie", cookie);
                //登陆
                if(httpPost("https://usereg.tsinghua.edu.cn/do.php", "action=login&user_login_name=" + userName + "&user_password=" + passwordMd5) != "ok")
                    throw new InvalidOperationException("返回异常。");
            };
            try
            {
                logOn();
            }
            catch(InvalidOperationException)
            {
                Task.Delay(100).Wait();
                logOn();//重试一次
            }
        }

        /// <summary>
        /// 异步请求更新状态。
        /// </summary>
        public Task RefreshAsync()
        {
            return Task.Run(() =>
                {
                    try
                    {
                        logOnUsereg();
                        //获取用户信息
                        var res1 = HtmlUtilities.ConvertToText(http.GetStringAsync("https://usereg.tsinghua.edu.cn/user_info.php").Result);
                        var info1 = Regex.Match(res1, @"使用流量\(IPV4\)(\d+)\(byte\).+帐户余额(.+)\(元\)", RegexOptions.Singleline).Groups;
                        if(info1.Count != 3)
                            throw new InvalidOperationException("获取到的数据格式错误。");
                        webTraffic.Value = ulong.Parse(info1[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                        Balance = decimal.Parse(info1[2].Value, System.Globalization.CultureInfo.InvariantCulture);
                        //获取登录信息
                        var res2 = http.GetStringAsync("https://usereg.tsinghua.edu.cn/online_user_ipv4.php").Result;
                        var info2 = Regex.Matches(res2, "<tr align=\"center\">.+?</tr>", RegexOptions.Singleline);
                        var devices = new List<WebDevice>();
                        foreach(Match item in info2)
                        {
                            var details = Regex.Matches(item.Value, "(?<=\\<td class=\"maintd\"\\>)(.+?)(?=\\</td\\>)");
                            devices.Add(new WebDevice(Ipv4Address.Parse(details[3].Value), Size.Parse(details[4].Value), MacAddress.Parse(details[17].Value), DateTime.ParseExact(details[14].Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), Regex.Match(item.Value, "(?<=drop\\('" + details[3].Value + "',')(.+?)(?='\\))").Value, http));
                        }
                        App.CurrentDispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            deviceList.Clear();
                            foreach(var item in devices)
                                deviceList.Add(item);
                        }).AsTask().Wait();
                        //全部成功
                        UpdateTime = DateTime.Now;
                        propertyChanging();
                    }
                    catch(Exception)
                    {
                        throw;
                    }
                });
        }

        public Task<WebUsageData> GetUsageAnsyc()
        {
            return Task<WebUsageData>.Run(() =>
            {
                logOnUsereg();
                var res = http.GetStringAsync("https://usereg.tsinghua.edu.cn/user_detail_list.php?action=balance2&start_time=1900-01-01&end_time=" + DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "&is_ipv6=0&page=1&offset=100000").Result;
                return new WebUsageData(res, DeviceList);
            });
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
        /// 当前连接的状态。
        /// </summary>
        public bool IsOnline
        {
            get;
            private set;
        }

        /// <summary>
        /// 当前账户余额。
        /// </summary>
        public decimal Balance
        {
            get;
            private set;
        }

        private Size webTraffic;

        /// <summary>
        /// 之前累积的的网络流量（不包括当前在线设备产生的流量）。
        /// </summary>
        public Size WebTraffic
        {
            get      
            {
                return webTraffic;
            }
        }

        /// <summary>
        /// 精确的网络流量（包括当前在线设备产生的流量）。
        /// </summary>
        public Size WebTrafficExact
        {
            get
            {
                var sum = webTraffic;
                foreach(var item in deviceList)
                {
                    sum += item.WebTraffic;
                }
                return sum;
            }
        }

        /// <summary>
        /// 信息更新的时间。
        /// </summary>
        public DateTime UpdateTime
        {
            get;
            private set;
        }

        #region IDisposable 成员

        /// <summary>
        /// 释放由 <see cref="TsinghuaNet.WebConnect"/> 使用的非托管资源和托管资源。
        /// </summary>
        public void Dispose()
        {
            this.http.Dispose();
        }

        #endregion

        #region INotifyPropertyChanged 成员

        public event PropertyChangedEventHandler PropertyChanged;

        private void propertyChanging([CallerMemberName] string propertyName = "")
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }


    /// <summary>
    /// 表示在登陆过程中发生的错误。
    /// </summary>
    public class LogOnException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="TsinghuaNet.LogOnException"/> 类的新实例。
        /// </summary>
        public LogOnException()
        {
        }

        /// <summary>
        /// 使用指定的错误信息初始化 <see cref="TsinghuaNet.LogOnException"/> 类的新实例。
        /// </summary>
        /// <param name="message">描述错误的消息。</param>
        public LogOnException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// 使用指定错误消息和对作为此异常原因的内部异常的引用来初始化 <see cref="TsinghuaNet.LogOnException"/> 类的新实例。
        /// </summary>
        /// <param name="message">解释异常原因的错误信息。</param>
        /// <param name="inner">
        /// 导致当前异常的异常；如果未指定内部异常，则是一个 <c>null</c> 引用（在 Visual Basic 中为 <c>Nothing</c>）。
        /// </param>
        public LogOnException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}