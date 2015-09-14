using Windows.ApplicationModel.Resources;

namespace BackgroundLogOnTask.LocalizedStrings
{    
    public static class Resources
    {
        private static readonly ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("BackgroundLogOnTask/Resources");

        public static string GetString(string resourceKey) => loader.GetString(resourceKey);

        /// <summary>
        /// 登陆成功
        /// </summary>
        public static string LogOnSucessful
        {
            get;
        } = loader.GetString("LogOnSucessful"); 

        /// <summary>
        /// 已用流量：{0}
        /// </summary>
        public static string Used
        {
            get;
        } = loader.GetString("Used"); 
    }

}
