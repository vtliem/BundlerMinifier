using System.Collections.Generic;
using System.Linq;
using System.IO;
using Minimatch;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Text;

namespace BundlerMinifier
{
    public class Bundle
    {
        //[JsonIgnore]
        //public string FileName { get; set; }

        [JsonProperty("outputFileName")]
        public string OutputFileName { get; set; }

        [JsonProperty("inputFiles")]
        public List<string> InputFiles { get;/*VTL*/ set; }// = new List<string>();

        [JsonProperty("minify")]
        public Dictionary<string, object> Minify { get; internal set; }// = new Dictionary<string, object>();//{ { "enabled", true } };

        [JsonProperty("includeInProject")]
        public bool? IncludeInProjectJSON { get; set; } //= true;
        [JsonIgnore]
        public bool IncludeInProject
        {
            get
            {
                if (IncludeInProjectJSON.HasValue) return IncludeInProjectJSON.Value;
                if (Config != null && Config.Default != null && Config.Default.IncludeInProjectJSON.HasValue)
                {
                    return Config.Default.IncludeInProjectJSON.Value;
                }
                return false;
            }
        }

        [JsonProperty("sourceMap")]
        public bool? SourceMapJSON { get; set; }
        [JsonIgnore]
        public bool SourceMap
        {
            get
            {
                if (SourceMapJSON.HasValue) return SourceMapJSON.Value;
                if (Config != null && Config.Default != null && Config.Default.SourceMapJSON.HasValue)
                {
                    return Config.Default.SourceMapJSON.Value;
                }
                return false;
            }
        }

        [JsonProperty("sourceMapRootPath")]
        public string SourceMapRootPathJSON { get; set; }
        [JsonIgnore]
        public string SourceMapRootPath
        {
            get
            {
                if (SourceMapRootPathJSON != null) return SourceMapRootPathJSON;
                if (Config != null && Config.Default != null && Config.Default.SourceMapRootPathJSON != null)
                {
                    return Config.Default.SourceMapRootPathJSON;
                }
                return null;
            }
        }
        #region Vtl
        /// <summary>
        /// Task category
        /// </summary>
        [JsonProperty("category")]
        public string Category { get; set; }
        [JsonIgnore]
        public BundleConfig Config { get; protected set; }
        [JsonIgnore]
        public string FileName { get { return Config == null ? null : Config.ConfigFile; } }
        internal object GetMinifyValue(string key, bool useDefault = true)
        {

            if (Minify != null && Minify.ContainsKey(key))
            {
                return Minify[key];
            }
            if (useDefault && Config != null && Config.Default != null && Config.Default.Minify != null && Config.Default.Minify.ContainsKey(key))
            {
                return Config.Default.Minify[key];
            }
            return null;
        }

