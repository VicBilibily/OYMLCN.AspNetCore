using Microsoft.Extensions.Configuration;

namespace OYMLCN.AspNetCore.Aliyun
{
    /// <summary>
    /// SmsSender for AspNetCore
    /// </summary>
    public class SmsSender : OYMLCN.Aliyun.SmsSender
    {
        /// <summary>
        /// SmsSender for AspNetCore
        /// </summary>
        /// <param name="accessKeyId"></param>
        /// <param name="accessKeySecret"></param>
        public SmsSender(string accessKeyId, string accessKeySecret) : base(accessKeyId, accessKeySecret)
        {
        }

        /// <summary>
        /// 采用注入方式初始化
        /// 需要在配置文件加入如下参数
        /// string Aliyun:AccessKeyID
        /// string Aliyun:AccessKeySecret
        /// </summary>
        /// <param name="configuration"></param>
        public SmsSender(IConfiguration configuration) :
            this(configuration.GetValue<string>("Aliyun:AccessKeyID"),
                configuration.GetValue<string>("Aliyun:AccessKeySecret"))
        { }
    }
}
