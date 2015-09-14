using Windows.ApplicationModel.Resources;

namespace NotificationService.Strings
{    
    public static class Resources
    {
        private static readonly ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("NotificationService/Resources");

        public static string GetString(string resourceKey) => loader.GetString(resourceKey);

        /// <summary>
        /// 当前没有设备在线。
        /// </summary>
        public static string NoDevices
        {
            get;
        } = loader.GetString("NoDevices"); 

        /// <summary>
        /// 已用流量：{0}
        /// </summary>
        public static string Usage
        {
            get;
        } = loader.GetString("Usage"); 
    }

}
