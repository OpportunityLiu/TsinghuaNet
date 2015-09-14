using Windows.ApplicationModel.Resources;

namespace TsinghuaNet.Strings
{    
    public static class Resources
    {
        private static readonly ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("/Resources");

        public static string GetString(string resourceKey) => loader.GetString(resourceKey);

        /// <summary>
        /// 版本 {0}.{1}.{2}.{3}
        /// </summary>
        public static string AppVersionFormat
        {
            get;
        } = loader.GetString("AppVersionFormat"); 

        /// <summary>
        /// 取消
        /// </summary>
        public static string Cancel
        {
            get;
        } = loader.GetString("Cancel"); 

        /// <summary>
        /// 请输入密码。
        /// </summary>
        public static string EmptyPassword
        {
            get;
        } = loader.GetString("EmptyPassword"); 

        /// <summary>
        /// 请输入用户名。
        /// </summary>
        public static string EmptyUserName
        {
            get;
        } = loader.GetString("EmptyUserName"); 

        /// <summary>
        /// 错误
        /// </summary>
        public static string Error
        {
            get;
        } = loader.GetString("Error"); 

        /// <summary>
        /// 确定
        /// </summary>
        public static string Ok
        {
            get;
        } = loader.GetString("Ok"); 

        /// <summary>
        /// Opportunity
        /// </summary>
        public static string PackageAuthor
        {
            get;
        } = loader.GetString("PackageAuthor"); 

        /// <summary>
        /// Tsinghua Net 是一个第三方的清华大学校园网认证客户端。
        /// </summary>
        public static string PackageDescription
        {
            get;
        } = loader.GetString("PackageDescription"); 

        /// <summary>
        /// Tsinghua Net
        /// </summary>
        public static string PackageName
        {
            get;
        } = loader.GetString("PackageName"); 

        /// <summary>
        /// 登陆失败
        /// </summary>
        public static string ToastFailed
        {
            get;
        } = loader.GetString("ToastFailed"); 

        /// <summary>
        /// 登陆成功
        /// </summary>
        public static string ToastSuccess
        {
            get;
        } = loader.GetString("ToastSuccess"); 
    }

}
