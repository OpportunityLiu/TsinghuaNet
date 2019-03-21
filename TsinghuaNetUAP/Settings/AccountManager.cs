using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web;
using Windows.Security.Credentials;

namespace Settings
{
    public static class AccountManager
    {
        private const string RESOURCE_NAME = "TsinghuaAllInOne";
        private const string ACCOUNT_NAME = "Account";

        private static PasswordCredential _CreatePasswordStore(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException(userName);
            password = password ?? "";
            return new PasswordCredential(RESOURCE_NAME, userName, password);
        }

        private static PasswordCredential Password
        {
            get
            {
                var passVault = new PasswordVault();
                try
                {
                    var pass = passVault.FindAllByResource(RESOURCE_NAME).First();
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
                    var oldPass = passVault.FindAllByResource(RESOURCE_NAME).First();
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

        private static AccountInfo Account
        {
            get
            {
                var json = SettingsHelper.GetRoaming(ACCOUNT_NAME, "");
                if (string.IsNullOrEmpty(json))
                    return null;
                try
                {
                    return JsonConvert.DeserializeObject<AccountInfo>(json);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                if (value is null)
                    SettingsHelper.SetRoaming(ACCOUNT_NAME, "");
                else
                    SettingsHelper.SetRoaming(ACCOUNT_NAME, JsonConvert.SerializeObject(value));
            }
        }

        public static void Save(AccountInfo account, string password)
        {
            Account = account;
            if (!string.IsNullOrEmpty(password) && !(account is null))
                Password = _CreatePasswordStore(account.UserName, password);
        }

        public static Tuple<AccountInfo, string> Load()
        {
            var acc = Account;
            var pass = Password;
            if (acc is null || pass is null || acc.UserName != pass.UserName)
                return null;
            pass.RetrievePassword();
            return Tuple.Create(acc, pass.Password);
        }
    }
}
