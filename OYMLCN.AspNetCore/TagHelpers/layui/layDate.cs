#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    [HtmlTargetElement("input", Attributes = "layui-laydate-value")]
    public class LayDateTagHelper : TagHelper
    {
        /// <summary>
        /// layui-nav-itemed-controller
        /// 多个用任意分隔符分割
        /// </summary>
        [HtmlAttributeName("layui-laydate-value")]
        public DateTime? Value { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.SetAttribute("value", Value?.ToString("yyyy-MM-dd hh:mm:ss"));
            base.Process(context, output);
        }
    }

}
