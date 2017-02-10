using NUglify;
using NUglify.JavaScript;

namespace BundlerMinifier
{
    static class JavaScriptOptions
    {
        public static CodeSettings GetSettings(Bundle bundle)
        {
            CodeSettings settings = new CodeSettings();
            settings.AlwaysEscapeNonAscii = GetValue(bundle, "alwaysEscapeNonAscii", false);// == "True";

            settings.PreserveImportantComments = GetValue(bundle, "preserveImportantComments", true);// == "True";
            settings.TermSemicolons = GetValue(bundle, "termSemicolons", true);// == "True";

            if (GetValue(bundle, "renameLocals", true)/* == "False"*/)
                settings.LocalRenaming = LocalRenaming.KeepAll;

            string evalTreatment = GetValue(bundle, "evalTreatment", "ignore");

            if (evalTreatment == "ignore")
                settings.EvalTreatment = EvalTreatment.Ignore;
            else if (evalTreatment == "makeAllSafe")
                settings.EvalTreatment = EvalTreatment.MakeAllSafe;
            else if (evalTreatment == "makeImmediateSafe")
                settings.EvalTreatment = EvalTreatment.MakeImmediateSafe;

            string outputMode = GetValue(bundle, "outputMode", "singleLine");

            if (outputMode == "multipleLines")
                settings.OutputMode = OutputMode.MultipleLines;
            else if (outputMode == "singleLine")
                settings.OutputMode = OutputMode.SingleLine;
            else if (outputMode == "none")
                settings.OutputMode = OutputMode.None;

            //string indentSize = GetValue(bundle, "indentSize", 2);
            //int size;
            //if (int.TryParse(indentSize, out size))
            settings.IndentSize = GetValue(bundle, "indentSize", 2);

            return settings;
        }
        internal static bool GetValue(Bundle bundle, string key, bool defaultValue)
        {
            return bundle.GetMinifyValue(key, true, defaultValue);
        }
        internal static string GetValue(Bundle bundle, string key, string defaultValue)
        {
            var obj = bundle.GetMinifyValue(key, true);
            if (obj == null) return defaultValue;
            return obj.ToString();

        }
        internal static int GetValue(Bundle bundle, string key, int defaultValue)
        {
            var obj = bundle.GetMinifyValue(key, true);
            if (obj == null) return defaultValue;
            int rsl;
            if (int.TryParse(obj.ToString(), out rsl)) return rsl;
            return defaultValue;
        }
        //internal static string GetValue(Bundle bundle, string key, object defaultValue = null)
        //{
        //    var value = bundle.GetMinifyValue(key);
        //    if (bundle.Minify.ContainsKey(key))
        //        return bundle.Minify[key].ToString();

        //    if (defaultValue != null)
        //        return defaultValue.ToString();

        //    return string.Empty;
        //}
    }
}
