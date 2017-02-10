using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace BundlerMinifier
{
    public class BundleFileProcessor
    {
        // private static string[] _supported = new[] { ".JS", ".CSS", ".HTML", ".HTM" };
        private static string[] _unsuported = { "bundleconfig.json", "bundleconfig.json.bindings", ".ds_store", "thumbs.db" };
        public static bool IsIgnoreFile(string file)
        {
            return _unsuported.Contains(Path.GetFileName(file).ToLowerInvariant());
        }
        public static bool IsSupported(params string[] files)
        {
            files = files.Where(f => !string.IsNullOrEmpty(f)).ToArray();

            if (!files.Any()) return false;

            string ext = Path.GetExtension(files.FirstOrDefault()).ToUpperInvariant();

            foreach (string file in files)
            {
                string fileExt = Path.GetExtension(file).ToUpperInvariant();

                if (/*!_supported.Contains(fileExt) || */!fileExt.Equals(ext, StringComparison.OrdinalIgnoreCase) || IsIgnoreFile(file))
                    return false;
            }

            return true;
        }

        public bool Process(string fileName, IEnumerable<Bundle> bundles = null)
        {
            FileInfo info = new FileInfo(fileName);
            bundles = bundles ?? BundleHandler.GetBundles(fileName);
            bool result = false;

            foreach (Bundle bundle in bundles)
            {
                result |= ProcessBundle(info.Directory.FullName, bundle);
            }

            return result;
        }

        public void Clean(string fileName, IEnumerable<Bundle> bundles = null)
        {
            FileInfo info = new FileInfo(fileName);
            bundles = bundles ?? BundleHandler.GetBundles(fileName);
            if (!bundles.Any()) return;
            var b = bundles.FirstOrDefault();
            if (b.Config.DeleteOutputBaseOnClean)
            {
                //delete output folder
                Directory.Delete(b.Config.OutputBase, true);
                return;
            }
            foreach (var bundle in bundles)
            {
                CleanBundle(info.Directory.FullName, bundle);
            }
        }

        public void SourceFileChanged(string bundleFile, string sourceFile)
        {
            var bundles = BundleHandler.GetBundles(bundleFile);
            string bundleFileFolder = Path.GetDirectoryName(bundleFile),
                   sourceFileFolder = Path.GetDirectoryName(sourceFile);

            foreach (Bundle bundle in bundles)
            {
                foreach (string input in bundle.InputFiles)
                {
                    if (input.Equals(sourceFile, StringComparison.OrdinalIgnoreCase) || input.Equals(sourceFileFolder, StringComparison.OrdinalIgnoreCase))
                        ProcessBundle(bundleFileFolder, bundle);
                }
            }
        }

        public static IEnumerable<Bundle> IsFileConfigured(string configFile, string sourceFile)
        {
            List<Bundle> list = new List<Bundle>();

            try
            {
                var configs = BundleHandler.GetBundles(configFile);
                string folder = Path.GetDirectoryName(configFile);

                foreach (Bundle bundle in configs)
                {
                    foreach (string input in bundle.InputFiles)
                    {
                        if (input.Equals(sourceFile, StringComparison.OrdinalIgnoreCase) && !list.Contains(bundle))
                            list.Add(bundle);
                    }
                }

                return list;
            }
            catch (Exception)
            {
                return list;
            }
        }
        private static bool MustProcessBundle(Bundle bundle)
        {
            if (bundle.InputFiles.Count > 1 || bundle.IsMinificationEnabled) return true;
            return bundle.AdjustRelativePaths
                    && bundle.OutputFileName != bundle.InputFiles.FirstOrDefault();
        }
        private bool ProcessBundle(string baseFolder, Bundle bundle)
        {
            OnProcessing(bundle, baseFolder);
            if (bundle.Config == null)
            {
                var ex = new Exception("No config bundle");
                BundleConfig.Log(baseFolder, "No config bundle");
                throw ex;
                //false
                //return false;
            }
            var inputs = bundle.InputFiles;//.GetAbsoluteInputFiles();
            bool changed = false;
            bundle.Output = null;
            if (MustProcessBundle(bundle))// /*bundle.GetAbsoluteInputFiles(true)*/inputs.Count > 1 || bundle.InputFiles.FirstOrDefault() != bundle.OutputFileName)
            {
                BundleHandler.ProcessBundle(baseFolder, bundle);
                if ((bundle.OutputFileMin == null && bundle.OutputFileName != bundle.InputFiles.FirstOrDefault())
                    || (bundle.OutputFileMin != null && bundle.OutputFileMin != bundle.OutputFileName && bundle.OutputFileName != bundle.InputFiles.FirstOrDefault()))
                {
                    //write ouput
                    if (changed = FileHelpers.HasFileContentChanged(bundle.OutputFileName, bundle.Output))
                    {
                        OnBeforeBundling(bundle, baseFolder, changed);
                        if (bundle.InputFiles.Contains(bundle.OutputFileName))
                        {
                            BundleConfig.Log(bundle.Config.OutputBase, "Input files contains output file:" + bundle.OutputFileName);
                            throw new Exception("Input files contains output file");
                        }
                        if (!Directory.Exists(Path.GetDirectoryName(bundle.OutputFileName)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(bundle.OutputFileName));
                        }else if (File.Exists(bundle.OutputFileName))
                        {
                            File.Delete(bundle.OutputFileName);
                        }
                        File.WriteAllText(bundle.OutputFileName, bundle.Output, new UTF8Encoding(false));
                        OnAfterBundling(bundle, baseFolder, changed);
                    }
                }
                if (bundle.IsMinificationEnabled && bundle.OutputFileMin != bundle.InputFiles.FirstOrDefault())
                {
                    var result = BundleMinifier.MinifyBundle(bundle);
                    changed |= result.Changed;
                    if (bundle.SourceMap && !string.IsNullOrEmpty(result.SourceMap))
                    {
                        string mapFile = bundle.OutputFileMin + ".map";
                        bool smChanges = FileHelpers.HasFileContentChanged(mapFile, result.SourceMap);

                        if (smChanges)
                        {
                            OnBeforeWritingSourceMap(bundle.OutputFileMin, mapFile, smChanges);
                            File.WriteAllText(mapFile, result.SourceMap, new UTF8Encoding(false));
                            OnAfterWritingSourceMap(bundle.OutputFileMin, mapFile, smChanges);
                            changed = true;
                        }
                    }
                }
                bundle.Output = null;
            }
            else if (bundle.OutputFileName != bundle.InputFiles.FirstOrDefault() && BundleExt.IsChanged(bundle.InputFiles.FirstOrDefault(), bundle.OutputFileName))
            {
                changed = true;
                //copy
                OnBeforeBundling(bundle, baseFolder, changed);
                if (!Directory.Exists(Path.GetDirectoryName(bundle.OutputFileName)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(bundle.OutputFileName));
                }
                File.Copy(bundle.InputFiles.FirstOrDefault(), bundle.OutputFileName, true);
                OnAfterBundling(bundle, baseFolder, changed);
            }
            //else
            //{
            //   changed = File.GetLastWriteTimeUtc(bundle.OutputFileName) < File.GetLastWriteTimeUtc(bundle.)
            //}
            if (bundle.IsGzipEnabled)
            {
                BundleMinifier.GzipFile(bundle, changed);
            }

            //string minFile = BundleMinifier.GetMinFileName(bundle.GetAbsoluteOutputFile());

            //if (bundle.IsMinificationEnabled || bundle.IsGzipEnabled)
            //{
            //    var result = BundleMinifier.MinifyBundle(bundle);

            //    changed |= result.Changed;

            //    if (bundle.IsMinificationEnabled && bundle.SourceMap && !string.IsNullOrEmpty(result.SourceMap))
            //    {
            //        string mapFile = minFile + ".map";
            //        bool smChanges = FileHelpers.HasFileContentChanged(mapFile, result.SourceMap);

            //        if (smChanges)
            //        {
            //            OnBeforeWritingSourceMap(minFile, mapFile, smChanges);
            //            File.WriteAllText(mapFile, result.SourceMap, new UTF8Encoding(false));
            //            OnAfterWritingSourceMap(minFile, mapFile, smChanges);
            //            changed = true;
            //        }
            //    }
            //}

            return changed;
        }

        private void CleanBundle(string baseFolder, Bundle bundle)
        {
            string outputFile = bundle.OutputFileName;
            baseFolder = baseFolder.DemandTrailingPathSeparatorChar();
            if (!bundle.InputFiles.Contains(outputFile, StringComparer.OrdinalIgnoreCase))
            {
                if (File.Exists(outputFile))
                {
                    FileHelpers.RemoveReadonlyFlagFromFile(outputFile);
                    File.Delete(outputFile);
                    Console.WriteLine($"Deleted {FileHelpers.MakeRelative(baseFolder, outputFile).Cyan().Bright()}");
                }
            }
            string mapFile = outputFile + ".map";
            if (File.Exists(mapFile))
            {
                FileHelpers.RemoveReadonlyFlagFromFile(mapFile);
                File.Delete(mapFile);
                Console.WriteLine($"Deleted {mapFile.Cyan().Bright()}");
            }
            string gzFile = outputFile + ".gz";
            if (File.Exists(gzFile))
            {
                FileHelpers.RemoveReadonlyFlagFromFile(gzFile);
                File.Delete(gzFile);
                Console.WriteLine($"Deleted {gzFile.Cyan().Bright()}");
            }
            string minFile = BundleMinifier.GetMinFileName(bundle.OutputFileName);
            if (minFile == outputFile) return;

            if (File.Exists(minFile))
            {
                FileHelpers.RemoveReadonlyFlagFromFile(minFile);
                File.Delete(minFile);
                Console.WriteLine($"Deleted {FileHelpers.MakeRelative(baseFolder, minFile).Cyan().Bright()}");
            }

            mapFile = minFile + ".map";
            if (File.Exists(mapFile))
            {
                FileHelpers.RemoveReadonlyFlagFromFile(mapFile);
                File.Delete(mapFile);
                Console.WriteLine($"Deleted {mapFile.Cyan().Bright()}");
            }

            gzFile = minFile + ".gz";
            if (File.Exists(gzFile))
            {
                FileHelpers.RemoveReadonlyFlagFromFile(gzFile);
                File.Delete(gzFile);
                Console.WriteLine($"Deleted {gzFile.Cyan().Bright()}");
            }
        }

        protected void OnProcessing(Bundle bundle, string baseFolder)
        {
            Processing?.Invoke(this, new BundleFileEventArgs(bundle.OutputFileName, bundle, baseFolder, false));
        }

        protected void OnBeforeBundling(Bundle bundle, string baseFolder, bool containsChanges)
        {
            BeforeBundling?.Invoke(this, new BundleFileEventArgs(bundle.OutputFileName, bundle, baseFolder, containsChanges));
        }


        protected void OnAfterBundling(Bundle bundle, string baseFolder, bool containsChanges)
        {
            AfterBundling?.Invoke(this, new BundleFileEventArgs(bundle.OutputFileName, bundle, baseFolder, containsChanges));
        }

        protected void OnBeforeWritingSourceMap(string file, string mapFile, bool containsChanges)
        {
            BeforeWritingSourceMap?.Invoke(this, new MinifyFileEventArgs(file, mapFile, containsChanges));
        }

        protected void OnAfterWritingSourceMap(string file, string mapFile, bool containsChanges)
        {
            AfterWritingSourceMap?.Invoke(this, new MinifyFileEventArgs(file, mapFile, containsChanges));
        }

        public event EventHandler<BundleFileEventArgs> Processing;
        public event EventHandler<BundleFileEventArgs> BeforeBundling;
        public event EventHandler<BundleFileEventArgs> AfterBundling;

        public event EventHandler<MinifyFileEventArgs> BeforeWritingSourceMap;
        public event EventHandler<MinifyFileEventArgs> AfterWritingSourceMap;
    }
}
