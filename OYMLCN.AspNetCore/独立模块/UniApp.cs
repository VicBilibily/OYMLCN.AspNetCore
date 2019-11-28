using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using OYMLCN.AspNetCore;
using OYMLCN.Extensions;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
namespace OYMLCN.AspNetCore
{

    public class UniAppPageMidleware : IMiddleware
    {
#if NETCOREAPP3_0
        private IWebHostEnvironment web;
        public UniAppPageMidleware(IWebHostEnvironment web)
#elif NETCOREAPP2_1
        private IHostingEnvironment web;
        public UniAppPageMidleware(IHostingEnvironment web)
#endif
        {
            this.web = web;
        }
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            string userAgent = context.Request.Headers[HeaderNames.UserAgent];
            if (context.Request.Path.Value.StartsWith("/pages") || context.Request.Path.Value == "/" && Controller.IsMobileAgent(userAgent))
            {
                context.Response.Clear();
                var filePath = Path.Combine(web.WebRootPath, "index.html");
                if (File.Exists(filePath))
                    return context.Response.SendFileAsync(filePath);
            }
            return next(context);
        }
    }
}
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// StartupConfigureExtension
    /// </summary>
    public static partial class StartupConfigureExtension
    {
        /// <summary>
        /// 在服务容器中注册UniApp页面拦截器中间件
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddUniAppPages(this IServiceCollection services)
            => services.AddSingleton<UniAppPageMidleware>();
    }
}
namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// StartupConfigureExtension
    /// </summary>
    public static partial class StartupConfigureExtensions
    {
        /// <summary>
        /// 添加UniApp页面拦截器到处理管道中
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseUniAppPages(this IApplicationBuilder builder)
            => builder.UseMiddleware<UniAppPageMidleware>();
    }
}