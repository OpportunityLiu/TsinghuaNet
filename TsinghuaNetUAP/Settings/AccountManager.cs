using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace Settings
{
    public static class AccountManager
    {
        private const string resourceName = "TsinghuaAllInOne";

        public static PasswordCredential CreateAccount(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException(userName);
            password = password ?? "";
            return new PasswordCredential(resourceName, userName, password);
        }

        public static PasswordCredential Account
        {
            get
            {
                var passVault = new PasswordVault();
                try
                {
                    var pass = passVault.FindAllByResource(resourceName).First();
                    return pass;
                }
                // 未找到储存的密码
                catch (Exception ex) when (ex.HResult == -2147023728)
                {
                    return null;
                }
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                if (string.IsNullOrEmpty(value.UserName))
                    throw new ArgumentException("no user name", nameof(value));
                var passVault = new PasswordVault();
                try
                {
                    var oldPass = passVault.FindAllByResource(resourceName).First();
                    passVault.Remove(oldPass);
                }
                // 未找到储存的密码
                catch (Exception ex) when (ex.HResult == -2147023728)
                {
                }
                passVault.Add(value);
                SettingsHelper.SetRoaming("ID", "");
            }
        }

        public static string ID
        {
            get => SettingsHelper.GetRoaming("ID", "");
            set => SettingsHelper.SetRoaming("ID", value ?? "");
        }
    }
}
