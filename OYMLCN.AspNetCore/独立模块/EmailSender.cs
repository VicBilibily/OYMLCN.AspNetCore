using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace OYMLCN.AspNetCore
{
    /// <summary>
    /// 邮件发送
    /// </summary>
    public class EmailSender : OYMLCN.EmailSender
    {
        readonly IMemoryCache MemoryCache;
        /// <summary>
        /// 邮件发送 普通模式（未加密）
        /// （注意：使用前需要AddMemoryCache启用缓存以控制邮件发送频率）
        /// （请使用AddSingleton注入方式调用，MVC框架会自动注入初始化参数）
        /// 使用该自动注入方法需要配置如下参数
        /// string EmailSender:DisplayName
        /// string EmailSender:UserName
        /// string EmailSender:SMTP
        /// string EmailSender:Password
        /// int EmailSender:Port
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="memoryCache"></param>
        public EmailSender(IConfiguration configuration, IMemoryCache memoryCache) : base()
        {
            var config = configuration.GetSection("EmailSender");
            base.DisplayName = config.GetValue<string>("DisplayName");
            base.UserName = config.GetValue<string>("UserName");
            base.SMTP = config.GetValue<string>("SMTP");
            base.Password = config.GetValue<string>("Password");
            base.Port = config.GetValue<int>("Port");
            this.MemoryCache = memoryCache;
        }
        /// <summary>
        /// 邮件发送
        /// </summary>
        /// <param name="displayName">显示名称</param>
        /// <param name="smtp">SMTP地址</param>
        /// <param name="userName">邮箱地址/用户名</param>
        /// <param name="password">密码</param>
        /// <param name="port">端口（默认25）</param>
        public EmailSender(string displayName, string smtp, string userName, string password, int port = 25) : base(displayName, smtp, userName, password, port) { }

        /// <summary>
        /// 发送邮件
        /// 用于限定发送频率
        /// </summary>
        /// <param name="iPAddress">记录IP，为null则不进行任何记录以用于测试</param>
        /// <param name="subject">主题</param>
        /// <param name="body">正文HTML</param>
        /// <param name="target">目标邮箱</param>
        /// <param name="ipLimitMinutes">IP发送限制时长（默认1分钟）</param>
        /// <param name="mailLimitMinutes">邮箱发送限制时长（默认3分钟）</param>
        /// <returns>成功返回true，否则报错</returns>
        public bool SendEmail(IPAddress iPAddress, string subject, string body, string target, byte ipLimitMinutes = 1, byte mailLimitMinutes = 3)
        {
            if (iPAddress.IsNotNull())
            {
                string ipKey = $"IP_{iPAddress}", mailKey = $"EMail_{target}";
                if (MemoryCache.Get(ipKey).IsNull() && MemoryCache.Get(mailKey).IsNull())
                {
                    MemoryCache.Set(ipKey, string.Empty, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(ipLimitMinutes)));
                    MemoryCache.Set(mailKey, string.Empty, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(mailLimitMinutes)));
                }
                else
                    throw new Exception("邮件发送过于频繁，请稍后再试");
            }
            return base.SendEmail(subject, body, target);
        }


    }
}
