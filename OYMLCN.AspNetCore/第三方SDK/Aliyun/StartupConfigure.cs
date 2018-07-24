using Microsoft.Extensions.DependencyInjection;
using OYMLCN.AspNetCore.Aliyun;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// StartupConfigureExtension
    /// </summary>
    public static partial class StartupConfigureExtensions
    {
        /// <summary>
        /// 注入腾讯云服务SDK
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAliyunScopeds(this IServiceCollection services)
        {
            services
                .AddScoped<SmsSender>();
            return services;
        }
    }
}
