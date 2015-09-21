namespace NotificationService.LocalizedStrings
{    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ResourceGenerator","1.0")]
    public static class Resources
    {
        private static readonly global::System.Collections.Generic.Dictionary<string, string> cache 
            = new global::System.Collections.Generic.Dictionary<string, string>();

        private static readonly global::Windows.ApplicationModel.Resources.ResourceLoader loader 
            = global::Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("NotificationService/Resources");

        public static string GetString(string resourceKey)
        {
            string value = null;
            if(cache.TryGetValue(resourceKey, out value))
                return value;
            else
                return cache[resourceKey] = loader.GetString(resourceKey);
        }

        /// <summary>
        /// 当前没有设备在线。
        /// </summary>
        public static string NoDevices
        {
            get
            {
                return GetString("NoDevices");
            }
        }

        /// <summary>
        /// 已用流量：{0}
        /// </summary>
        public static string Usage
        {
            get
            {
                return GetString("Usage");
            }
        }
    }

}
