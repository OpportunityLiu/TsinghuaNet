namespace TsinghuaNet.LocalizedStrings
{    
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
        public static string AppVersionFormat
        {
            get
            {
                return GetString("AppVersionFormat");
            }
        }

        /// <summary>
        /// 取消
        /// </summary>
        public static string Cancel
        {
            get
            {
                return GetString("Cancel");
            }
        }

        /// <summary>
        /// 请输入密码。
        /// </summary>
        public static string EmptyPassword
        {
            get
            {
                return GetString("EmptyPassword");
            }
        }

        /// <summary>
        /// 请输入用户名。
        /// </summary>
        public static string EmptyUserName
        {
            get
            {
                return GetString("EmptyUserName");
            }
        }

        /// <summary>
        /// 错误
        /// </summary>
        public static string Error
        {
            get
            {
                return GetString("Error");
            }
        }

        /// <summary>
        /// 确定
        /// </summary>
        public static string Ok
        {
            get
            {
                return GetString("Ok");
            }
        }

        /// <summary>
        /// Opportunity
        /// </summary>
        public static string PackageAuthor
        {
            get
            {
                return GetString("PackageAuthor");
            }
        }

        /// <summary>
        /// Tsinghua Net 是一个第三方的清华大学校园网认证客户端。
        /// </summary>
        public static string PackageDescription
        {
            get
            {
                return GetString("PackageDescription");
            }
        }

        /// <summary>
        /// Tsinghua Net
        /// </summary>
        public static string PackageName
        {
            get
            {
                return GetString("PackageName");
            }
        }

        /// <summary>
        /// 登陆失败
        /// </summary>
        public static string ToastFailed
        {
            get
            {
                return GetString("ToastFailed");
            }
        }

        /// <summary>
        /// 登陆成功
        /// </summary>
        public static string ToastSuccess
        {
            get
            {
                return GetString("ToastSuccess");
            }
        }
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
        ///         &lt;/binding&gt;
        ///     &lt;/visual&gt;
        /// &lt;/toast&gt;
        /// </summary>
        public static string DownloadFailed
        {
            get
            {
                return GetString("DownloadFailed");
            }
        }

        /// <summary>
        /// &lt;toast&gt;
        ///     &lt;visual&gt;
        ///         &lt;binding template='ToastGeneric'&gt;
        ///             &lt;text&gt;下载完成&lt;/text&gt;
        ///         &lt;/binding&gt;
        ///     &lt;/visual&gt;
        /// &lt;/toast&gt;
        /// </summary>
        public static string DownloadSucceed
        {
            get
            {
                return GetString("DownloadSucceed");
            }
        }
    }

}
