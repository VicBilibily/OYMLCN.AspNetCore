#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using OYMLCN.Extensions;

namespace OYMLCN.AspNetCore
{
    public static class JsonWebToken
    {
        public static SecurityKey CrateSecurityKey(string secret) =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret.AsEncrypt().MD5));
        public sealed class JwtToken
        {
            internal JwtToken(JwtSecurityToken token, int expires)
            {
                this.token = new JwtSecurityTokenHandler().WriteToken(token);
                this.expires = expires;
            }

            [JsonIgnore]
            internal string token { get; private set; }
            [JsonIgnore]
            internal int expires { get; private set; }

            public string access_token => token;
            public string refresh_token => Guid.NewGuid().ToString("N");
            public int expires_in => expires;
        }
        public sealed class JwtTokenBuilder
        {
            private SecurityKey securityKey = null;
            private string subject = "";
            private string issuer = "";
            private string audience = "";
            private Dictionary<string, string> claims = new Dictionary<string, string>();
            private int expiryInMinutes = 0;

            /// <summary>
            /// JsonWebToken 构造
            /// </summary>
            /// <param name="secret">密钥</param>
            /// <param name="subject">用户标识</param>
            /// <param name="issuer">信任签发者</param>
            /// <param name="audience">信任服务者</param>
            public JwtTokenBuilder(string secret, string subject, string issuer, string audience) :
                this(CrateSecurityKey(secret), subject, issuer, audience)
            { }
            /// <summary>
            /// JsonWebToken 构造
            /// </summary>
            /// <param name="securityKey">密钥</param>
            /// <param name="subject">用户标识</param>
            /// <param name="issuer">信任签发者</param>
            /// <param name="audience">信任服务者</param>
            public JwtTokenBuilder(SecurityKey securityKey, string subject, string issuer, string audience)
            {
                this.securityKey = securityKey;
                this.subject = subject;
                this.issuer = issuer;
                this.audience = audience;
            }

            public JwtTokenBuilder AddClaim(string type, string value)
            {
                this.claims.Add(type, value);
                return this;
            }
            public JwtTokenBuilder AddClaims(Dictionary<string, string> claims)
            {
                this.claims.Union(claims);
                return this;
            }

            /// <summary>
            /// Token有效期
            /// </summary>
            /// <param name="expiryInMinutes">单位：分钟</param>
            /// <returns></returns>
            public JwtTokenBuilder AddExpiry(int expiryInMinutes)
            {
                this.expiryInMinutes = expiryInMinutes;
                return this;
            }

            public JwtToken Build()
            {
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, this.subject),
                    //new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }
                .Union(this.claims.Select(item => new Claim(item.Key, item.Value)));
                DateTime? expires = null;
                if (expiryInMinutes > 0)
                    expires = DateTime.UtcNow.AddMinutes(expiryInMinutes);

                var token = new JwtSecurityToken(
                    issuer: this.issuer,
                    audience: this.audience,
                    claims: claims,
                    expires: expires,
                    signingCredentials: new SigningCredentials(
                        this.securityKey,
                        SecurityAlgorithms.HmacSha256));

                return new JwtToken(token, expiryInMinutes == 0 ? -1 : expiryInMinutes * 60);
            }
        }

    }
    internal class JWTMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string tokenName;

        public JWTMiddleware(RequestDelegate next, string tokenName = "access_token")
        {
            _next = next;
            this.tokenName = tokenName;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                var cookie = context.Request.Cookies[tokenName];
                if (cookie != null)
                    context.Request.Headers.Append("Authorization", "Bearer " + cookie);
                else
                {
                    string token = context.Request.Query[tokenName];
                    if (token.IsNullOrWhiteSpace())
                        try
                        {
                            token = context.Request.Form[tokenName];
                        }
                        catch { }
                    if (token != null)
                        context.Request.Headers.Append("Authorization", "Bearer " + token);
                }
            }
            await _next.Invoke(context);
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
        /// 一句话配置JsonWebToken(JWT)身份验证
        /// 需在Configure中加入 app.UseAuthentication() 以使得登陆配置生效 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="secret">密钥</param>
        /// <param name="issuer">信任签发者</param>
        /// <param name="audience">信任服务者</param>
        /// <param name="clockSkew">宽限时间/时间验证偏差（默认偏差5分钟）</param>
        /// <returns></returns>
        public static IServiceCollection AddJsonWebTokenAuthentication(this IServiceCollection services, string secret, string issuer, string audience, TimeSpan clockSkew = default(TimeSpan)) =>
            AddJsonWebTokenAuthentication(services, OYMLCN.AspNetCore.JsonWebToken.CrateSecurityKey(secret), issuer, audience, clockSkew);
        /// <summary>
        /// 一句话配置JsonWebToken(JWT)身份验证
        /// 需在Configure中加入 app.UseAuthentication() 以使得登陆配置生效 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="securityKey">密钥</param>
        /// <param name="issuer">信任签发者</param>
        /// <param name="audience">信任服务者</param>
        /// <param name="clockSkew">宽限时间/时间验证偏差（默认偏差5分钟）</param>
        /// <returns></returns>
        public static IServiceCollection AddJsonWebTokenAuthentication(this IServiceCollection services, SecurityKey securityKey, string issuer, string audience, TimeSpan clockSkew = default(TimeSpan))
        {
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = securityKey,

                    };
                    if (clockSkew != default(TimeSpan))
                        options.TokenValidationParameters.ClockSkew = clockSkew;
                });
            return services;
        }

    }


}
