using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OYMLCN.AspNetCore;
using Controller = Microsoft.AspNetCore.Mvc.Controller;

namespace OYMLCN.Extensions
{
    public static class ControllerExtension
    {
        /// <summary>
        /// 用户登陆
        /// 需要在 Startup 中配置Session及Cookie基本信息
        /// services.AddSessionAndCookie();
        /// app.UseAuthentication();
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="userName">用户名</param>
        /// <param name="userId">用户ID</param>
        /// <param name="roles">用户角色</param>
        /// <param name="claims">其他标识</param>
        public static void UserSignIn(this Controller controller, string userName, long userId, string[] roles = null, params Claim[] claims)
        {
            var newClaims = claims.ToList();
            newClaims.Add(new Claim(ClaimTypes.Name, userName));
            foreach (var role in roles)
                newClaims.Add(new Claim(ClaimTypes.Role, role));
            newClaims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));

            controller.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
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
        public static void UserSignOut(this Controller controller)
            => controller.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
        /// <summary>
        /// 用户登陆（将已经生成的JWT身份认证信息写入到Cookie）
        /// 若要使用Cookie+JWT验证，需要在Startup中配置app.UseJWTAuthenticationWithCookie
        /// </summary>
        public static void UserSignInWithJWT(this Controller controller, JsonWebToken.JwtToken jwt, bool secureCookie = true, CookieOptions options = null)
        {
            options = options ?? options ?? new CookieOptions();
            options.Secure = secureCookie;
            options.HttpOnly = true;
            controller.HttpContext.Response.Cookies.Append(jwt.key, jwt.access_token, options);
        }

        /// <summary>
        /// 注销登陆（删除Cookie中的JWT身份凭证）
        /// tokenKey 为 null 时将会尝试从配置文件 JWT:Name 中获取
        /// </summary>
        public static void UserSignOutWithJWT(this Controller controller, string tokenKey = null, bool secureCookie = true, CookieOptions options = null)
        {
            options = options ?? new CookieOptions();
            options.Secure = secureCookie;
            options.HttpOnly = true;

            if (tokenKey.IsNullOrEmpty())
            {
                tokenKey = controller.GetRequiredService<IConfiguration>().GetValue<string>(JsonWebToken.TokenNamePath);
                if (tokenKey.IsNullOrEmpty())
                    tokenKey = JsonWebToken.TokenNameDefault;
            }
            controller.HttpContext.Response.Cookies.Delete(tokenKey, options);
        }

        /// <summary>
        /// 获取已注入的服务实例
        /// </summary>
        public static T GetRequiredService<T>(this Controller controller)
            => controller.HttpContext.RequestServices.GetRequiredService<T>();

    }
}
