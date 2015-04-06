using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks
{
    internal static class ErrorCode
    {  
        private static Dictionary<string,string> initErrorCode()
        {
            var re = new Dictionary<string, string>();
            re.Add("username_error", "用户名错误");
            re.Add("password_error", "密码错误");
            re.Add("user_tab_error", "认证程序未启动");
            re.Add("user_group_error", "您的计费组信息不正确");
            re.Add("non_auth_error", "您无须认证，可直接上网");
            re.Add("status_error", "用户已欠费，请尽快充值。");
            re.Add("available_error", "您的帐号已停用");
            re.Add("delete_error", "您的帐号已删除");
            re.Add("ip_exist_error", "IP已存在，请稍后再试。");
            re.Add("usernum_error", "用户数已达上限");
            re.Add("online_num_error", "该帐号的登录人数已超过限额");
            re.Add("mode_error", "系统已禁止WEB方式登录，请使用客户端");
            re.Add("time_policy_error", "当前时段不允许连接");
            re.Add("flux_error", "您的流量已超支");
            re.Add("minutes_error", "您的时长已超支");
            re.Add("ip_error", "您的 IP 地址不合法");
            re.Add("mac_error", "您的 MAC 地址不合法");
            re.Add("sync_error", "您的资料已修改，正在等待同步，请 2 分钟后再试。");
            re.Add("ip_alloc", "您不是这个地址的合法拥有者，IP 地址已经分配给其它用户。");
            re.Add("ip_invaild", "您是区内地址，无法使用。");
            return re;
        }

        private static Dictionary<string, string> error = initErrorCode();

        public static Dictionary<string, string> ErrorDict
        {
            get
            {
                return error;
            }
        }
    }
}
