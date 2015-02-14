using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TsinghuaNet
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
            var dict = new Dictionary<LogOnExceptionType, string>();
            dict.Add(LogOnExceptionType.connect_error, "连接错误。");
            dict.Add(LogOnExceptionType.username_error, "用户名错误。");
            dict.Add(LogOnExceptionType.password_error, "密码错误。");
            dict.Add(LogOnExceptionType.user_tab_error, "认证程序未启动。");
            dict.Add(LogOnExceptionType.user_group_error, "您的计费组信息不正确。");
            dict.Add(LogOnExceptionType.non_auth_error, "您无须认证，可直接上网。");
            dict.Add(LogOnExceptionType.status_error, "用户已欠费，请尽快充值。");
            dict.Add(LogOnExceptionType.available_error, "您的帐号已停用。");
            dict.Add(LogOnExceptionType.delete_error, "您的帐号已删除。");
            dict.Add(LogOnExceptionType.ip_exist_error, "IP已存在，请稍后再试。");
            dict.Add(LogOnExceptionType.usernum_error, "用户数已达上限。");
            dict.Add(LogOnExceptionType.online_num_error, "该帐号的登录人数已超过限额，请登录https://usereg.tsinghua.edu.cn断开不用的连接。");
            dict.Add(LogOnExceptionType.mode_error, "系统已禁止WEB方式登录，请使用客户端。");
            dict.Add(LogOnExceptionType.time_policy_error, "当前时段不允许连接。");
            dict.Add(LogOnExceptionType.minutes_error, "您的时长已超支。");
            dict.Add(LogOnExceptionType.ip_error, "您的 IP 地址不合法。");
            dict.Add(LogOnExceptionType.mac_error, "您的 MAC 地址不合法。");
            dict.Add(LogOnExceptionType.sync_error, "您的资料已修改，正在等待同步，请 2 分钟后再试。");
            dict.Add(LogOnExceptionType.ip_alloc, "您不是这个地址的合法拥有者，IP 地址已经分配给其它用户。");
            dict.Add(LogOnExceptionType.ip_invaild, "您是区内地址，无法使用。");
            dict.Add(LogOnExceptionType.unknown, "未知错误。");
            return dict;
        }

        private static ReadOnlyDictionary<string, LogOnExceptionType> logOnErrorDict = new ReadOnlyDictionary<string, LogOnExceptionType>(initLogOnErrorDict());

        private static Dictionary<string, LogOnExceptionType> initLogOnErrorDict()
        {
            var dict = new Dictionary<string, LogOnExceptionType>();
            dict.Add("connect_error", LogOnExceptionType.connect_error);
            dict.Add("username_error", LogOnExceptionType.username_error);
            dict.Add("password_error", LogOnExceptionType.password_error);
            dict.Add("user_tab_error", LogOnExceptionType.user_tab_error);
            dict.Add("user_group_error", LogOnExceptionType.user_group_error);
            dict.Add("non_auth_error", LogOnExceptionType.non_auth_error);
            dict.Add("status_error", LogOnExceptionType.status_error);
            dict.Add("available_error", LogOnExceptionType.available_error);
            dict.Add("delete_error", LogOnExceptionType.delete_error);
            dict.Add("ip_exist_error", LogOnExceptionType.ip_exist_error);
            dict.Add("usernum_error", LogOnExceptionType.usernum_error);
            dict.Add("online_num_error", LogOnExceptionType.online_num_error);
            dict.Add("mode_error", LogOnExceptionType.mode_error);
            dict.Add("time_policy_error", LogOnExceptionType.time_policy_error);
            dict.Add("flux_error", LogOnExceptionType.flux_error);
            dict.Add("minutes_error", LogOnExceptionType.minutes_error);
            dict.Add("ip_error", LogOnExceptionType.ip_error);
            dict.Add("mac_error", LogOnExceptionType.mac_error);
            dict.Add("sync_error", LogOnExceptionType.sync_error);
            dict.Add("ip_alloc", LogOnExceptionType.ip_alloc);
            dict.Add("ip_invaild", LogOnExceptionType.ip_invaild);
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
        unknown = 0,
        /// <summary>
        /// 连接错误。
        /// </summary>
        connect_error,
        /// <summary>
        /// 用户名错误。
        /// </summary>
        username_error,
        /// <summary>
        /// 密码错误。
        /// </summary>
        password_error,
        /// <summary>
        /// 认证程序未启动。
        /// </summary>
        user_tab_error,
        /// <summary>
        /// 您的计费组信息不正确。
        /// </summary>
        user_group_error,
        /// <summary>
        /// 您无须认证，可直接上网。
        /// </summary>
        non_auth_error,
        /// <summary>
        /// 用户已欠费，请尽快充值。
        /// </summary>
        status_error,
        /// <summary>
        /// 您的帐号已停用。
        /// </summary>
        available_error,
        /// <summary>
        /// 您的帐号已删除。
        /// </summary>
        delete_error,
        /// <summary>
        /// IP已存在，请稍后再试。
        /// </summary>
        ip_exist_error,
        /// <summary>
        /// 用户数已达上限。
        /// </summary>
        usernum_error,
        /// <summary>
        /// 该帐号的登录人数已超过限额。
        /// </summary>
        online_num_error,
        /// <summary>
        /// 系统已禁止WEB方式登录，请使用客户端。
        /// </summary>
        mode_error,
        /// <summary>
        /// 当前时段不允许连接。
        /// </summary>
        time_policy_error,
        /// <summary>
        /// 您的流量已超支。
        /// </summary>
        flux_error,
        /// <summary>
        /// 您的时长已超支。
        /// </summary>
        minutes_error,
        /// <summary>
        /// 您的 IP 地址不合法。
        /// </summary>
        ip_error,
        /// <summary>
        /// 您的 MAC 地址不合法。
        /// </summary>
        mac_error,
        /// <summary>
        /// 您的资料已修改，正在等待同步，请 2 分钟后再试。
        /// </summary>
        sync_error,
        /// <summary>
        /// 您不是这个地址的合法拥有者，IP 地址已经分配给其它用户。
        /// </summary>
        ip_alloc,
        /// <summary>
        /// 您是区内地址，无法使用。
        /// </summary>
        ip_invaild
    }
}