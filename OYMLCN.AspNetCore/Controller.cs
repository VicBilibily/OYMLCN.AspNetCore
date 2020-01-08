using Microsoft.AspNetCore.Http.Features;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using OYMLCN.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

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
        public long UserId => User.Claims.Where(d => d.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value.ConvertToNullableLong() ?? 0;

        /// <summary>
        /// 上一来源(Uri)
        /// </summary>
        public Uri RefererUri => Request.Headers["Referer"].FirstOrDefault()?.ConvertToUri();
        /// <summary>
        /// 上一来源域名
        /// </summary>
        public string RefererHost => RefererUri?.Host;
        /// <summary>
        /// 上一来源路径
        /// </summary>
        public string RefererPath => RefererUri?.AbsolutePath;

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
        public string RequestUserAgent => Request.Headers[HeaderNames.UserAgent];

        #region 判断是否使用手机浏览
        //regex from http://detectmobilebrowsers.com/
        private static readonly Regex b = new Regex(@"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private static readonly Regex v = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        /// <summary>
        /// 判断是否使用手机浏览
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public static bool IsMobileAgent(string userAgent)
        {
            try
            {
                return b.IsMatch(userAgent) || v.IsMatch(userAgent.Substring(0, 4));
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 判断是否使用手机浏览
        /// </summary>
        public bool IsMobileRequest => IsMobileAgent(this.RequestUserAgent);
        #endregion

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
    }
}