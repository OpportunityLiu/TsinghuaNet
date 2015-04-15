using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Html;

namespace TsinghuaNet.Web
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
            this.http = new HttpClient(new HttpClientHandler(), true);
            this.http.BaseAddress = new Uri("https://usereg.tsinghua.edu.cn");
            this.deviceList = new ObservableCollection<WebDevice>();
            this.DeviceList = new ReadOnlyObservableCollection<WebDevice>(this.deviceList);
            this.deviceList.CollectionChanged += (sender, e) => propertyChanging("WebTrafficExact");
        }

        private static WebConnect current;

        public static WebConnect Current
        {
            set
            {
                if(value == null)
                    throw new ArgumentNullException("value");
                WebConnect.current = value;
            }
            get
            {
                return WebConnect.current;
            }
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
                string res = null;
                Func<string, bool> check = toPost =>
                {
                    try
                    {
                        res = http.Post("http://net.tsinghua.edu.cn/cgi-bin/do_login", toPost);
                    }
                    catch(AggregateException ex)
                    {
                        throw new LogOnException(LogOnExceptionType.ConnectError, ex);
                    }
                    if(Regex.IsMatch(res, @"^\d+,"))
                    {
                        var a = res.Split(',');
                        App.DispatcherRunAnsyc(() =>
                        {
                            this.WebTraffic = new Size(ulong.Parse(a[2], System.Globalization.CultureInfo.InvariantCulture));
                            this.IsOnline = true;
                        }).Wait();
                        return true;
                    }
                    return false;
                };
                if(check("action=check_online"))
                    return;
                if(check("username=" + userName + "&password=" + passwordMd5 + "&mac=" + MacAddress.Current + "&drop=0&type=1&n=100"))
                    return;
                App.DispatcherRunAnsyc(() => this.IsOnline = false).Wait();
                if((Regex.IsMatch(res, @"^password_error@\d+")))
                    throw new LogOnException(LogOnExceptionType.PasswordError);
                else
                    throw LogOnException.GetByErrorString(res);
            });
        }

        private void signInUsereg()
        {
            Action signIn = () =>
            {
                string logOnRes;
                lock(http)
                {
                    logOnRes = http.Post("https://usereg.tsinghua.edu.cn/do.php", "action=login&user_login_name=" + userName + "&user_password=" + passwordMd5);
                }
                switch(logOnRes)
                {
                    case "ok":
                        break;
                    case "用户不存在":
                        throw new LogOnException(LogOnExceptionType.UserNameError);
                    case "密码错误":
                        throw new LogOnException(LogOnExceptionType.PasswordError);
                    default:
                        throw new LogOnException(logOnRes);
                }
            };
            try
            {
                signIn();
            }
            catch(AggregateException ex)
            {
                throw new LogOnException(LogOnExceptionType.ConnectError, ex);
            }
            catch(LogOnException ex)
            {
                if(ex.ExceptionType == LogOnExceptionType.UnknownError)
                {
                    Task.Delay(100).Wait();
                    try
                    {
                        signIn();//重试第一次
                    }
                    catch(InvalidOperationException)
                    {
                        Task.Delay(1000).Wait();
                        signIn();//重试第二次
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        private volatile bool refreshing = false;

        /// <summary>
        /// 异步请求更新状态。
        /// </summary>
        public Task RefreshAsync()
        {
            return Task.Run(() =>
            {
                if(refreshing)
                    return;
                refreshing = true;
                try
                {
                    signInUsereg();
                    //获取用户信息
                    string res1, res2;
                    lock(http)
                        res1 = HtmlUtilities.ConvertToText(http.Get("https://usereg.tsinghua.edu.cn/user_info.php"));
                    var info1 = Regex.Match(res1, @"使用流量\(IPV4\)(\d+)\(byte\).+帐户余额(.+)\(元\)", RegexOptions.Singleline).Groups;
                    if(info1.Count != 3)
                    {
                        var ex = new InvalidOperationException("获取到的数据格式错误。");
                        ex.Data.Add("HtmlResponse", res1);
                        throw ex;
                    }
                    var task1 = App.DispatcherRunAnsyc(() =>
                    {
                        WebTraffic = new Size(ulong.Parse(info1[1].Value, System.Globalization.CultureInfo.InvariantCulture));
                        Balance = decimal.Parse(info1[2].Value, System.Globalization.CultureInfo.InvariantCulture);
                    });
                    //获取登录信息
                    lock(http)
                        res2 = http.Get("https://usereg.tsinghua.edu.cn/online_user_ipv4.php");
                    var info2 = Regex.Matches(res2, "<tr align=\"center\">.+?</tr>", RegexOptions.Singleline);
                    var devices = new List<WebDevice>();
                    foreach(Match item in info2)
                    {
                        var details = Regex.Matches(item.Value, "(?<=\\<td class=\"maintd\"\\>)(.+?)(?=\\</td\\>)");
                        devices.Add(new WebDevice(Ipv4Address.Parse(details[3].Value), Size.Parse(details[4].Value), MacAddress.Parse(details[17].Value), DateTime.ParseExact(details[14].Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), Regex.Match(item.Value, "(?<=drop\\('" + details[3].Value + "',')(.+?)(?='\\))").Value, http));
                    }
                    App.DispatcherRunAnsyc(() =>
                    {
                        var isOnlineTemp = false;
                        deviceList.Clear();
                        foreach(var item in devices)
                        {
                            deviceList.Add(item);
                            if(item.Mac.IsCurrent)
                                isOnlineTemp = true;
                        }
                        this.IsOnline = isOnlineTemp;
                    }).Wait();
                    task1.Wait();
                    //全部成功
                    App.DispatcherRunAnsyc(() => UpdateTime = DateTime.Now).Wait();
                }
                catch(InvalidOperationException ex)
                {
                    throw new LogOnException(LogOnExceptionType.ConnectError, ex);
                }
                catch(Exception)
                {
                    throw;
                }
                finally
                {
                    refreshing = false;
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
        /// 当前连接的状态。
        /// </summary>
        public bool IsOnline
        {
            get
            {
                return isOnline;
            }
            private set
            {
                isOnline = value;
                propertyChanging();
            }
        }

        private bool isOnline = false;

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
                propertyChanging("WebTrafficExact");
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
                var sum = webTraffic;
                foreach(var item in deviceList)
                {
                    sum += item.WebTraffic;
                }
                return sum;
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
            }
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

        /// <summary>
        /// 属性更改时引发。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void propertyChanging([CallerMemberName] string propertyName = "")
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}