namespace Web.LocalizedStrings
{    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ResourceGenerator","1.0")]
    public static class Errors
    {
        private static readonly global::System.Collections.Generic.Dictionary<string, string> cache 
            = new global::System.Collections.Generic.Dictionary<string, string>();

        private static readonly global::Windows.ApplicationModel.Resources.ResourceLoader loader 
            = global::Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("Web/Errors");

        public static string GetString(string resourceKey)
        {
            string value = null;
            if(cache.TryGetValue(resourceKey, out value))
                return value;
            else
                return cache[resourceKey] = loader.GetString(resourceKey);
        }

        /// <summary>
        /// 流量或时长已用尽。
        /// </summary>
        public static string E3001
        {
            get
            {
                return GetString("E3001");
            }
        }

        /// <summary>
        /// 计费策略条件不匹配。
        /// </summary>
        public static string E3002
        {
            get
            {
                return GetString("E3002");
            }
        }

        /// <summary>
        /// 控制策略条件不匹配。
        /// </summary>
        public static string E3003
        {
            get
            {
                return GetString("E3003");
            }
        }

        /// <summary>
        /// 余额不足。
        /// </summary>
        public static string E3004
        {
            get
            {
                return GetString("E3004");
            }
        }

        /// <summary>
        /// 用户不存在。
        /// </summary>
        public static string E2531
        {
            get
            {
                return GetString("E2531");
            }
        }

        /// <summary>
        /// 两次认证的间隔太短。
        /// </summary>
        public static string E2532
        {
            get
            {
                return GetString("E2532");
            }
        }

        /// <summary>
        /// 尝试次数过于频繁。
        /// </summary>
        public static string E2533
        {
            get
            {
                return GetString("E2533");
            }
        }

        /// <summary>
        /// 密码错误。
        /// </summary>
        public static string E2553
        {
            get
            {
                return GetString("E2553");
            }
        }

        /// <summary>
        /// 不是专用客户端。
        /// </summary>
        public static string E2601
        {
            get
            {
                return GetString("E2601");
            }
        }

        /// <summary>
        /// 用户被禁用。
        /// </summary>
        public static string E2606
        {
            get
            {
                return GetString("E2606");
            }
        }

        /// <summary>
        /// MAC 绑定错误。
        /// </summary>
        public static string E2611
        {
            get
            {
                return GetString("E2611");
            }
        }

        /// <summary>
        /// NAS PORT 绑定错误。
        /// </summary>
        public static string E2613
        {
            get
            {
                return GetString("E2613");
            }
        }

        /// <summary>
        /// 已欠费。
        /// </summary>
        public static string E2616
        {
            get
            {
                return GetString("E2616");
            }
        }

        /// <summary>
        /// 连接数已满，请登录http://usereg.tsinghua.edu.cn，选择下线您的IP地址。
        /// </summary>
        public static string E2620
        {
            get
            {
                return GetString("E2620");
            }
        }

        /// <summary>
        /// 找不到符合条件的产品。
        /// </summary>
        public static string E2806
        {
            get
            {
                return GetString("E2806");
            }
        }

        /// <summary>
        /// 找不到符合条件的计费策略。
        /// </summary>
        public static string E2807
        {
            get
            {
                return GetString("E2807");
            }
        }

        /// <summary>
        /// 找不到符合条件的控制策略。
        /// </summary>
        public static string E2808
        {
            get
            {
                return GetString("E2808");
            }
        }

        /// <summary>
        /// IP 地址异常，请重新拿地址 
        /// </summary>
        public static string E2833
        {
            get
            {
                return GetString("E2833");
            }
        }

        /// <summary>
        /// 校内地址不允许访问外网。
        /// </summary>
        public static string E2840
        {
            get
            {
                return GetString("E2840");
            }
        }

        /// <summary>
        /// IP 地址绑定错误。
        /// </summary>
        public static string E2841
        {
            get
            {
                return GetString("E2841");
            }
        }

        /// <summary>
        /// IP 地址无需认证可直接上网。
        /// </summary>
        public static string E2842
        {
            get
            {
                return GetString("E2842");
            }
        }

        /// <summary>
        /// IP 地址不在 IP 表中。
        /// </summary>
        public static string E2843
        {
            get
            {
                return GetString("E2843");
            }
        }

        /// <summary>
        /// IP 地址在黑名单中，请联系管理员。
        /// </summary>
        public static string E2844
        {
            get
            {
                return GetString("E2844");
            }
        }

        /// <summary>
        /// 第三方接口认证失败。
        /// </summary>
        public static string E2901
        {
            get
            {
                return GetString("E2901");
            }
        }

        /// <summary>
        /// 认证过程中发生错误。
        /// </summary>
        public static string AuthError
        {
            get
            {
                return GetString("AuthError");
            }
        }

        /// <summary>
        /// 连接错误。
        /// </summary>
        public static string ConnectError
        {
            get
            {
                return GetString("ConnectError");
            }
        }

        /// <summary>
        /// 密码错误。
        /// </summary>
        public static string PasswordError
        {
            get
            {
                return GetString("PasswordError");
            }
        }

        /// <summary>
        /// 未知错误。
        /// </summary>
        public static string UnknownError
        {
            get
            {
                return GetString("UnknownError");
            }
        }

        /// <summary>
        /// 用户名错误。
        /// </summary>
        public static string UserNameError
        {
            get
            {
                return GetString("UserNameError");
            }
        }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ResourceGenerator","1.0")]
    public static class Resources
    {
        private static readonly global::System.Collections.Generic.Dictionary<string, string> cache 
            = new global::System.Collections.Generic.Dictionary<string, string>();

        private static readonly global::Windows.ApplicationModel.Resources.ResourceLoader loader 
            = global::Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("Web/Resources");

        public static string GetString(string resourceKey)
        {
            string value = null;
            if(cache.TryGetValue(resourceKey, out value))
                return value;
            else
                return cache[resourceKey] = loader.GetString(resourceKey);
        }

        /// <summary>
        /// (本机)
        /// </summary>
        public static string CurrentDevice
        {
            get
            {
                return GetString("CurrentDevice");
            }
        }

        /// <summary>
        /// (未知设备)
        /// </summary>
        public static string UnknownDevice
        {
            get
            {
                return GetString("UnknownDevice");
            }
        }
    }

}