        internal bool GetMinifyValue(string key, bool useDefault = true, bool defaultValue = false)
        {
            var rsl = GetMinifyValue(key, useDefault);
            return rsl == null ? defaultValue : rsl.ToString().Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        internal bool IsMinificationEnabled { get; set; }

        //{
        //    get
        //    {
        //        return "true".Equals(GetMinifyStringValue("enabled"), StringComparison.OrdinalIgnoreCase);
        //    }
        //}

        internal bool IsGzipEnabled { get; set; }
        //{
        //    get
        //    {
        //        return "true".Equals(GetMinifyStringValue("gzip"), StringComparison.OrdinalIgnoreCase);
        //    }
        //}
        internal bool AdjustRelativePaths { get; set; }
        internal string OutputFileMin { get; set; }
        #endregion Vtl

        internal string Output { get; set; }

        //internal bool IsMinificationEnabled
        //{
        //    get
        //    {
        //        return Minify.ContainsKey("enabled") && Minify["enabled"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase);
        //    }
        //}

        //internal bool IsGzipEnabled
        //{
        //    get
        //    {
        //        return Minify.ContainsKey("gzip") && Minify["gzip"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase);
        //    }
        //}

        [JsonIgnore]
        public bool OutputIsMinFile
        {
            get { return !string.IsNullOrEmpty(OutputFileName) && Path.GetFileName(OutputFileName).Contains(".min."); }
        }

        ///// <summary>
        ///// Converts the relative output file to an absolute file path.
        ///// </summary>
        //public string GetAbsoluteOutputFile()
        //{
        //    return OutputFileName;
        //    //string folder = new FileInfo(FileName).DirectoryName;
        //    //return Path.Combine(folder, OutputFileName.NormalizePath());
        //}

        ///// <summary>
        ///// Returns a list of absolute file paths of all matching input files.
        ///// </summary>
        ///// <param name="notifyOnPatternMiss">Writes to the Console if any input file is missing on disk.</param>
        //public List<string> GetAbsoluteInputFiles(bool notifyOnPatternMiss = false)
        //{
        //    List<string> files = new List<string>();

        //    if (!InputFiles.Any())
        //        return files;

        //    //VTL string folder = new DirectoryInfo(Path.GetDirectoryName(FileName)).FullName;
        //    string folder = Config.InputBase;
        //    string ext = Path.GetExtension(InputFiles.First());
        //    Options options = new Options { AllowWindowsPaths = true };

        //    foreach (string inputFile in InputFiles.Where(f => !f.StartsWith("!", StringComparison.Ordinal)))
        //    {
        //        int globIndex = inputFile.IndexOf('*');

        //        if (globIndex > -1)
        //        {
        //            string relative = string.Empty;
        //            int last = inputFile.LastIndexOf('/', globIndex);

        //            if (last > -1)
        //                relative = inputFile.Substring(0, last + 1);

        //            var output = GetAbsoluteOutputFile();
        //            var outputMin = BundleMinifier.GetMinFileName(output);

        //            string searchDir = new FileInfo(Path.Combine(folder, relative).NormalizePath()).FullName;
        //            var allFiles = Directory.EnumerateFiles(searchDir, "*" + ext, SearchOption.AllDirectories).Select(f => f.Replace(folder + FileHelpers.PathSeparatorChar, ""));

        //            var matches = Minimatcher.Filter(allFiles, inputFile, options).Select(f => Path.Combine(folder, f));
        //            matches = matches.Where(match => match != output && match != outputMin).ToList();

        //            if (notifyOnPatternMiss)
        //            {
        //                Console.WriteLine($"  No files matched the pattern {inputFile}".Orange().Bright());
        //            }

        //            files.AddRange(matches.Where(f => !files.Contains(f)));
        //        }
        //        else
        //        {
        //            string fullPath = Path.Combine(folder, inputFile.NormalizePath());

        //            if (Directory.Exists(fullPath))
        //            {
        //                DirectoryInfo dir = new DirectoryInfo(fullPath);
        //                SearchOption search = SearchOption.TopDirectoryOnly;
        //                var dirFiles = dir.GetFiles("*" + Path.GetExtension(OutputFileName), search);
        //                var collected = dirFiles.Select(f => f.FullName).Where(f => !files.Contains(f)).ToList();

        //                if (notifyOnPatternMiss && collected.Count == 0)
        //                {
        //                    Console.WriteLine($"  No files were found in {inputFile}".Orange().Bright());
        //                }

        //                files.AddRange(collected);
        //            }
        //            else
        //            {
        //                files.Add(fullPath);

        //                if (notifyOnPatternMiss && !File.Exists(fullPath))
        //                {
        //                    Console.WriteLine($"  {inputFile} was not found".Orange().Bright());
        //                }
        //            }
        //        }
        //    }

        //    // Remove files starting with a !
        //    foreach (string inputFile in InputFiles)
        //    {
        //        int globIndex = inputFile.IndexOf('!');

        //        if (globIndex == 0)
        //        {
        //            var allFiles = files.Select(f => f.Replace(folder + FileHelpers.PathSeparatorChar, ""));
        //            var matches = Minimatcher.Filter(allFiles, inputFile, options).Select(f => Path.Combine(folder, f));
        //            files = matches.ToList();
        //        }
        //    }

        //    return files;
        //}

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            if (obj == this) return true;

            Bundle other = (Bundle)obj;

            //if (GetHashCode() != other.GetHashCode()) return false;
            return string.Equals(other.OutputFileName, OutputFileName, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return (OutputFileName ?? "").GetHashCode();
        }

        ///// <summary>For the JSON.NET serializer</summary>
        //public bool ShouldSerializeIncludeInProject()
        //{
        //    if (Config == null || Config.Default == null || Config.Default.IncludeInProject != IncludeInProject) return true;
        //    //Bundle config = Config == null?null;// new Bundle();
        //    return false;// IncludeInProject != config.IncludeInProject;
        //}

        ///// <summary>For the JSON.NET serializer</summary>
        //public bool ShouldSerializeMinify()
        //{
        //    if (Minify == null || Minify.Count == 0) return false;
        //    //if (Config == null || Config.Default == null || Config.Default.Minify == null || !DictionaryEqual(Minify, Config.Default.Minify, null)) return true;
        //    return false;
        //    //Bundle config = new Bundle();
        //    //return !DictionaryEqual(Minify, config.Minify, null);
        //}

        //private static bool DictionaryEqual<TKey, TValue>(
        //    IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second,
        //    IEqualityComparer<TValue> valueComparer)
        //{
        //    if (first == second) return true;
        //    if ((first == null) || (second == null)) return false;
        //    if (first.Count != second.Count) return false;

        //    valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;

        //    foreach (var kvp in first)
        //    {
        //        TValue secondValue;
        //        if (!second.TryGetValue(kvp.Key, out secondValue)) return false;
        //        if (!valueComparer.Equals(kvp.Value, secondValue)) return false;
        //    }
        //    return true;
        //}
    }
    #region Vtl
    /// <summary>
    /// File matching filters
    /// </summary>
    public class FileFilters
    {
        private HashSet<string> includes;
        private HashSet<System.Text.RegularExpressions.Regex> regexs;
        public bool NoFilters
        {
            get { return includes == null && regexs == null; }
        }
        public FileFilters(params string[] filters)
        {
            includes = null;
            regexs = null;
            if (filters != null && filters.Length > 0)
            {
                foreach (var filter in filters)
                {
                    if (filter.Length == 0) continue;
                    if (filter.StartsWith("?"))
                    {
                        //regex
                        var regex = new System.Text.RegularExpressions.Regex(filter.Substring(1), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (regexs == null) regexs = new HashSet<System.Text.RegularExpressions.Regex>();
                        regexs.Add(regex);
                    }
                    else
                    {
                        if (includes == null) includes = new HashSet<string>();
                        includes.Add(filter);
                    }
                }
            }
        }
        /// <summary>
        /// check if file is matched filters
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool IsMatch(string file)
        {
            if (includes != null)
            {
                foreach (var filter in includes)
                {
                    if (filter.StartsWith("."))
                    {
                        if (file.EndsWith(filter, StringComparison.OrdinalIgnoreCase)) return true;
                    }
                    else if (file.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                }
            }
            if (regexs != null)
            {
                foreach (var filter in regexs)
                {
                    if (filter.IsMatch(file)) return true;
                }
            }
            return false;
        }
    }
    //public class FileFiltersJsonConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type objectType)
    //    {
    //        return false;// objectType == typeof(string[]) || objectType == typeof(string);
    //    }

    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        string[] values;
    //        if (reader.TokenType == JsonToken.StartArray)
    //        {
    //            values = serializer.Deserialize<string[]>(reader);
    //        }
    //        else if (reader.TokenType == JsonToken.String)
    //        {
    //            values = new string[] { serializer.Deserialize<string>(reader) };
    //        }
    //        else
    //        {

    //            return null;
    //        }
    //        return new FileFilters(values);
    //    }

    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {

    //        // JsonToken t = JsonToken.FromObject(value);
    //    }
    //    public override bool CanRead
    //    {
    //        get
    //        {
    //            return true;
    //        }
    //    }
    //    public override bool CanWrite
    //    {
    //        get
    //        {
    //            return false;
    //        }
    //    }
    //}
    public class BundleConverter : CustomCreationConverter<BundleExt>
    {
        public JsonSerializer Serializer { get; private set; }
        public BundleConfig Config { get; internal set; }
        public BundleConverter(BundleConfig config)
        {
            Config = config;
            Serializer = JsonSerializer.Create(new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                Converters = { this }

            });
        }
        public override BundleExt Create(Type objectType)
        {
            return new BundleExt(Config);
        }

    }

    public class BundleExt : Bundle
    {
        /// <summary>
        /// Config bundle
        /// </summary>
        [JsonProperty("config")]
        public bool IsConfig { get; set; } = false;
        /// <summary>
        /// Copy Only
        /// </summary>
        [JsonProperty("resource")]
        public bool IsResource { get; set; } = false;
        private string[] _gzipExts;
        /// <summary>
        /// When copy sources, 
        /// </summary>
        [JsonProperty("gzipExts")]
        public string[] GZipExtsJSON
        {
            get { return _gzipExts; }
            set
            {
                _gzipExts = value;
                gZipExtsList = _gzipExts == null || _gzipExts.Length == 0 ? null : new FileFilters(_gzipExts);
            }
        }
        //[JsonConverter(typeof(FileFiltersJsonConverter))]
        protected FileFilters gZipExtsList;
        [JsonIgnore]
        public FileFilters GZipExts
        {
            get
            {
                if (gZipExtsList != null) return gZipExtsList;
                if (Config != null && Config.Default != null) return Config.Default.gZipExtsList;
                return null;
            }
        }
        /// <summary>
        /// Copy subfolders
        /// </summary>
        [JsonProperty("recursively")]
        public bool? RecursivelyJSON { get; set; }
        [JsonIgnore]
        public bool Recursively
        {
            get
            {
                if (RecursivelyJSON.HasValue) return RecursivelyJSON.Value;
                if (Config != null && Config.Default != null && Config.Default.RecursivelyJSON.HasValue) return Config.Default.RecursivelyJSON.Value;
                return false;
            }
        }
        private string[] _excludes;
        /// <summary>
        /// Don't copy if match
        /// </summary>
        [JsonProperty("excludes")]
        public string[] ExcludesJSON
        {
            get { return _excludes; }
            set
            {
                _excludes = value;
                Excludes = _excludes == null || _excludes.Length == 0 ? null : new FileFilters(_excludes);
            }
        }
        // [JsonConverter(typeof(FileFiltersJsonConverter))]
        [JsonIgnore]
        public FileFilters Excludes { get; protected set; }

        public void SetDefault()
        {
            if (Excludes == null)
            {
                Excludes = BundleConfig.DefaultExcludes;
            }
            if (gZipExtsList == null)
            {
                gZipExtsList = BundleConfig.DefaultGZipExts;
            }
        }
        public BundleExt(BundleConfig config)
        {
            Config = config;
            //if (config != null && config.Default != null)
            //{
            //    //if (Config.Default.Minify.Count > 0)
            //    //{
            //    //    foreach (var item in Config.Default.Minify)
            //    //    {
            //    //        Minify.Add(item.Key, item.Value);
            //    //    }
            //    //}
            //    //IncludeInProject = Config.Default.IncludeInProject;
            //    //SourceMap = Config.Default.SourceMap;
            //    //SourceMapRootPath = Config.Default.SourceMapRootPath;
            //    GZipExts = Config.Default.GZipExts;
            //    Recursively = Config.Default.Recursively;
            //}
        }
        public BundleExt(BundleExt src, string input, string output)
        {
            Config = src.Config;
            OutputFileName = output;
            InputFiles = new List<string> { input };
            Minify = src.Minify;
            IncludeInProjectJSON = src.IncludeInProjectJSON;
            SourceMapJSON = src.SourceMapJSON;
            SourceMapRootPathJSON = src.SourceMapRootPathJSON;
            IsResource = src.IsResource;
            GZipExtsJSON = src.GZipExtsJSON;
            Category = src.Category;
            Prepare();
        }
        protected void Prepare()
        {
            IsMinificationEnabled = GetMinifyValue("enabled", true, false) && BundleConfig.MinifySupporteds.IsMatch(OutputFileName);
            IsGzipEnabled = GetMinifyValue("gzip", true, false) && (GZipExts == null || GZipExts.IsMatch(OutputFileName));
            AdjustRelativePaths = GetMinifyValue("adjustRelativePaths", true, true) && OutputFileName.EndsWith(".css", StringComparison.OrdinalIgnoreCase);
            if (IsMinificationEnabled)
            {
                if (Config.NoMinFile)
                {
                    OutputFileMin = OutputFileName;
                }
                else
                {

                    OutputFileMin = BundleMinifier.GetMinFileName(OutputFileName);
                }
            }
            else
            {
                OutputFileMin = null;
            }
        }
        ///// <summary>
        ///// Converts the relative output file to an absolute file path.
        ///// </summary>
        //public override string GetAbsoluteOutputFile()
        //{
        //    return Path.Combine(Config.OutputBase, OutputFileName.NormalizePath());
        //}
        /// <summary>
        /// Check input file
        /// </summary>
        /// <param name="f">input file</param>
        /// <param name="matchers">matchers</param>
        /// <param name="pattern">pattern matcher</param>
        /// <param name="options">matcher options</param>
        /// <returns></returns>
        private bool IsMatch(string f, IEnumerable<Minimatcher> matchers, string pattern, Options options)
        {
            if (matchers != null)
            {
                foreach (var m in matchers)
                {
                    if (!m.IsMatch(f)) return false;
                }
            }
            if (!string.IsNullOrEmpty(pattern))
            {
                if (!new Minimatcher(pattern, options).IsMatch(f)) return false;
            }
            if ((Excludes == null || Excludes.NoFilters) && (Config.Default.Excludes == null || Config.Default.Excludes.NoFilters)) return true;
            f = FileHelpers.PathSeparatorChar + f;
            if (Excludes != null && Excludes.IsMatch(f)) return false;
            if (Config.Default.Excludes != null && Config.Default.Excludes.IsMatch(f)) return false;
            return true;

        }

        private int AddToList(List<Bundle> bundles)
        {
            var output = string.IsNullOrEmpty(OutputFileName) ? Config.OutputBase : Path.Combine(Config.OutputBase, OutputFileName.NormalizePath());
            if (InputFiles == null || InputFiles.Count == 0)
            {
                // OutputFileName = output;
                InputFiles = null;
                //No input
                // bundles.Add(this);
                // Config.MissingInputFiles.Add("No inputFiles");
                return 0;
            }
            //copy from bundles
            List<string> files = new List<string>();
            string folder = Config.InputBase;
            Options options = new Options { AllowWindowsPaths = true };


            //VTL string folder = new DirectoryInfo(Path.GetDirectoryName(FileName)).FullName;
            //#region bundle input files
            //OutputFilePath = Path.Combine(Config.OutputBase, OutputFileName.NormalizePath());
            //string ext = Path.GetExtension(InputFiles.First());
            int backupCount = bundles.Count;

            var excludeFilters = InputFiles.Where(f => f.Length > 0 && f[0] == '!').Select(f => new Minimatcher(f, options));
            //  var theSameIO = Config.OutputBase == Config.InputBase;

            string outputMin = IsResource || Config.NoMinFile || OutputIsMinFile ? null : BundleMinifier.GetMinFileName(output);
            foreach (string inputFile in InputFiles)
            {
                if (inputFile.Length > 0 && inputFile[0] == '!') continue;
                //pattern
                int globIndex = inputFile.IndexOf('*');

                if (globIndex > -1)
                {
                    string relative = string.Empty;
                    int last = inputFile.LastIndexOf('/', globIndex);

                    if (last > -1)
                        relative = inputFile.Substring(0, last + 1);
                    SearchOption search = Recursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    string searchDir = new FileInfo(Path.Combine(folder, relative).NormalizePath()).FullName;
                    string ext = IsResource ? "*" : "*" + Path.GetExtension(InputFiles.First());
                    var matches = Directory.EnumerateFiles(searchDir, ext, search)
                                   .Select(f => f.Replace(folder + FileHelpers.PathSeparatorChar, ""))
                                   .Where(f => IsMatch(f, excludeFilters, inputFile, options));
                    if (matches == null || !matches.Any())
                    {
                        Config.MissingInputFiles.Add(inputFile);
                        continue;
                    }
                    int count = 0;
                    string file;
                    foreach (var f in matches)
                    {
                        file = Path.Combine(folder, f);
                        if (IsResource)
                        {
                            var outPath = Path.Combine(output, f);
                            //outputMin = Config.NoMinFile || OutputIsMinFile ? null : BundleMinifier.GetMinFileName(output);
                            if (outPath != file && (Config.NoMinFile || OutputIsMinFile || BundleMinifier.GetMinFileName(outPath) != file))
                            {
                                bundles.Add(new BundleExt(this, file, outPath));
                                ++count;
                            }
                        }
                        else if (file != output && file != outputMin && !files.Contains(file))
                        {
                            files.Add(f);
                            ++count;
                        }

                    }
                    if (count == 0)
                    {
                        Config.MissingInputFiles.Add(inputFile);
                    }
                }
                else
                {
                    string fullPath = Path.Combine(folder, inputFile.NormalizePath());
                    //directory
                    if (Directory.Exists(fullPath))
                    {
                        DirectoryInfo dir = new DirectoryInfo(fullPath);
                        SearchOption search = Recursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                        var matches = dir.GetFiles("*" + Path.GetExtension(OutputFileName), search)
                                       .Select(f => f.FullName.Replace(folder + FileHelpers.PathSeparatorChar, ""))
                                       .Where(f => IsMatch(f, excludeFilters, null, null));
                        if (matches == null || !matches.Any())
                        {
                            Config.MissingInputFiles.Add(inputFile);
                            continue;
                        }
                        int count = 0;
                        string file;
                        foreach (var f in matches)
                        {
                            file = Path.Combine(folder, f);
                            if (IsResource)
                            {
                                var outPath = Path.Combine(output, f);
                                if (outPath != file && (Config.NoMinFile || OutputIsMinFile || BundleMinifier.GetMinFileName(outPath) != file))
                                {
                                    bundles.Add(new BundleExt(this, file, outPath));
                                    ++count;
                                }
                            }
                            else if (file != output && file != outputMin && !files.Contains(file))
                            {
                                files.Add(f);
                                ++count;
                            }

                        }
                        if (count == 0)
                        {
                            Config.MissingInputFiles.Add(inputFile);
                        }
                    }
                    else
                    {
                        //file
                        if (!File.Exists(fullPath) || files.Contains(fullPath) || !IsMatch(fullPath.Replace(folder + FileHelpers.PathSeparatorChar, ""), excludeFilters, null, null))
                        {
                            Config.MissingInputFiles.Add(inputFile);
                        }
                        else if (IsResource)
                        {
                            string f = fullPath.Substring(Config.InputBaseLength + 1);
                            var outPath = Path.Combine(output, f);
                            if (outPath != fullPath)
                                bundles.Add(new BundleExt(this, fullPath, outPath));
                            else
                                Config.MissingInputFiles.Add(inputFile);
                        }
                        else //file != output && file != outputMin && !files.Contains(file)
                        {
                            files.Add(fullPath);
                        }
                        //if (notifyOnPatternMiss && !File.Exists(fullPath))
                        //{
                        //    Console.WriteLine($"  {inputFile} was not found".Orange().Bright());
                        //}
                    }
                }

            }
            if (!IsResource)
            {
                if (files.Count == 0)
                {
                    InputFiles = null;
                    return 0;
                }
                OutputFileName = output;
                InputFiles = files;
                Prepare();
                bundles.Add(this);
            }
            return bundles.Count - backupCount;

        }
        private static T GetJsonProperty<T>(JToken json, string key, T defaultVal)
        {
            foreach (var p in json.Children<JProperty>())
            {
                if (p.Name == key)
                {
                    if (p.HasValues)
                        return p.Value.Value<T>();
                    return defaultVal;
                }
            }
            return defaultVal;
        }
        public static BundleConfig GetConfig(string configFile)
        {
            if (string.IsNullOrEmpty(configFile) || !File.Exists(configFile = new FileInfo(configFile).FullName))
            {
                new BundleConfig(configFile, null, false, true);
            }

            var json = JArray.Parse(File.ReadAllText(configFile));
            var converter = new BundleConverter(null);
            foreach (var item in json.Children())
            {
                if (GetJsonProperty(item, "config", false))
                {
                    return new BundleConfig(configFile, item.ToObject<BundleExt>(converter.Serializer), GetJsonProperty(item, "deleteOnClean", false), GetJsonProperty(item, "noMinFile", false));
                }
            }
            return new BundleConfig(configFile, null, false, true);
        }
        /// <summary>
        /// Read config.josn
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="bundles"></param>
        /// <param name="throwEx">raise exception when error</param>
        /// <param name="forUpdate">for update config.json</param>
        /// <returns></returns>
        public static bool TryGetBundles(string configFile, out IEnumerable<Bundle> bundles, bool throwEx, bool forUpdate = false)
        {
            try
            {

                if (string.IsNullOrEmpty(configFile) || !File.Exists(configFile = new FileInfo(configFile).FullName))
                {
                    if (throwEx)
                    {
                        var ex = new IOException("config file not found:" + configFile);
                        BundleConfig.Log(Path.GetDirectoryName(configFile), "GetBundlers failed", ex);
                        throw ex;// new IOException("config file not found:" + configFile);
                    }
                    bundles = Enumerable.Empty<Bundle>();
                    return false;
                }

                var converter = new BundleConverter(null);
                var json = JArray.Parse(File.ReadAllText(configFile));
                if (forUpdate)
                {
                    bundles = json.ToObject<BundleExt[]>(converter.Serializer);
                    return true;
                }
                foreach (var item in json.Children())
                {
                    if (GetJsonProperty(item, "config", false))
                    {
                        converter.Config = new BundleConfig(configFile, item.ToObject<BundleExt>(converter.Serializer), GetJsonProperty(item, "deleteOnClean", false), GetJsonProperty(item, "noMinFile", false));
                    }
                    if (converter.Config != null) break;
                }
                if (converter.Config == null)
                {
                    converter.Config = new BundleConfig(configFile, null, false, true);
                }

                var rsl = new List<Bundle>();
                bundles = rsl;
                foreach (var item in json)
                {
                    var bundle = item.ToObject<BundleExt>(converter.Serializer);
                    if (bundle.IsConfig)
                    {
                        continue;
                    }
                    if (bundle.AddToList(rsl) == 0)
                    {
                        converter.Config.MissingInputFiles.Add("No input Files");
                    }

                }
                if (converter.Config.MissingInputFiles != null && converter.Config.MissingInputFiles.Count > 0)
                {
                    BundleConfig.Log(converter.Config.OutputBase, "Missing files:", converter.Config.MissingInputFiles);
                }
                return true;
            }
            catch (Exception e)
            {
                BundleConfig.Log(Path.GetDirectoryName(configFile), "GetBundlers failed", e);
                if (throwEx) throw;
                bundles = null;
                return false;
            }
        }
        /// <summary>
        /// check File writime and size
        /// </summary>
        /// <param name="srcFile"></param>
        /// <param name="dstFile"></param>
        /// <param name="checkSize"></param>
        /// <returns></returns>
        public static bool IsChanged(string srcFile, string dstFile, bool checkSize = true)
        {
            if (!File.Exists(dstFile)) return true;
            var f1 = new FileInfo(srcFile);
            var f2 = new FileInfo(dstFile);
            if (f1.LastWriteTimeUtc > f2.LastWriteTimeUtc) return true;
            return checkSize && f1.Length != f2.Length;
        }

        public override bool Equals(object obj)
        {
            if (!base.Equals(obj)) return false;
            if (obj is BundleExt)
            {
                return !IsResource;
            }
            var other = (BundleExt)obj;
            if (IsResource == other.IsResource && !IsResource) return true;

            if (InputFiles == null || !InputFiles.Any())
            {
                if (other.InputFiles != null && other.InputFiles.Any()) return false;
            }
            else if (other.InputFiles == null && !other.InputFiles.Any()) return false;
            if (InputFiles.Except(other.InputFiles, StringComparer.OrdinalIgnoreCase).Any()) return false;
            if (other.InputFiles.Except(InputFiles, StringComparer.OrdinalIgnoreCase).Any()) return false;
            return true;
        }
        public override int GetHashCode()
        {
            int rsl = base.GetHashCode();
            return rsl ^ 31;
        }

    }
    public class BundleConfig
    {
        internal const string LOG_FILE = "bundleminifier_vtl.log";
        internal static readonly FileFilters DefaultExcludes = new FileFilters(new string[] { "\\bbundleconfig.json", "\\bbundleconfig.json.bindings", "\\.ds_store", "\\bthumbs.db", "\\.svn", "\\bundleminifier_vtl.log" });
        internal static readonly FileFilters DefaultGZipExts = new FileFilters(new string[] { ".js", ".css", ".html", ".html", ".svg", ".woff", ".ttf", ".txt", ".json", ".md", ".bcmap" });
        internal static readonly FileFilters MinifySupporteds = new FileFilters(new string[] { ".js", ".css", ".html", ".html" });
        /// <summary>
        /// Output folder
        /// </summary>
        public string OutputBase { get; private set; }
        /// <summary>
        /// Input folder
        /// </summary>
        public string InputBase { get; private set; }
        /// <summary>
        /// Input base path length
        /// </summary>
        public int InputBaseLength { get; private set; }
        /// <summary>
        /// Config file
        /// </summary>
        public string ConfigFile { get; private set; }
        /// <summary>
        /// Default options
        /// </summary>
        public BundleExt Default { get; private set; }
        /// <summary>
        /// No input file
        /// </summary>
        public List<string> MissingInputFiles { get; } = new List<string>();
        /// <summary>
        /// Delete output base on clean
        /// </summary>
        public bool DeleteOutputBaseOnClean { get; private set; }
        /// <summary>
        /// Don't create *.min.*
        /// </summary>
        public bool NoMinFile { get; private set; }
        private static BundleExt CreateDefault()
        {
            var bundle = new BundleExt(null);
            bundle.Minify = new Dictionary<string, object>() { { "enabled", true }, { "adjustRelativePaths", true } };
            return bundle;
        }
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="configBundle"></param>
        public BundleConfig(string configFile, BundleExt configBundle, bool deleteOnClean, bool nomin)
        {
            ConfigFile = configFile;
            if (configBundle == null)
            {
                Default = CreateDefault();
            }
            else
            {
                Default = configBundle;

                //if (Default.Excludes == null)
                //{
                //    Default.Excludes = DefaultExcludes;
                //}
                //if (Default.GZipExts == null)
                //{
                //    Default.GZipExts = DefaultGZipExts;
                //}
            }
            Default.SetDefault();
            InputBase = Path.GetDirectoryName(ConfigFile);
            InputBaseLength = InputBase.Length;
            if (InputBase[InputBaseLength - 1] == Path.DirectorySeparatorChar)
            {
                --InputBaseLength;
            }
            if (string.IsNullOrEmpty(Default.OutputFileName))
            {
                OutputBase = InputBase;
            }
            else
            {
                Default.OutputFileName = Default.OutputFileName.NormalizePath();

                if (Path.IsPathRooted(Default.OutputFileName))
                {
                    OutputBase = Path.GetFullPath(Default.OutputFileName);
                }
                else
                {
                    OutputBase = Path.GetFullPath(Path.Combine(InputBase, Default.OutputFileName));
                }
            }
            if (OutputBase[OutputBase.Length - 1] == FileHelpers.PathSeparatorChar)
            {
                OutputBase = OutputBase.Substring(0, OutputBase.Length - 1);
            }
            DeleteOutputBaseOnClean = deleteOnClean && !InputBase.StartsWith(OutputBase);
            NoMinFile = nomin;
        }

        private static void LogObject(StreamWriter writer, object obj)
        {
            if (obj == null)
            {
                writer.WriteLine("null");
                return;
            }
            if (obj is string)
            {
                writer.WriteLine((string)obj);
                return;
            }
            if (obj is System.Collections.IEnumerable)
            {
                System.Collections.IEnumerable lst = (System.Collections.IEnumerable)obj;
                foreach (var item in lst)
                {
                    LogObject(writer, item);
                }
                return;
            }
            writer.WriteLine(obj);
        }
        /// <summary>
        /// write log
        /// </summary>
        /// <param name="outBase"></param>
        /// <param name="log"></param>
        /// <param name="args"></param>
        public static void Log(string outBase, string log, params object[] args)
        {

            if (!Directory.Exists(outBase)) Directory.CreateDirectory(outBase);
            string path = Path.Combine(outBase, LOG_FILE);
            using (var writer = new StreamWriter(File.OpenWrite(path), UTF8Encoding.UTF8))
            {
                if (log != null)
                {
                    writer.WriteLine(new DateTime().ToString("yyyy/MM/dd HH:mm:ss"));
                }
                else
                {
                    writer.Write(new DateTime().ToString("yyyy/MM/dd HH:mm:ss   "));
                    writer.WriteLine(log);
                }
                if (args != null && args.Length > 0)
                {
                    foreach (var obj in args)
                    {
                        LogObject(writer, obj);
                    }
                }
            }
        }
    }

    #endregion vtl
}
