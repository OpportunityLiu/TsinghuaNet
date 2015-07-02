using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Resources;

namespace TsinghuaNet.Web
{
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
        /// 使用指定的错误类型初始化 <see cref="TsinghuaNet.LogOnException"/> 类的新实例。
        /// </summary>
        /// <param name="exceptionType">描述错误的类型。</param>
        public LogOnException(LogOnExceptionType exceptionType)
            : base(logOnErrorMessageDict[exceptionType])
        {
            this.exceptionType = exceptionType;
        }

        /// <summary>
        /// 使用指定的错误类型初始化 <see cref="TsinghuaNet.LogOnException"/> 类的新实例。
        /// </summary>
        /// <param name="exceptionType">描述错误的类型。</param>
        /// <param name="inner">
        /// 导致当前异常的异常；如果未指定内部异常，则是一个 <c>null</c> 引用（在 Visual Basic 中为 <c>Nothing</c>）。
        /// </param>
        public LogOnException(LogOnExceptionType exceptionType, Exception inner)
            : base(logOnErrorMessageDict[exceptionType], inner)
        {
            this.exceptionType = exceptionType;
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

        public static LogOnException GetByErrorString(string error)
        {
            LogOnExceptionType value;
            if(logOnErrorDict.TryGetValue(error, out value))
                return new LogOnException(value);
            else
                return new LogOnException(error);
        }

        public static LogOnException GetByErrorString(string error, Exception inner)
        {
            LogOnExceptionType value;
            if(logOnErrorDict.TryGetValue(error, out value))
                return new LogOnException(value, inner);
            else
                return new LogOnException(error, inner);
        }

        private static ReadOnlyDictionary<LogOnExceptionType, string> logOnErrorMessageDict = new ReadOnlyDictionary<LogOnExceptionType, string>(initLogOnErrorMessageDict());

        private static Dictionary<LogOnExceptionType, string> initLogOnErrorMessageDict()
        {
            var resource=ResourceLoader.GetForViewIndependentUse("Errors");
            Func<string, string> getResourceString = key => resource.GetString(key);
            var dict = new Dictionary<LogOnExceptionType, string>();
            dict.Add(LogOnExceptionType.ConnectError,getResourceString("ConnectError"));
            dict.Add(LogOnExceptionType.UserNameError,getResourceString("UserNameError"));
            dict.Add(LogOnExceptionType.PasswordError,getResourceString("PasswordError"));
            dict.Add(LogOnExceptionType.UserTabError,getResourceString("UserTabError"));
            dict.Add(LogOnExceptionType.UserGroupError,getResourceString("UserGroupError"));
            dict.Add(LogOnExceptionType.NonAuthError,getResourceString("NonAuthError"));
            dict.Add(LogOnExceptionType.BalanceError,getResourceString("BalanceError"));
            dict.Add(LogOnExceptionType.UnavailableError,getResourceString("UnavailableError"));
            dict.Add(LogOnExceptionType.DeletedError,getResourceString("DeletedError"));
            dict.Add(LogOnExceptionType.IPExistError,getResourceString("IPExistError"));
            dict.Add(LogOnExceptionType.UserCountError,getResourceString("UserCountError"));
            dict.Add(LogOnExceptionType.OnlineCountError,getResourceString("OnlineCountError"));
            dict.Add(LogOnExceptionType.ModeError,getResourceString("ModeError"));
            dict.Add(LogOnExceptionType.TimePolicyError,getResourceString("TimePolicyError"));
            dict.Add(LogOnExceptionType.TimeSpanLengthError,getResourceString("TimeSpanLengthError"));
            dict.Add(LogOnExceptionType.IPError,getResourceString("IPError"));
            dict.Add(LogOnExceptionType.MacAddressError,getResourceString("MacAddressError"));
            dict.Add(LogOnExceptionType.SyncError,getResourceString("SyncError"));
            dict.Add(LogOnExceptionType.IPAllocError,getResourceString("IPAllocError"));
            dict.Add(LogOnExceptionType.IPInvalidError,getResourceString("IPInvalidError"));
            dict.Add(LogOnExceptionType.UnknownError,getResourceString("UnknownError"));
            return dict;
        }

        private static ReadOnlyDictionary<string, LogOnExceptionType> logOnErrorDict = new ReadOnlyDictionary<string, LogOnExceptionType>(initLogOnErrorDict());

        private static Dictionary<string, LogOnExceptionType> initLogOnErrorDict()
        {
            var dict = new Dictionary<string, LogOnExceptionType>();
            dict.Add("connect_error", LogOnExceptionType.ConnectError);
            dict.Add("username_error", LogOnExceptionType.UserNameError);
            dict.Add("password_error", LogOnExceptionType.PasswordError);
            dict.Add("user_tab_error", LogOnExceptionType.UserTabError);
            dict.Add("user_group_error", LogOnExceptionType.UserGroupError);
            dict.Add("non_auth_error", LogOnExceptionType.NonAuthError);
            dict.Add("status_error", LogOnExceptionType.BalanceError);
            dict.Add("available_error", LogOnExceptionType.UnavailableError);
            dict.Add("delete_error", LogOnExceptionType.DeletedError);
            dict.Add("ip_exist_error", LogOnExceptionType.IPExistError);
            dict.Add("usernum_error", LogOnExceptionType.UserCountError);
            dict.Add("online_num_error", LogOnExceptionType.OnlineCountError);
            dict.Add("mode_error", LogOnExceptionType.ModeError);
            dict.Add("time_policy_error", LogOnExceptionType.TimePolicyError);
            dict.Add("flux_error", LogOnExceptionType.TrafficCountError);
            dict.Add("minutes_error", LogOnExceptionType.TimeSpanLengthError);
            dict.Add("ip_error", LogOnExceptionType.IPError);
            dict.Add("mac_error", LogOnExceptionType.MacAddressError);
            dict.Add("sync_error", LogOnExceptionType.SyncError);
            dict.Add("ip_alloc", LogOnExceptionType.IPAllocError);
            dict.Add("ip_invaild", LogOnExceptionType.IPInvalidError);
            return dict;
        }

        public LogOnExceptionType ExceptionType
        {
            get
            {
                return exceptionType;
            }
        }

        LogOnExceptionType exceptionType;
    }

