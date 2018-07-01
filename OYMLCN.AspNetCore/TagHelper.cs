#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Linq;
using OYMLCN;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    [HtmlTargetElement("__taghelper__")]
    public class ViewContextTagHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// 传入属性值对比是否等于当前请求的Controller
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public bool IsEqualController(string controller) => controller.IsEqual(ViewContext.RouteData.Values["controller"]?.ToString());
        /// <summary>
        /// 传入属性值对比是否等于当前请求的Controller
        /// </summary>
        /// <param name="controllers"></param>
        /// <returns></returns>
        public bool IsEqualControllers(string controllers) => IsEqualControllers(controllers.SplitAuto());
        /// <summary>
        /// 传入属性值对比是否等于当前请求的Controller
        /// </summary>
        /// <param name="controllers"></param>
        /// <returns></returns>
        public bool IsEqualControllers(params string[] controllers) => controllers.Contains(ViewContext.RouteData.Values["controller"]?.ToString());
        /// <summary>
        /// 传入属性值对比是否等于当前请求的Action
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool IsEqualAction(string action) => action.IsEqual(ViewContext.RouteData.Values["action"]?.ToString());
        /// <summary>
        /// 传入属性值对比是否等于当前请求的Action与Controller
        /// </summary>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        public bool IsEqualAction(string action, string controller) => IsEqualController(controller) && IsEqualAction(action);
    }

    [HtmlTargetElement("input", Attributes = "asp-checked")]
    public class InputCheckedHelper : TagHelper
    {
        [HtmlAttributeName("asp-checked")]
        public bool? Attribute { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Attribute == true)
                output.Attributes.SetAttribute("checked", null);
            else
                output.RemoveAttribute("checked");

            base.Process(context, output);
        }
    }
    [HtmlTargetElement("option", Attributes = "asp-selected")]
    public class SelectOptionSelectedHelper : TagHelper
    {
        [HtmlAttributeName("asp-selected")]
        public bool? Attribute { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Attribute == true)
                output.Attributes.SetAttribute("selected", null);
            else
                output.RemoveAttribute("selected");
            base.Process(context, output);
        }
    }

}

namespace OYMLCN
{
    public static class TagHelperExtensions
    {
        /// <summary>
        /// 获取属性
        /// </summary>
        /// <param name="output"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static TagHelperAttribute GetAttribute(this TagHelperOutput output, string name) =>
            output.Attributes.Where(d => d.Name.IsEqualIgnoreCase(name)).FirstOrDefault();
        /// <summary>
        /// 移除并返回属性
        /// </summary>
        /// <param name="output"></param>
        /// <param name="name"></param>
        /// <returns>被移除的属性</returns>
        public static TagHelperAttribute RemoveAttribute(this TagHelperOutput output, string name)
        {
            var old = output.GetAttribute(name);
            if (old.IsNotNull())
                output.Attributes.Remove(old);
            return old;
        }

        /// <summary>
        /// 添加 Class 属性
        /// </summary>
        /// <param name="output"></param>
        /// <param name="className"></param>
        public static void AddClass(this TagHelperOutput output, string className)
        {
            var classNames = output.RemoveAttribute("class")?.Value.ToString().SplitBySign(" ").ToList() ?? new List<string>();
            classNames.Add(className);
            output.Attributes.Add("class", classNames.Distinct().Join(" "));
        }
    }
}