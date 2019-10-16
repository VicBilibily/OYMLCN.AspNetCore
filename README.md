# OYMLCN.AspNetCore

1.类Controller继承自源类型
加入参数 IsQcloudCDNRequest、RequestSourceIP、IsAuthenticated、UserName、UserId
加入方法 UserSignIn、UserSignOut、UserSignInWithJWT、UserSignOutWithJWT
加入辅助 RefererPath、RequestHost、RequestPath、RequestUserAgent、RequestQueryParams
2.配置扩展
services.AddSessionAndCookie、app.UseQcloudForwardedHeaders
app.UseSessionAndAuthentication、app.UseJsonResultForStatusCodeAndException
app.UseJsonResultForStatusCodeAndException
3.独立模块
ViewRender、EmailSender
4.TagHelper（附带部分适用于LayUI的辅助扩展）
辅助类 CDNImageHelper
TagHelperOutput扩展方法 GetAttribute、RemoveAttribute、AddClass
5、扩展模块
JsonWebToken 构造器JwtTokenBuilder 中间件JWTInCookieMiddleware(已弃用) 配置services.AddJsonWebTokenAuthentication、app.UseJWTAuthenticationWithCookie
EntityFramework扩展：RemoveOne、RemoveOneAndSave



### 通过配置使用JWT的说明

1、在 `Startup` 中的 `ConfigureServices` 添加

``` c#
services.AddJsonWebTokenAuthentication(Configuration);
```

2、在 `Startup` 中的 `Configure` 添加

```c#
app.UseAuthentication();
app.UseAuthorization(); //netcoreapp3.0新增
```

3、在 `appsettings.json` 中增加以下配置节

```json
{
  "Logging": {
...
  },
  "JWT": {
    "SecretKey": "加密密钥", 
    "Issuer": "信任签发者",
    "Audience": "信任服务者", 
    "Name": "access_token"
  }
}

```

4、创建一个 `BasicController` 基类并每个Controller都继承，或所有Controller继承自 `OYMLCN.AspNetCore.Controller`

```c#
public partial class BasicController : OYMLCN.AspNetCore.Controller
```

5、用户登录时创建Token并调用继承基类下的 `UserSignInWithJWT` 方法写入用户Cookies

``` c#
var tokenBuilder = new JwtTokenBuilder(Configuration, user.ID);
tokenBuilder.AddExpiry((keepLogin ? 7 : 1) * 60 * 24); // 有效期 一天
tokenBuilder.AddClaim(ClaimTypes.Name, user.FullName ?? user.Phone);
var token = tokenBuilder.Build();
var options = new CookieOptions();
if (keepLogin) options.MaxAge = TimeSpan.FromDays(7);
var access_token = token.access_token; // 返回到客户端的token
base.UserSignInWithJWT(token, false, options); // 调用此句写Cookies
return access_token;
```

6、用户退出时调用继承基类下的 `UserSignOutWithJWT` 即可

