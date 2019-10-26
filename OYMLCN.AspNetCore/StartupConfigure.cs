using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text;
using OYMLCN.Extensions;
using System.Net;
using OYMLCN.AspNetCore;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// StartupConfigureExtension
    /// </summary>
    public static partial class StartupConfigureExtensions
    {
        /// <summary>
        /// 一句配置Session和登陆Cookie
        /// 需在Configure中加入 app.UseSessionAndAuthentication() 以使得登陆配置生效 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="sessionTimeOutHours">Session过期回收时间（默认2小时）</param>
        /// <param name="loginPath">用户登陆路径</param>
        /// <param name="accessDeniedPath">禁止访问路径，不设置则回到登陆页</param>
        /// <param name="returnUrlParameter">上一页面地址回传参数</param>
        /// <param name="cookieDomain">Cookie作用域</param>
        /// <param name="securePolicy">Cookie安全策略</param>
        /// <returns></returns>
        public static IServiceCollection AddSessionAndCookie(this IServiceCollection services,
            double sessionTimeOutHours = 2,
            string loginPath = "/Account/Login",
            string accessDeniedPath = null,
            string returnUrlParameter = "ReturnUrl",
            string cookieDomain = null,
            CookieSecurePolicy securePolicy = CookieSecurePolicy.SameAsRequest)
        {
            services.AddMemoryCache();
            services
                .AddSession(options =>
                {
                    var cookie = options.Cookie;
                    cookie.HttpOnly = true;
                    cookie.SecurePolicy = securePolicy;
                    options.IdleTimeout = TimeSpan.FromHours(sessionTimeOutHours);
                })
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = new PathString(loginPath);
                    options.ReturnUrlParameter = returnUrlParameter;
                    options.AccessDeniedPath = new PathString(accessDeniedPath ?? loginPath);

                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = securePolicy;
                    options.Cookie.Domain = cookieDomain;
                });

            return services;
        }
#if NETCOREAPP3_0
        /// <summary>
        /// 添加字符转化器，以避免中文被编码
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddHtmlEncoder(this IServiceCollection services)
            => services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));
        /// <summary>
        /// 添加自定义的默认JSON序列化配置
        /// </summary>
        /// <param name="mvcBuilder"></param>
        /// <returns></returns>
        public static IMvcBuilder AddDefaultJsonOptions(this IMvcBuilder mvcBuilder)
        {
            var defaultOptions = JsonExtensions.DefaultOptions;
            mvcBuilder.AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.WriteIndented = defaultOptions.WriteIndented;
                options.JsonSerializerOptions.IgnoreNullValues = defaultOptions.IgnoreNullValues;
                options.JsonSerializerOptions.Encoder = defaultOptions.Encoder;
            });
            return mvcBuilder;
        }
#endif
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
        /// 开启腾讯CDN加速请求头识别
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseQcloudForwardedHeaders(this IApplicationBuilder app) =>
              app.UseForwardedHeaders(new ForwardedHeadersOptions
              {
                  ForwardedForHeaderName = "X-Forwarded-For",
                  ForwardedProtoHeaderName = "X-Forwarded-Proto",
                  OriginalForHeaderName = "X-Original-For",
                  OriginalProtoHeaderName = "X-Original-Proto",
                  ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
              });
        /// <summary>
        /// 启用Session缓存和身份验证
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseSessionAndAuthentication(this IApplicationBuilder app) => app.UseSession().UseAuthentication();

        /// <summary>
        /// 返回异常Json数据信息，适用于纯接口服务
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseJsonResultForStatusCodeAndException(this IApplicationBuilder app) =>
              app.UseStatusCodePages(context =>
              {
                  var response = context.HttpContext.Response;
                  response.ContentType = "application/json";
                  return response.WriteAsync(new
                  {
                      code = response.StatusCode,
                      msg = $"{response.StatusCode.ToString()} {((HttpStatusCode)response.StatusCode).ToString()}"
                  }.ToJsonString(), Encoding.UTF8);
              }).UseExceptionHandler(config =>
              {
                  config.Run(handler =>
                  {
                      var err = handler.Features.Get<IExceptionHandlerFeature>();
                      handler.Response.StatusCode = 200;
                      handler.Response.ContentType = "application/json";
                      return handler.Response.WriteAsync(new
                      {
                          code = 500,
                          msg = err.Error.Message
                      }.ToJsonString(), Encoding.UTF8);
                  });
              });

    }
}