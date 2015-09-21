namespace BackgroundLogOnTask.LocalizedStrings
{    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ResourceGenerator","1.0")]
    public static class Resources
    {
        private static readonly global::System.Collections.Generic.Dictionary<string, string> cache 
            = new global::System.Collections.Generic.Dictionary<string, string>();

        private static readonly global::Windows.ApplicationModel.Resources.ResourceLoader loader 
            = global::Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("BackgroundLogOnTask/Resources");

        public static string GetString(string resourceKey)
        {
            string value = null;
            if(global::BackgroundLogOnTask.LocalizedStrings.Resources.cache.TryGetValue(resourceKey, out value))
                return value;
            else
                return global::BackgroundLogOnTask.LocalizedStrings.Resources.cache[resourceKey] = global::BackgroundLogOnTask.LocalizedStrings.Resources.loader.GetString(resourceKey);
        }

        /// <summary>
        /// 登陆成功
        /// </summary>
        public static string LogOnSucessful
        {
            get
            {
                return global::BackgroundLogOnTask.LocalizedStrings.Resources.GetString("LogOnSucessful");
            }
        }

        /// <summary>
        /// 已用流量：{0}
        /// </summary>
        public static string Used
        {
            get
            {
                return global::BackgroundLogOnTask.LocalizedStrings.Resources.GetString("Used");
            }
        }
    }

}
