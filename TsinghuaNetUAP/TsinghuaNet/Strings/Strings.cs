namespace TsinghuaNet.LocalizedStrings
{    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ResourceGenerator","1.0")]
    public static class Errors
    {
        private static readonly global::System.Collections.Generic.Dictionary<string, string> cache 
            = new global::System.Collections.Generic.Dictionary<string, string>();

        private static readonly global::Windows.ApplicationModel.Resources.ResourceLoader loader 
            = global::Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("/Errors");

        public static string GetString(string resourceKey)
        {
            string value;
            if(cache.TryGetValue(resourceKey, out value))
                return value;
            else
                return cache[resourceKey] = loader.GetString(resourceKey);
        }

        public static void ClearCache()
        {
            cache.Clear();
        }

        /// <summary>
        /// 用户名或密码错误。
        /// </summary>
        public static string AuthError => GetString("AuthError");

        /// <summary>
        /// 连接错误。
        /// </summary>
        public static string ConnectError => GetString("ConnectError");

        /// <summary>
        /// 请输入密码。
        /// </summary>
        public static string EmptyPassword => GetString("EmptyPassword");

        /// <summary>
        /// 请输入用户名。
        /// </summary>
        public static string EmptyUserName => GetString("EmptyUserName");
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ResourceGenerator","1.0")]
    public static class Resources
    {
        private static readonly global::System.Collections.Generic.Dictionary<string, string> cache 
            = new global::System.Collections.Generic.Dictionary<string, string>();

        private static readonly global::Windows.ApplicationModel.Resources.ResourceLoader loader 
            = global::Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("/Resources");

        public static string GetString(string resourceKey)
        {
            string value;
            if(cache.TryGetValue(resourceKey, out value))
                return value;
            else
                return cache[resourceKey] = loader.GetString(resourceKey);
        }

        public static void ClearCache()
        {
            cache.Clear();
        }

        /// <summary>
        /// 版本 {0}.{1}.{2}.{3}
        /// </summary>
        public static string AppVersionFormat => GetString("AppVersionFormat");

        /// <summary>
        /// 取消
        /// </summary>
        public static string Cancel => GetString("Cancel");

        /// <summary>
        /// 下线失败，请刷新后再试
        /// </summary>
        public static string DropFailed => GetString("DropFailed");

        /// <summary>
        /// 下线成功
        /// </summary>
        public static string DropSuccess => GetString("DropSuccess");

        /// <summary>
        /// 错误
        /// </summary>
        public static string Error => GetString("Error");

        /// <summary>
        /// 确定
        /// </summary>
        public static string Ok => GetString("Ok");

        /// <summary>
        /// Opportunity
        /// </summary>
        public static string PackageAuthor => GetString("PackageAuthor");

        /// <summary>
        /// Tsinghua Net 是一个第三方的清华大学校园网认证客户端。
        /// </summary>
        public static string PackageDescription => GetString("PackageDescription");

        /// <summary>
        /// Tsinghua Net
        /// </summary>
        public static string PackageName => GetString("PackageName");

        /// <summary>
        /// 登陆失败
        /// </summary>
        public static string ToastFailed => GetString("ToastFailed");

        /// <summary>
        /// 登陆成功
        /// </summary>
        public static string ToastSuccess => GetString("ToastSuccess");
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ResourceGenerator","1.0")]
    public static class Toast
    {
        private static readonly global::System.Collections.Generic.Dictionary<string, string> cache 
            = new global::System.Collections.Generic.Dictionary<string, string>();

        private static readonly global::Windows.ApplicationModel.Resources.ResourceLoader loader 
            = global::Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("/Toast");

        public static string GetString(string resourceKey)
        {
            string value;
            if(cache.TryGetValue(resourceKey, out value))
                return value;
            else
                return cache[resourceKey] = loader.GetString(resourceKey);
        }

        public static void ClearCache()
        {
            cache.Clear();
        }

        /// <summary>
        /// &lt;toast&gt;
        ///     &lt;visual&gt;
        ///         &lt;binding template='ToastGeneric'&gt;
        ///             &lt;text&gt;下载失败&lt;/text&gt;
        ///             &lt;text&gt;{0}&lt;/text&gt;
        ///         &lt;/binding&gt;
        ///     &lt;/visual&gt;
        /// &lt;/toast&gt;
        /// </summary>
        public static string DownloadFailed => GetString("DownloadFailed");

        /// <summary>
        /// &lt;toast&gt;
        ///   &lt;visual&gt;
        ///     &lt;binding template=&amp;quot;ToastGeneric&amp;quot;&gt;
        ///       &lt;text&gt;下载完成&lt;/text&gt;
        ///       &lt;text&gt;{0}&lt;/text&gt;
        ///     &lt;/binding&gt;
        ///   &lt;/visual&gt;
        /// &lt;/toast&gt;
        /// </summary>
        public static string DownloadSucceed => GetString("DownloadSucceed");
    }

}
