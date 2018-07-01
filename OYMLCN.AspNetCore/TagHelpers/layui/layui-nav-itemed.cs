using Microsoft.AspNetCore.Razor.TagHelpers;
using OYMLCN;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// layui-nav-itemed
    /// </summary>
    [HtmlTargetElement("li", Attributes = "layui-nav-itemed-controller")]
    public class LayuiNavItemedTagHelper : ViewContextTagHelper
    {
        /// <summary>
        /// layui-nav-itemed-controller
        /// 多个用任意分隔符分割
        /// </summary>
        [HtmlAttributeName("layui-nav-itemed-controller")]
        public string Controller { get; set; }
        /// <summary>
        /// layui-nav-itemed-action
        /// </summary>
        [HtmlAttributeName("layui-nav-itemed-action")]
        public string Action { get; set; }

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (IsEqualControllers(Controller) && (Action.IsNullOrEmpty() || IsEqualAction(Action)))
                output.AddClass("layui-nav-itemed");
            base.Process(context, output);
        }
    }
}
