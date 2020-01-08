#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using OYMLCN.AspNetCore;
using OYMLCN.Extensions;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OYMLCN.AspNetCore
{
    public static class JsonWebToken
    {
        internal const string SecretKeyPath = "JWT:SecretKey";
        internal const string IssuerPath = "JWT:Issuer";
        internal const string AudiencePath = "JWT:Audience";
        internal const string TokenNamePath = "JWT:Name";
        internal const string TokenNameDefault = "access_token";

        internal static string Encoder<T>(this T encryptor, string str) where T : HashAlgorithm
        {
            var sha1bytes = Encoding.UTF8.GetBytes(str);
            byte[] resultHash = encryptor.ComputeHash(sha1bytes);
            string sha1String = BitConverter.ToString(resultHash).ToLower();
            sha1String = sha1String.Replace("-", "");
            return sha1String;
        }

        public static SecurityKey CrateSecurityKey(string secret) =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(MD5.Create().Encoder(secret)));
        public sealed class JwtToken
        {
            internal JwtToken(JwtSecurityToken token, string key, int expires)
            {
                this.token = new JwtSecurityTokenHandler().WriteToken(token);
                this.key = key;
                this.expires = expires;
            }
            [JsonIgnore]
            internal string token { get; private set; }
            [JsonIgnore]
            internal int expires { get; private set; }

            public string key { get; private set; }
            public string access_token => token;
            public string refresh_token => Guid.NewGuid().ToString("N");
            public int expires_in => expires;
        }
        public sealed class JwtTokenBuilder
        {
            private string tokenKey = TokenNameDefault;
            private SecurityKey securityKey = null;
            private string subject;
            private string issuer;
            private string audience;
            private Dictionary<string, string> claims = new Dictionary<string, string>();
            private int expiryInMinutes = 0;

            public IConfiguration Configuration { get; set; }

            /// <summary>
            /// JsonWebToken 构造
            /// </summary>
            /// <param name="configuration">应用程序设置，需要在配置文件设置JWT->SecretKey/Issuer/Audience/Name</param>
            /// <param name="subject">用户标识</param>
            public JwtTokenBuilder(IConfiguration configuration, object subject) :
                this(CrateSecurityKey(configuration.GetValue<string>(SecretKeyPath)), subject.ToString(), configuration.GetValue<string>(IssuerPath), configuration.GetValue<string>(AudiencePath))
            {
                this.tokenKey = configuration.GetValue<string>(TokenNamePath) ?? TokenNameDefault;
            }
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
                this.claims.AddOrUpdate(type, value);
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
                return new JwtToken(token, tokenKey, expiryInMinutes == 0 ? -1 : expiryInMinutes * 60);
            }
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
        /// 配置JsonWebToken(JWT)身份验证
        /// 需要在配置文件设置JWT->SecretKey/Issuer/Audience/Name
        /// 需在Configure中加入 app.UseAuthentication() 以使得登陆配置生效 
        /// </summary>
        public static IServiceCollection AddJsonWebTokenAuthentication(this IServiceCollection services)
        {
            var configuration = services.GetRequiredService<IConfiguration>();
            return AddJsonWebTokenAuthentication(services,
                configuration.GetValue<string>("JWT:SecretKey"),
                configuration.GetValue<string>("JWT:Issuer"),
                configuration.GetValue<string>("JWT:Audience"),
                configuration.GetValue<string>("JWT:Name") ?? JsonWebToken.TokenNameDefault
            );
        }
        /// <summary>
        /// 配置JsonWebToken(JWT)身份验证
        /// 需在Configure中加入 app.UseAuthentication() 以使得登陆配置生效 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="secret">密钥</param>
        /// <param name="issuer">信任签发者</param>
        /// <param name="audience">信任服务者</param>
        /// <param name="name">Token名称</param>
        /// <param name="clockSkew">宽限时间/时间验证偏差（默认偏差5分钟）</param>
        /// <returns></returns>
        public static IServiceCollection AddJsonWebTokenAuthentication(this IServiceCollection services, string secret, string issuer, string audience, string name = JsonWebToken.TokenNameDefault, TimeSpan clockSkew = default(TimeSpan))
            => AddJsonWebTokenAuthentication(services, JsonWebToken.CrateSecurityKey(secret), issuer, audience, name, clockSkew);
        /// <summary>
        /// 配置JsonWebToken(JWT)身份验证
        /// 需在Configure中加入 app.UseAuthentication() 以使得登陆配置生效 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="securityKey">密钥</param>
        /// <param name="issuer">信任签发者</param>
        /// <param name="audience">信任服务者</param>
        /// <param name="name">Token名称</param>
        /// <param name="clockSkew">宽限时间/时间验证偏差（默认偏差5分钟）</param>
        /// <returns></returns>
        public static IServiceCollection AddJsonWebTokenAuthentication(this IServiceCollection services, SecurityKey securityKey, string issuer, string audience, string name = JsonWebToken.TokenNameDefault, TimeSpan clockSkew = default(TimeSpan))
        {
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Events = new JwtBearerEvents()
                    {
                        // 注册自定义token获取方式
                        OnMessageReceived = context =>
                        {
                            // 首先尝试从Cookie中获取Token
                            string token = context.Request.Cookies[name];
                            // 如果无，则尝试参数从中获取Token
                            if (token.IsNullOrEmpty()) token = context.Request.Query[name];
                            // 执行完毕，把取得的值设置为token
                            // 如果为空原始方式会从Header重新获取
                            context.Token = token;
                            return Task.CompletedTask;
                        }
                    };
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
                    if (clockSkew != default)
                        options.TokenValidationParameters.ClockSkew = clockSkew;
                });
            return services;
        }

    }


}
