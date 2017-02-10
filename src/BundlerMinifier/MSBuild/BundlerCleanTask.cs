using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Collections.Generic;

namespace BundlerMinifier
{
    public class BundlerCleanTask : Task
    {
        public string FileName { get; set; }

        public override bool Execute()
        {
            FileInfo configFile = new FileInfo(FileName);

            Log.LogMessage(MessageImportance.High, Environment.NewLine + "Bundler: Cleaning output from " + configFile.Name);

            if (!configFile.Exists)
            {
                Log.LogWarning(configFile.FullName + " does not exist");
                return true;
            }

            IEnumerable<Bundle> bundles;
            try
            {
                bundles = BundleHandler.GetBundles(configFile.FullName);
            }
            catch (Exception ex)
            {
                Log.LogError($"There was an error reading {configFile.Name}", ex);
                return false;
            }
            if (bundles != null)
            {

                foreach (Bundle bundle in bundles)
                {
                    if (bundle.Config.DeleteOutputBaseOnClean)
                    {
                        Directory.Delete(bundle.Config.OutputBase, true);
                        break;
                    }
                    var outputFile = bundle.OutputFileName;//.GetAbsoluteOutputFile();
                    var inputFiles = bundle.InputFiles;//.GetAbsoluteInputFiles();

                    var minFile = BundleMinifier.GetMinFileName(outputFile);
                    var mapFile = minFile + ".map";
                    var gzipFile = minFile + ".gz";

                    if (!inputFiles.Contains(outputFile))
                        Deletefile(outputFile);

                    Deletefile(minFile);
                    Deletefile(mapFile);
                    Deletefile(gzipFile);
                }

                Log.LogMessage(MessageImportance.High, "Bundler: Done cleaning output file from " + configFile.Name);

                return true;
            }

            Log.LogWarning($"There was an error reading {configFile.Name}");
            return false;
        }

        private void Deletefile(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    FileHelpers.RemoveReadonlyFlagFromFile(file);
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
            }
        }
    }
}
