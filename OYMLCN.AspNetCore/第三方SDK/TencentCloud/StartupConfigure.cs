using Microsoft.Extensions.DependencyInjection;
using OYMLCN.AspNetCore.TencentCloud;

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
        public static IServiceCollection AddTencentCloudScopeds(this IServiceCollection services)
        {
            services
                .AddScoped<CosCloud>()
                .AddScoped<SmsSender>();
            return services;
        }
    }
}
