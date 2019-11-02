using System;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OYMLCN.Extensions;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// layui-this
    /// </summary>
    [HtmlTargetElement("li", Attributes = "layui-this-controller")]
    [HtmlTargetElement("dd", Attributes = "layui-this-controller,layui-this-action")]
    [HtmlTargetElement("dd", Attributes = "layui-this-controller,layui-this-action")]
    public class LayuiThisTagHelper : ViewContextTagHelper
    {
        /// <summary>
        /// layui-this-controller
        /// </summary>
        [HtmlAttributeName("layui-this-controller")]
        public string Controller { get; set; }
        /// <summary>
        /// layui-this-action
        /// </summary>
        [HtmlAttributeName("layui-this-action")]
        public string Action { get; set; }

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (IsEqualController(Controller) && (Action.IsNullOrEmpty() || IsEqualAction(Action)))
                output.AddClass("layui-this");
            base.Process(context, output);
        }

    }

    /// <summary>
    /// layui-this 添加 href 地址
    /// </summary>
    [HtmlTargetElement("a", Attributes = "layui-this")]
    [HtmlTargetElement("a", Attributes = "layui-params")]
    [HtmlTargetElement("a", Attributes = "layui-all-route-data")]
    [HtmlTargetElement("a", Attributes = "asp-all-route-data,layui-all-route-data")]
    [HtmlTargetElement("a", Attributes = "asp-all-route-data,layui-route-*")]
    [HtmlTargetElement("a", Attributes = "asp-route-*,layui-all-route-data")]
    [HtmlTargetElement("a", Attributes = "asp-route-*,layui-route-*")]
    [HtmlTargetElement("a", Attributes = "layui-this-controller")]
    [HtmlTargetElement("a", Attributes = "layui-this-controller,layui-this-action")]
    public class AnchorLayuiThisTagHelper : LayuiThisTagHelper
    {
        /// <summary>
        /// layui-params，需要对比的参数，若不传则默认完全匹配。
        /// 多个参数用英文 , 号分隔
        /// </summary>
        [HtmlAttributeName("layui-params")]
        public string LayuiParams { get; set; }
        /// <summary>
        /// layui-this
        /// </summary>
        [HtmlAttributeName("layui-this")]
        public bool? LayuiThis { get; set; }

        /// <summary>
        /// asp-all-route-data
        /// </summary>
        [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
        public IDictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// layui-all-route-data
        /// </summary>
        [HtmlAttributeName("layui-all-route-data", DictionaryAttributePrefix = "layui-route-")]
        public IDictionary<string, string> LayuiRouteValues { get; set; } = new Dictionary<string, string>();

        private bool CheckRouteValues()
        {
            if (LayuiRouteValues == null || LayuiRouteValues.Count == 0) return false;
            foreach (var rule in LayuiRouteValues)
            {
                if (RouteValues.TryGetValue(rule.Key, out string source) && Equals(rule.Value, source))
                    continue;
                else
                    return false;
            }
            return true;
        }

        IHtmlGenerator Generator;
        /// <summary>
        /// AnchorLayuiThisTagHelper
        /// </summary>
        /// <param name="generator"></param>
        public AnchorLayuiThisTagHelper(IHtmlGenerator generator) => Generator = generator;

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            //var aHelper = new AnchorTagHelper(Generator)
            //{
            //    Action = Action,
            //    Controller = Controller,
            //    ViewContext = ViewContext
            //};
            //aHelper.Process(context, output);
            var addClass = Convert.ToBoolean(LayuiThis);

            if (Controller.IsNotNullOrEmpty())
                addClass = IsEqualController(Controller) && (Action.IsNullOrEmpty() || IsEqualAction(Action));

            // 如果未指示需要添加样式，尝试对比参数
            if (!addClass)
            {
                // 是否有指定对比参数，没有的话全部进行匹配
                if (LayuiParams.IsNullOrWhiteSpace())
                    addClass = CheckRouteValues();
                else
                {
                    // 如果指定了匹配参数，进行参数匹配
                    int matched = 0;
                    var paramArr = LayuiParams.SplitAuto();
                    foreach (var key in paramArr)
                    {
                        if (RouteValues.TryGetValue(key, out string route) &&
                            LayuiRouteValues.TryGetValue(key, out string source) &&
                            Equals(route, source))
                            matched++;
                    }
                    addClass = matched == paramArr.Length;
                }
            }
            if (addClass)
                output.AddClass("layui-this");
            base.Process(context, output);
        }
    }
}
