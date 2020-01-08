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
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using OYMLCN.AspNetCore;
using OYMLCN.Extensions;
using OYMLCN.Helpers;

namespace OYMLCN.AspNetCore
{
    /// <summary>
    /// UniApp页面拦截跳转配置
    /// </summary>
    public class UniAppPageOptions
    {
        private bool _RedirectAll = false;
        /// <summary>
        /// 开启全部自动跳转
        /// - 自动跳转到手机端专用域名
        /// - UniApp Page 自动跳转到 PC 页面 （在PC打开手机页面时触发）
        /// - PC页面 自动跳转到 UniApp Page （在手机打开PC端页面时触发）
        /// </summary>
        public bool AutoRedirect
        {
            get => _RedirectAll;
            set
            {
                _RedirectAll = value;
                this.AutoRedirectHost = value;
            }
        }

        /// <summary>
        /// 开启自动跳转到手机端专用域名（默认值false）
        /// </summary>
        public bool AutoRedirectHost { get; set; } = false;
        /// <summary>
        /// 手机页面域名完整域，结尾不包含斜杠（eg：https://m.qq.com）
        /// </summary>
        public string PageHost { get; set; }
        /// <summary>
        /// PC页面域名完整域，结尾不包含斜杠（eg：https://www.qq.com）
        /// </summary>
        public string WebHost { get; set; }

        /// <summary>
        /// UniApp Page 自动跳转到 PC 页面 （在PC打开手机页面时触发）
        /// </summary>
        public bool AutoRedirectToAction { get; set; }
        /// <summary>
        /// PC页面 自动跳转到 UniApp Page （在手机打开PC端页面时触发）
        /// </summary>
        public bool AutoRedirectToPage { get; set; }

        internal List<MapInfo> MapsList = new List<MapInfo>();
        /// <summary>
        /// 添加路径映射
        /// </summary>
        /// <param name="pagePath">UniApp Page路径（eg:/pages/home）[不区分大小写]</param>
        /// <param name="actionPath">PC端网站Action完整路径（eg:/Home/Index）[不区分大小写]</param>
        /// <returns></returns>
        public MapInfo AddMap(string pagePath, string actionPath)
        {
            var map = new MapInfo();
            map.PagePath = pagePath.ToLower();
            map.ActionPath = actionPath.ToLower();
            MapsList.Add(map);
            return map;
        }
        internal List<string> IgnorePaths = new List<string>();

        /// <summary>
        /// 添加要忽略处理的路径（eg:/api）[不区分大小写]
        /// </summary>
        /// <param name="paths">忽略处理的路径（eg:/api）[不区分大小写]</param>
        /// <returns></returns>
        public UniAppPageOptions AddIgnorePath(params string[] paths)
        {
            IgnorePaths.AddRange(paths.Select(v => v.ToLower()));
            return this;
        }

        /// <summary>
        /// 页面映射信息
        /// </summary>
        public class MapInfo
        {
            internal MapInfo() { }
            /// <summary>
            /// UniApp Page路径 (eg:/pages/home)
            /// </summary>
            public string PagePath { get; set; }
            /// <summary>
            /// PC端网站Action完整路径（eg:/Home/Index）
            /// </summary>
            public string ActionPath { get; set; }

