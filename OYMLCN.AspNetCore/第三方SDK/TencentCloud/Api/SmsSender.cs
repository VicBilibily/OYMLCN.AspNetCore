using Microsoft.Extensions.Configuration;

namespace OYMLCN.AspNetCore.TencentCloud
{
    /// <summary>
    /// SmsSender for AspNetCore
    /// </summary>
    public class SmsSender : OYMLCN.TencentCloud.SmsSender
    {
        /// <summary>
        /// SmsSender for AspNetCore
        /// </summary>
        /// <param name="sdkAppId"></param>
        /// <param name="appKey"></param>
        public SmsSender(int sdkAppId, string appKey) : base(sdkAppId, appKey)
        {
        }

        /// <summary>
        /// 采用注入方式初始化
        /// 需要在配置文件加入如下参数
        /// int TencentCloud:SMS:SDKAppID
        /// string TencentCloud:SMS:AppKey
        /// </summary>
        /// <param name="configuration"></param>
        public SmsSender(IConfiguration configuration) : base(
            configuration.GetValue<int>("TencentCloud:SMS:SDKAppID"),
            configuration.GetValue<string>("TencentCloud:SMS:AppKey")
            )
        { }
    }
}
