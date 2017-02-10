using Newtonsoft.Json.Linq;
using NUglify.Html;

namespace BundlerMinifier
{
    static class HtmlOptions
    {
        public static HtmlSettings GetSettings(Bundle bundle)
        {
            var settings = new HtmlSettings
            {
                RemoveOptionalTags = JavaScriptOptions.GetValue(bundle, "removeOptionalEndTags", false),// == "True",
                ShortBooleanAttribute = JavaScriptOptions.GetValue(bundle, "collapseBooleanAttributes", true),// == "True",
                MinifyCss = JavaScriptOptions.GetValue(bundle, "minifyEmbeddedCssCode", true),// == "True",
                MinifyJs = JavaScriptOptions.GetValue(bundle, "minifyEmbeddedJsCode", true),// == "True",
                MinifyCssAttributes = JavaScriptOptions.GetValue(bundle, "minifyInlineCssCode", false),// == "True",
                AttributesCaseSensitive = JavaScriptOptions.GetValue(bundle, "preserveCase", false),// == "True",
                RemoveComments = JavaScriptOptions.GetValue(bundle, "removeHtmlComments", true),// == "True",
                RemoveQuotedAttributes = JavaScriptOptions.GetValue(bundle, "removeQuotedAttributes", true),// == "True",
                CollapseWhitespaces = JavaScriptOptions.GetValue(bundle, "collapseWhitespace", true),// == "True",
                IsFragmentOnly = JavaScriptOptions.GetValue(bundle, "isFragmentOnly", true)// == "True"
            };

            return settings;
        }

        //internal static string GetValue(Bundle bundle, string key, object defaultValue = null)
        //{
        //    if (bundle.Minify.ContainsKey(key))
        //    {
        //        object value = bundle.Minify[key];
        //        if (value is JArray)
        //        {
        //            return string.Join(",", ((JArray)value).Values<string>());
        //        }
        //        else
        //        {
        //            return value.ToString();
        //        }
        //    }

        //    if (defaultValue != null)
        //        return defaultValue.ToString();

        //    return string.Empty;
        //}
    }
}