            internal Dictionary<string, string> QueryMaps = new Dictionary<string, string>();
            internal Dictionary<string, string> ActionMaps => QueryMaps
                .GroupBy(v => v.Value)
                .ToDictionary(
                    v => v.Key,
                    v => v.Select(d => d.Key).FirstOrDefault()
                );
            /// <summary>
            /// 添加页面参数映射
            /// </summary>
            /// <param name="queryName">参数名称（UniApp和PC参数名一致）[区分大小写]</param>
            /// <returns></returns>
            public MapInfo AddQueryMap(string queryName)
                => AddQueryMap(queryName, queryName);
            /// <summary>
            /// 添加页面参数映射
            /// </summary>
            /// <param name="pageQueryName">[区分大小写] UniApp Page 路径参数</param>
            /// <param name="actionQueryName">[区分大小写] PC端网站Action 路径参数，如果在路由路径而不是参数传递，填写 [0] 表示匹配路径后的第1个匹配路由参数</param>
            /// <returns></returns>
            public MapInfo AddQueryMap(string pageQueryName, string actionQueryName)
            {
                QueryMaps.AddOrUpdate(pageQueryName, actionQueryName);
                return this;
            }
        }

    }
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    public class UniAppPageMidleware : IMiddleware
    {
        private readonly UniAppPageOptions _options;
#if NETCOREAPP3_0
        private IWebHostEnvironment web;
        public UniAppPageMidleware(IWebHostEnvironment web, IOptions<UniAppPageOptions> options)
#elif NETCOREAPP2_1
        private IHostingEnvironment web;
        public UniAppPageMidleware(IHostingEnvironment web, IOptions<UniAppPageOptions> options)
#endif
        {
            this.web = web;
            _options = options?.Value ?? new UniAppPageOptions();
        }
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            string requestPath = context.Request.Path.Value.ToLower();
            // 忽略处理的路径直接移交给下一处理管道
            if (_options.IgnorePaths.Any(v => requestPath.StartsWith(v)))
                return next(context);

            var requestQuery = FormatHelpers.QueryStringToDictionary(context.Request.QueryString.Value ?? string.Empty);

            // 获取浏览器代理字符串以判断是否是手机访问
            string userAgent = context.Request.Headers[HeaderNames.UserAgent];
            bool isMobile = Controller.IsMobileAgent(userAgent);
            // 判断是否访问手机端页面 对于以 /pages 路径开头的都认为是手机页面请求 
            if (requestPath.StartsWith("/pages") ||
                requestPath == "/" && (isMobile || context.Request.Host.Value.StartsWith("m.")))
            {
                context.Response.Clear();
                if (_options.AutoRedirectHost)
                {
                    if (_options.PageHost.IsNotNullOrEmpty() &&
                        context.Request.Host.Value != _options.PageHost.ConvertToUri()?.Host)
                    {
                        context.Response.Redirect(_options.PageHost + requestPath + context.Request.QueryString);
                        return Task.CompletedTask;
                    }
                }

                // 电脑访问手机页面，自动跳转到PC页面
                if (isMobile == false && _options.AutoRedirectToAction)
                {
                    if (requestPath == "/")
                    {
                        context.Response.Redirect(_options.WebHost);
                        return Task.CompletedTask;
                    }
                    // 尝试匹配检查存在设置的映射
                    var match = _options.MapsList.Where(v => v.PagePath.IsNotNullOrEmpty()).FirstOrDefault(v => requestPath == v.PagePath);
                    if (match != null)
                    {
                        var queryMaps = match.QueryMaps;
                        var actionPath = queryMaps.Where(v => v.Value.StartsWith('[') && v.Value.EndsWith(']')).ToList();
                        var actionQuery = match.ActionMaps.Where(v => !v.Key.StartsWith('[') && !v.Key.EndsWith(']'))
                            .ToDictionary(v => v.Key, v => v.Value);
                        var actionArray = new string[actionPath.Count];
                        foreach (var path in actionPath)
                            try
                            {
                                actionArray.SetValue(requestQuery.SelectValueOrDefault(path.Key), path.Value.ConvertToInt());
                            }
                            catch { }

                        var actionUrl = match.ActionPath;
                        // 处理路径参数
                        actionArray = actionArray.Where(v => v.IsNotNullOrEmpty()).ToArray();
                        if (actionArray.Length > 0)
                            actionUrl = $"{actionUrl}/{actionArray.Join("/")}";
                        // 处理请求参数
                        var queryDic = new Dictionary<string, string>();
                        foreach (var query in actionQuery)
                            queryDic.Add(query.Key, requestQuery.SelectValueOrDefault(query.Value));
                        if (actionQuery.Count > 0)
                            actionUrl = $"{actionUrl}?{queryDic.ToQueryString()}";

                        context.Response.Redirect(_options.WebHost + actionUrl);
                        return Task.CompletedTask;
                    }
                }

                var filePath = Path.Combine(web.WebRootPath, "index.html");
                if (File.Exists(filePath))
                    return context.Response.SendFileAsync(filePath);
            }

            // 处理相关映射跳转
            // - 能到达这里都是请求PC端页面的，不是以 /pages 路径开头
            if (isMobile)
            {
                // 匹配PC页面映射
                var match = _options.MapsList.Where(v => v.ActionPath.IsNotNullOrEmpty()).FirstOrDefault(v => requestPath == v.ActionPath)
                    ?? _options.MapsList.Where(v => v.ActionPath.IsNotNullOrEmpty() && v.ActionMaps.Any(d => d.Key.StartsWith('[') && d.Key.EndsWith(']'))).FirstOrDefault(v => requestPath.StartsWith(v.ActionPath));
                if (match != null)
                {
                    // 根据映射参数构造 UniApp Page 对应是页面参数
                    var queryDic = new Dictionary<string, string>();
                    var actionPath = match.QueryMaps.Where(v => v.Value.StartsWith('[') && v.Value.EndsWith(']')).ToList();
                    var paths = requestPath.Substring(match.ActionPath.Length).Split('/').Skip(1).ToArray();
                    // 如果定义有路由参数，从路由取出映射值
                    foreach (var path in actionPath)
                        try
                        {
                            queryDic.AddOrUpdate(path.Key, paths[path.Value.ConvertToInt()]);
                        }
                        catch { }
                    // 根据参数映射关系转移参数值
                    foreach (var query in requestQuery)
                    {
                        var found = match.ActionMaps.FirstOrDefault(v => v.Key == query.Key);
                        if (found.IsDefault() == false)
                            queryDic.AddOrUpdate(found.Value, query.Value);
                    }
                    // 最后拼接新的跳转地址
                    string pageUrl = $"{match.PagePath}";
                    if (queryDic.Count > 0)
                        pageUrl = $"{pageUrl}?{queryDic.ToQueryString()}";
                    // 实施跳转
                    context.Response.Redirect(_options.PageHost + pageUrl);
                    return Task.CompletedTask;
                }
            }

            return next(context);
        }
    }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
}

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
        /// <summary>
        /// 在服务容器中注册UniApp页面拦截器中间件
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddUniAppPages(this IServiceCollection builder, Action<UniAppPageOptions> options)
        {
            builder.Configure(options);
            builder.AddUniAppPages();
            return builder;
        }
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