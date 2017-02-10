using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BundlerMinifier
{
    public static class BundleHandler
    {
        public static void AddBundle(string configFile, Bundle newBundle)
        {
            IEnumerable<Bundle> existing = GetBundles(configFile, false, true)
                .Where(x => !x.Equals(newBundle));

            List<Bundle> bundles = new List<Bundle>();

            bundles.AddRange(existing);
            bundles.Add(newBundle);
            //newBundle.FileName = configFile;

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            string content = JsonConvert.SerializeObject(bundles, settings);
            File.WriteAllText(configFile, content + Environment.NewLine);
        }

        public static void RemoveBundle(string configFile, Bundle bundleToRemove)
        {
            IEnumerable<Bundle> bundles = GetBundles(configFile, false, true);
            List<Bundle> newBundles = new List<Bundle>();

            if (bundles.Contains(bundleToRemove))
            {
                newBundles.AddRange(bundles.Where(b => !b.Equals(bundleToRemove)));
                string content = JsonConvert.SerializeObject(newBundles, Formatting.Indented);
                File.WriteAllText(configFile, content);
            }
        }
        /// <summary>
        /// Read config.josn
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="bundles"></param>
        /// <param name="throwEx">raise exception when error</param>
        /// <param name="forUpdate">for update config.json</param>
        /// <returns></returns>
        public static bool TryGetBundles(string configFile, out IEnumerable<Bundle> bundles, bool throwEx = false, bool forUpdate = false)
        {
            return BundleExt.TryGetBundles(configFile, out bundles, throwEx, forUpdate);
        }
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(configFile) || !File.Exists(configFile))
        //        {
        //            bundles = Enumerable.Empty<Bundle>();
        //            return false;
        //        }

        //        configFile = new FileInfo(configFile).FullName;
        //        string content = File.ReadAllText(configFile);
        //        bundles = JArray.Parse(content).ToObject<Bundle[]>();

        //        foreach (Bundle bundle in bundles)
        //        {
        //            bundle.FileName = configFile;
        //        }

        //        return true;
        //    }
        //    catch
        //    {
        //        bundles = null;
        //        return false;
        //    }
        //}
        /// <summary>
        /// Read and parse config.json
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="throwEx">Raise exception when error</param>
        /// <param name="forUpdate">For update config.json</param>
        /// <returns></returns>
        public static IEnumerable<Bundle> GetBundles(string configFile, bool throwEx = true, bool forUpdate = false)
        {
            IEnumerable<Bundle> bundles;
            BundleExt.TryGetBundles(configFile, out bundles, throwEx, forUpdate);
            return bundles;
        }
        /// <summary>
        /// Get all input text
        /// </summary>
        /// <param name="baseFolder">unused</param>
        /// <param name="bundle"></param>
        public static void ProcessBundle(string baseFolder, Bundle bundle)
        {
            StringBuilder sb = new StringBuilder();
            List<string> inputFiles = bundle.InputFiles;// GetAbsoluteInputFiles();
            foreach (string file in inputFiles)
            {
                //string  file = Path.Combine(baseFolder, input);

                if (File.Exists(file))
                {
                    string content;

                    if (bundle.AdjustRelativePaths)
                    {
                        content = CssRelativePath.Adjust(file, bundle.OutputFileName);//.GetAbsoluteOutputFile());
                    }
                    else
                    {
                        content = FileHelpers.ReadAllText(file);
                    }

                    sb.AppendLine(content);
                }
            }

            bundle.Output = sb.ToString().Trim();
        }

        //internal static bool AdjustRelativePaths(Bundle bundle)
        //{

        //    //if (!bundle.Minify.ContainsKey("adjustRelativePaths"))
        //    //    return true;

        //    //return bundle.Minify["adjustRelativePaths"].ToString() == "True";
        //}
    }
}
