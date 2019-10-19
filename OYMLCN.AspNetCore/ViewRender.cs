using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.IO;

namespace OYMLCN.AspNetCore
{
    /// <summary>
    /// 视图渲染
    /// </summary>
    public class ViewRenderService
    {
        IRazorViewEngine _viewEngine;
        ITempDataProvider _tempDataProvider;
        IHttpContextAccessor _httpContextAccessor;
        /// <summary>
        /// 视图渲染（请使用AddScoped注入方式调用，MVC框架会自动注入初始化参数）
        /// </summary>
        /// <param name="viewEngine"></param>
        /// <param name="tempDataProvider"></param>
        /// <param name="httpContextAccessor"></param>
        public ViewRenderService(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IHttpContextAccessor httpContextAccessor)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 渲染视图
        /// </summary>
        /// <param name="viewPath"></param>
        /// <returns></returns>
        public string Render(string viewPath) => Render(viewPath, string.Empty);
        /// <summary>
        /// 渲染视图
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="viewPath"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public string Render<TModel>(string viewPath, TModel model)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var viewEngineResult = _viewEngine.GetView("~/", viewPath, false);
            var actionContext = new ActionContext(httpContext, httpContext.GetRouteData(), new ActionDescriptor());
            if (!viewEngineResult.Success)
                viewEngineResult = _viewEngine.FindView(actionContext, viewPath, false);
            if (!viewEngineResult.Success)
                throw new InvalidOperationException($"Couldn't find view {viewPath}");

            var view = viewEngineResult.View;
            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    view,
                    new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model },
                    new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                    output,
                    new HtmlHelperOptions()
                );
                view.RenderAsync(viewContext).GetAwaiter().GetResult();
                return output.ToString();
            }
        }
    }
}
