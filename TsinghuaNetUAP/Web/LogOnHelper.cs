using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Web.Http;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.Storage.Streams;
using Windows.Networking;
using Windows.Networking.Sockets;
using System.Threading;

namespace Web
{

    internal static class LogOnHelper
    {
        private static readonly Uri logOnUri = new Uri("http://net.tsinghua.edu.cn/do_login.php");
        private static readonly Uri srunUri = new Uri("http://166.111.204.120:69/cgi-bin/srun_portal");
        private static readonly Uri useregUri = new Uri("http://usereg.tsinghua.edu.cn/do.php");
        private static DatagramSocket udpClient;

        private static AutoResetEvent waiter = new AutoResetEvent(false);

        private static async Task sendUdpRequest(string userName)
        {
            udpClient = new DatagramSocket();
            udpClient.MessageReceived += UdpClient_MessageReceived;
            await udpClient.ConnectAsync(new HostName("166.111.204.120"), "3335");

            long type = -100L;
            long user_id = -1L;
            using(var dataWriter = new DataWriter(udpClient.OutputStream)
            {
                ByteOrder = ByteOrder.LittleEndian
            })
            {
                dataWriter.WriteInt64(type);
                dataWriter.WriteInt64(user_id);
                var byteUserName = userName.Select(c => (byte)c).ToArray();
                dataWriter.WriteBytes(byteUserName);
                dataWriter.WriteBytes(new byte[56 - 16 - byteUserName.Length]);
                var i = await dataWriter.StoreAsync();
            }

        }

        static long type;
        static long userID;
        static byte[] challenge;

        private static void UdpClient_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            using(var reader = args.GetDataReader())
            {
                reader.ByteOrder = ByteOrder.LittleEndian;
                type = reader.ReadInt64();
                userID = reader.ReadInt64();
                challenge = new byte[16];
                reader.ReadBytes(challenge);
                var padding = new byte[32];
                reader.ReadBytes(padding);
                sender.Dispose();
                udpClient = null;
                waiter.Set();
            }
        }

        /// <summary>
        /// 检测当前客户端在线情况。
        /// </summary>
        /// <param name="http">使用的连接</param>
        /// <returns>在线情况</returns>
        public static IAsyncOperation<bool> CheckOnline(HttpClient http)
        {
            return Run(async token =>
            {
                try
                {
                    var action = http.PostStrAsync(logOnUri, "action=check_online");
                    token.Register(action.Cancel);
                    var result = await action;
                    return "online" == result;
                }
                catch(OperationCanceledException) { throw; }
                catch(Exception ex)
                {
                    throw new LogOnException(LogOnExceptionType.ConnectError, ex);
                }
            });
        }

        /// <summary>
        /// 登陆当前客户端。
        /// </summary>
        /// <param name="http">使用的连接</param>
        /// <param name="userName">用户名</param>
        /// <param name="passwordMd5">加密后的密码</param>
        /// <exception cref="LogOnException">登陆失败</exception>
        public static IAsyncAction LogOn(HttpClient http, string userName, string passwordMd5)
        {
            return Run(async token =>
            {
                try
                {
                    await sendUdpRequest(userName).ConfigureAwait(false);
                    if(!waiter.WaitOne(1000))
                    {
                        throw new LogOnException(LogOnExceptionType.ConnectError);
                    }
                    if(type != -101L)
                    {
                        throw new LogOnException(LogOnExceptionType.AuthError);
                    }
                    var pass = new byte[49];
                    pass[0] = ((byte)(int)(userID & 0xFF));
                    for(int i = 0; i < 32; i++)
                    {
                        pass[(i + 1)] = ((byte)passwordMd5[i]);
                    }
                    Array.Copy(challenge, 0, pass, 33, challenge.Length);
                    var sendPass = MD5Helper.GetMd5Hash(pass);
                    var action = http.PostStrAsync(srunUri, $"action=login&username={userName}&password={sendPass}&drop=0&type=11&n=120&ac_id=1&mac={MacAddress.Current}&chap=1");
                    token.Register(action.Cancel);
                    var res = await action;
                    if(res.StartsWith("login_error"))
                        throw LogOnException.GetByErrorString(res.Substring(res.IndexOf('#') + 1));

                    //模拟网页方式登陆相关代码

                    //action = http.PostStrAsync(logOnUri, $"action=login&username={userName}&password={{MD5_HEX}}{passwordMd5}&type=1&ac_id=1&mac={MacAddress.Current}");
                    //res = await action;
                    //if(!res.StartsWith("E"))
                    //    return;
                    //else
                    //    throw LogOnException.GetByErrorString(res);
                }
                catch(OperationCanceledException) { throw; }
                catch(LogOnException) { throw; }
                catch(Exception ex)
                {
                    throw new LogOnException(LogOnExceptionType.ConnectError, ex);
                }
            });
        }

        /// <summary>
        /// 登陆到 Usereg。
        /// </summary>
        /// <param name="http">使用的连接</param>
        /// <param name="userName">用户名</param>
        /// <param name="passwordMd5">加密后的密码</param>
        private static IAsyncAction signIn(HttpClient http, string userName, string passwordMd5)
        {
            return Run(async token =>
            {
                var postAction = http.PostStrAsync(useregUri, $"action=login&user_login_name={userName}&user_password={passwordMd5}");
                token.Register(postAction.Cancel);
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
            });
        }

        public static IAsyncAction SignInUsereg(HttpClient http, string userName, string passwordMd5)
        {
            return Run(async token =>
            {
                var signInAction = signIn(http, userName, passwordMd5);
                token.Register(() => signInAction?.Cancel());
                try
                {
                    await signInAction;
                }
                catch(LogOnException ex) when(ex.ExceptionType == LogOnExceptionType.UnknownError)
                {
                    await Task.Delay(500);
                    signInAction = signIn(http, userName, passwordMd5);
                    await signInAction;//重试
                }
                catch(LogOnException) { throw; }
                catch(OperationCanceledException) { throw; }
                catch(Exception ex)
                {
                    throw new LogOnException(LogOnExceptionType.ConnectError, ex);
                }
            });
        }

    }
}
