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
using Windows.UI.Xaml;

namespace TsinghuaNet.Web
{
    /// <summary>
    /// 表示当前认证状态，并提供相关方法的类。
    /// </summary>
    public sealed class WebConnect : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// 使用用户名和加密后的密码创建新实例。
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="passwordMD5">MD5 加密后的密码，请使用 <see cref="MD5Helper.GetMd5Hash(string)"/> 方法进行加密。</param>
        /// <exception cref="ArgumentNullException">参数为 <c>null</c> 或 <see cref="string.Empty"/>。</exception>
        public WebConnect(string userName, string passwordMD5)
        {
            if(string.IsNullOrEmpty(userName))
                throw new ArgumentNullException("userName");
            if(string.IsNullOrEmpty(passwordMD5))
                throw new ArgumentNullException("passwordMD5");
            this.userName = userName;
            this.passwordMd5 = passwordMD5;
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
                WebConnect.current = value;
            }
            get
            {
                return WebConnect.current;
            }
        }

        private readonly string userName, passwordMd5;

        private static readonly Uri logOnUri = new Uri("http://net.tsinghua.edu.cn/cgi-bin/do_login");

        /// <summary>
        /// 异步登陆网络。
        /// </summary>
        /// <exception cref="TsinghuaNet.WebConnect.LogOnException">在登陆过程中发生错误。</exception>
        public IAsyncAction LogOnAsync()
        {
            return Run(async token =>
            {
                string res = null;
                IAsyncOperation<string> post = null;
                token.Register(() => post?.Cancel());
                using(var http = new HttpClient())
                {
                    Func<string, Task<bool>> check = async toPost =>
                         {
                             try
                             {
                                 post = http.PostStrAsync(logOnUri, toPost);
                                 res = await post;
                             }
                             catch(OperationCanceledException)
                             {
                                 return true;
                             }
                             catch(Exception ex)
                             {
                                 throw new LogOnException(LogOnExceptionType.ConnectError, ex);
                             }
                             if(Regex.IsMatch(res, @"^\d+,"))
                             {
                                 var a = res.Split(',');
                                 this.WebTraffic = new Size(ulong.Parse(a[2], CultureInfo.InvariantCulture));
                                 this.IsOnline = true;
                                 return true;
                             }
                             return false;
                         };
                    if(await check("action=check_online"))
                        return;
                    if(await check("username=" + userName + "&password=" + passwordMd5 + "&mac=" + MacAddress.Current + "&drop=0&type=1&n=100"))
                        return;
                    this.IsOnline = false;
                    if((Regex.IsMatch(res, @"^password_error@\d+")))
                        throw new LogOnException(LogOnExceptionType.PasswordError);
                    else
                        throw LogOnException.GetByErrorString(res);
                }
            });
        }

        private IAsyncAction signInUsereg(HttpClient http)
        {
            return Run(async token =>
            {
                bool needRetry = false;
                IAsyncOperation<string> postAction = null;
                token.Register(() => postAction?.Cancel());
                Func<Task> signIn = async () =>
                    {
                        postAction = http.PostStrAsync(new Uri("https://usereg.tsinghua.edu.cn/do.php"), "action=login&user_login_name=" + userName + "&user_password=" + passwordMd5);
                        var logOnRes = await postAction;
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
                    await signIn();
                }
                catch(LogOnException ex)
                {
                    if(ex.ExceptionType == LogOnExceptionType.UnknownError)
                    {
                        needRetry = true;
                    }
                    else
                    {
                        throw;
                    }
                }
                catch(OperationCanceledException)
                {
                    throw;
                }
                catch(Exception ex)
                {
                    throw new LogOnException(LogOnExceptionType.ConnectError, ex);
                }
                if(!needRetry)
                    return;
                await Task.Delay(500);
                await signIn();//重试
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
                var http = new HttpClient();
                IAsyncAction act = null;
                IAsyncOperation<string> ope = null;
                token.Register(() =>
                {
                    act?.Cancel();
                    ope?.Cancel();
                });
                try
                {
                    act = signInUsereg(http);
                    await act;
                    //获取用户信息
                    ope = http.GetStrAsync(new Uri("https://usereg.tsinghua.edu.cn/user_info.php"));
                    var res1 = await ope;
                    var info1 = Regex.Match(res1, "使用流量\\(IPV4\\).+?(\\d+?)\\(byte\\).+?帐户余额.+?([0-9.]+)\\(元\\)", RegexOptions.Singleline).Groups;
                    if(info1.Count != 3)
                    {
                        var ex = new InvalidOperationException("获取到的数据格式错误。");
                        ex.Data.Add("HtmlResponse", res1);
                        throw ex;
                    }
                    WebTraffic = new Size(ulong.Parse(info1[1].Value, CultureInfo.InvariantCulture));
                    Balance = decimal.Parse(info1[2].Value, CultureInfo.InvariantCulture);
                    //获取登录信息
                    var res2 = await http.GetStrAsync(new Uri("https://usereg.tsinghua.edu.cn/online_user_ipv4.php"));
                    var info2 = Regex.Matches(res2, "<tr align=\"center\">.+?</tr>", RegexOptions.Singleline);
                    var devices = (from Match r in info2
                                   let details = Regex.Matches(r.Value, "(?<=\\<td class=\"maintd\"\\>)(.+?)(?=\\</td\\>)")
                                   select new WebDevice(Ipv4Address.Parse(details[3].Value),
                                                       MacAddress.Parse(details[17].Value))
                                   {
                                       WebTraffic = Size.Parse(details[4].Value),
                                       LogOnDateTime = DateTime.ParseExact(details[14].Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                                       DropToken = Regex.Match(r.Value, "(?<=drop\\('" + details[3].Value + "',')(.+?)(?='\\))").Value,
                                       HttpClient = http
                                   }).ToArray();
                    var t = deviceList.FirstOrDefault();
                    if(t != null)
                        t.HttpClient.Dispose();
                    var common = devices.Join(deviceList, n => n, o => o, (n, o) =>
                    {
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
                catch(LogOnException)
                {
                    throw;
                }
                catch(OperationCanceledException)
                {
                    throw;
                }
                catch(Exception ex)
                {
                    throw new LogOnException(LogOnExceptionType.ConnectError, ex);
                }
                finally
                {
                    if(deviceList.Count == 0)
                        http.Dispose();
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

        private readonly ObservableCollection<WebDevice> deviceList;

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
            await Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }

        #endregion

        #region IDisposable Support

        void IDisposable.Dispose()
        {
            foreach(var item in deviceList)
            {
                item.Dispose();
            }
        }

        #endregion
    }
}