    /// <summary>
    /// 表示登陆错误的类型。
    /// </summary>
    public enum LogOnExceptionType
    {
        /// <summary>
        /// 未知错误。
        /// </summary>
        UnknownError = 0,
        /// <summary>
        /// 连接错误。
        /// </summary>
        ConnectError,
        /// <summary>
        /// 用户名错误。
        /// </summary>
        UserNameError,
        /// <summary>
        /// 密码错误。
        /// </summary>
        PasswordError,
        /// <summary>
        /// 认证程序未启动。
        /// </summary>
        UserTabError,
        /// <summary>
        /// 您的计费组信息不正确。
        /// </summary>
        UserGroupError,
        /// <summary>
        /// 您无须认证，可直接上网。
        /// </summary>
        NonAuthError,
        /// <summary>
        /// 用户已欠费，请尽快充值。
        /// </summary>
        BalanceError,
        /// <summary>
        /// 您的帐号已停用。
        /// </summary>
        UnavailableError,
        /// <summary>
        /// 您的帐号已删除。
        /// </summary>
        DeletedError,
        /// <summary>
        /// IP已存在，请稍后再试。
        /// </summary>
        IPExistError,
        /// <summary>
        /// 用户数已达上限。
        /// </summary>
        UserCountError,
        /// <summary>
        /// 该帐号的登录人数已超过限额。
        /// </summary>
        OnlineCountError,
        /// <summary>
        /// 系统已禁止WEB方式登录，请使用客户端。
        /// </summary>
        ModeError,
        /// <summary>
        /// 当前时段不允许连接。
        /// </summary>
        TimePolicyError,
        /// <summary>
        /// 您的流量已超支。
        /// </summary>
        TrafficCountError,
        /// <summary>
        /// 您的时长已超支。
        /// </summary>
        TimeSpanLengthError,
        /// <summary>
        /// 您的 IP 地址不合法。
        /// </summary>
        IPError,
        /// <summary>
        /// 您的 Mac 地址不合法。
        /// </summary>
        MacAddressError,
        /// <summary>
        /// 您的资料已修改，正在等待同步，请 2 分钟后再试。
        /// </summary>
        SyncError,
        /// <summary>
        /// 您不是这个地址的合法拥有者，IP 地址已经分配给其它用户。
        /// </summary>
        IPAllocError,
        /// <summary>
        /// 您是区内地址，无法使用。
        /// </summary>
        IPInvalidError
    }
}