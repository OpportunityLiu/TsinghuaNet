using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Resources;
using System.Text.RegularExpressions;

namespace Web
{
    /// <summary>
    /// 表示在登陆过程中发生的错误。
    /// </summary>
    public class LogOnException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="LogOnException"/> 类的新实例。
        /// </summary>
        public LogOnException()
        {
        }

        /// <summary>
        /// 使用指定的错误类型初始化 <see cref="LogOnException"/> 类的新实例。
        /// </summary>
        /// <param name="exceptionType">描述错误的类型。</param>
        public LogOnException(LogOnExceptionType exceptionType)
            : base(logOnErrorMessageDict[exceptionType])
        {
            this.ExceptionType = exceptionType;
        }

        /// <summary>
        /// 使用指定的错误类型初始化 <see cref="LogOnException"/> 类的新实例。
        /// </summary>
        /// <param name="exceptionType">描述错误的类型。</param>
        /// <param name="inner">
        /// 导致当前异常的异常；如果未指定内部异常，则是一个 <c>null</c> 引用（在 Visual Basic 中为 <c>Nothing</c>）。
        /// </param>
        public LogOnException(LogOnExceptionType exceptionType, Exception inner)
            : base(logOnErrorMessageDict[exceptionType], inner)
        {
            this.ExceptionType = exceptionType;
        }

        /// <summary>
        /// 使用指定的错误信息初始化 <see cref="LogOnException"/> 类的新实例。
        /// </summary>
        /// <param name="message">描述错误的消息。</param>
        public LogOnException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// 使用指定错误消息和对作为此异常原因的内部异常的引用来初始化 <see cref="LogOnException"/> 类的新实例。
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
            return GetByErrorString(error, null);
        }

        public static LogOnException GetByErrorString(string error, Exception inner)
        {
            if(!error.StartsWith("E", StringComparison.OrdinalIgnoreCase))
                return new LogOnException(error, inner);
            else
            {
                var r = Regex.Match(error, @"^[eE](\d+).+$");
                var message = LocalizedStrings.Errors.GetString($"E{r.Groups[1].Value}");
                error = string.IsNullOrEmpty(message) ? error : message;
                return new LogOnException(error, inner) { ExceptionType = LogOnExceptionType.AuthError };
            }
        }

        private static readonly Dictionary<LogOnExceptionType, string> logOnErrorMessageDict = new Dictionary<LogOnExceptionType, string>()
        {
            [LogOnExceptionType.UnknownError] = LocalizedStrings.Errors.UnknownError,
            [LogOnExceptionType.ConnectError] = LocalizedStrings.Errors.ConnectError,
            [LogOnExceptionType.UserNameError] = LocalizedStrings.Errors.UserNameError,
            [LogOnExceptionType.PasswordError] = LocalizedStrings.Errors.PasswordError,
            [LogOnExceptionType.AuthError] = LocalizedStrings.Errors.AuthError
        };

        public LogOnExceptionType ExceptionType
        {
            get;
            protected set;
        }
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
        /// 认证过程中的其他错误。
        /// </summary>
        AuthError
    }
}