using Microsoft.Extensions.Configuration;

namespace OYMLCN.AspNetCore.TencentCloud
{
    /// <summary>
    /// CosCloud for AspNetCore
    /// </summary>
    public class CosCloud : OYMLCN.TencentCloud.CosCloud
    {
        /// <summary>
        ///  CosCloud for AspNetCore
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="secretId"></param>
        /// <param name="secretKey"></param>
        /// <param name="timeOut"></param>
        public CosCloud(int appId, string secretId, string secretKey, int timeOut = 60) : base(appId, secretId, secretKey, timeOut)
        {
        }

        /// <summary>
        /// 采用注入方式初始化
        /// 需要在配置文件加入如下参数
        /// int TencentCloud:AppId
        /// string TencentCloud:SecretId
        /// string TencentCloud:SecretKey
        /// </summary>
        /// <param name="configuration"></param>
        public CosCloud(IConfiguration configuration) :
            base(
                configuration.GetValue<int>("TencentCloud:AppId"),
                configuration.GetValue<string>("TencentCloud:SecretId"),
                configuration.GetValue<string>("TencentCloud:SecretKey"),
                HTTP_TIMEOUT_TIME * 1000
                )
        { }
    }
}
