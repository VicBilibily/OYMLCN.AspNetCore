using Microsoft.AspNetCore.Http.Features;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using OYMLCN.Extensions;

namespace OYMLCN.AspNetCore
{
    /// <summary>
    /// Controller
    /// </summary>
    public abstract class Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        /// <summary>
        /// 是否来自腾讯云CDN加速服务
        /// </summary>
        public bool IsQcloudCDNRequest => HttpContext.Request.Headers["X-Tencent-Ua"].Contains("Qcloud");
        /// <summary>
        /// 用户真实IP地址
        /// </summary>
        public IPAddress RequestSourceIP =>
            IsQcloudCDNRequest ?
                IPAddress.Parse(HttpContext.Request.Headers["X-Forwarded-For"]) :
                (Request.HttpContext.Features?.Get<IHttpConnectionFeature>()?.RemoteIpAddress ?? HttpContext.Connection.RemoteIpAddress);

        /// <summary>
        /// 登陆标识
        /// </summary>
        public bool IsAuthenticated => User.Identity.IsAuthenticated;
        /// <summary>
        /// 登陆用户名
        /// </summary>
        public string UserName => User.Identity.Name;
        /// <summary>
        /// 登陆用户唯一标识
        /// </summary>
        public long UserId => User.Claims.Where(d => d.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value.AsType().NullableLong ?? 0;

        /// <summary>
        /// 用户登陆
        /// 需要在 Startup 中配置Session及Cookie基本信息
        /// services.AddSessionAndCookie();
        /// app.UseAuthentication();
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="userId">用户ID</param>
        /// <param name="roles">用户角色</param>
        /// <param name="claims">其他标识</param>
        public void UserSignIn(string userName, long userId, string[] roles = null, params Claim[] claims)
        {
            var newClaims = claims.ToList();
            newClaims.Add(new Claim(ClaimTypes.Name, userName));
            foreach (var role in roles)
                newClaims.Add(new Claim(ClaimTypes.Role, role));
            newClaims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));

            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(
                    new ClaimsPrincipal(
                        new ClaimsIdentity(newClaims, CookieAuthenticationDefaults.AuthenticationScheme)
                        )
                    )
                ).Wait();
        }
        /// <summary>
        /// 注销登陆
        /// </summary>
        public void UserSignOut() => HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
        /// <summary>
        /// 用户登陆（将已经生成的JWT身份认证信息写入到Cookie）
        /// 若要使用Cookie+JWT验证，需要在Startup中配置app.UseJWTAuthenticationWithCookie
        /// </summary>
        /// <param name="jwt"></param>
        /// <param name="cookieName"></param>
        /// <param name="secureCookie"></param>
        public void UserSignInWithJWT(JsonWebToken.JwtToken jwt, string cookieName = "access_token", bool secureCookie = true) =>
            Response.Cookies.Append("access_token", jwt.access_token, new CookieOptions()
            {
                Secure = secureCookie,
                HttpOnly = true
            });
        /// <summary>
        /// 注销登陆（删除Cookie中的JWT身份凭证）
        /// </summary>
        /// <param name="cookieName"></param>
        public void UserSignOutWithJWT(string cookieName = "access_token") => Response.Cookies.Delete(cookieName);


        /// <summary>
        /// 上一路径
        /// </summary>
        public string RefererPath => (Request.Headers as RequestHeaders).Referer?.AbsolutePath;
        /// <summary>
        /// 请求域名
        /// </summary>
        public string RequestHost => Request.Host.Value;
        /// <summary>
        /// 请求路径
        /// </summary>
        public string RequestPath => Request.Path.Value;
        /// <summary>
        /// 请求标识
        /// </summary>
        public string RequestUserAgent => Request.Headers["User-Agent"];


        private Dictionary<string, string> requestQueryParams;
        /// <summary>
        /// 请求参数集合
        /// </summary>
        public Dictionary<string, string> RequestQueryParams
        {
            get
            {
                if (requestQueryParams.IsNotNull())
                    return requestQueryParams;

                requestQueryParams = new Dictionary<string, string>();
                foreach (var item in Request.Query)
                    requestQueryParams[item.Key] = item.Value;

                if (Request.Method != "POST")
                    return requestQueryParams;
                else
                {
                    if (Request.Form.IsNotEmpty())
                        foreach (var item in Request.Form)
                            requestQueryParams[item.Key] = item.Value;
                    else if (Request.ContentType.Equals("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
                        foreach (var item in Request.Body.ReadToEnd().SplitBySign("&"))
                        {
                            var query = item.SplitBySign("=");
                            requestQueryParams.Add(query.FirstOrDefault(), query.Skip(1).FirstOrDefault());
                        }
                    return requestQueryParams;
                }
            }
        }

        UrlHelper urlHelper;
        /// <summary>
        /// UrlHelper
        /// </summary>
        public UrlHelper UrlHelper => urlHelper ?? (urlHelper = new UrlHelper(ControllerContext));
    }
}