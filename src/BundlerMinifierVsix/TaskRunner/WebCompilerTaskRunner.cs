using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BundlerMinifier;
using Microsoft.VisualStudio.TaskRunnerExplorer;

namespace BundlerMinifierVsix
{
    [TaskRunnerExport(Constants.CONFIG_FILENAME)]
    class BundlerTaskRunner : ITaskRunner
    {
        private static ImageSource _icon;
        private static string _exe;

        public BundlerTaskRunner()
        {
            if (_icon == null || _exe == null)
            {
                string folder = GetExecutableFolder();
                _icon = new BitmapImage(new Uri(Path.Combine(folder, "Resources\\logo.png")));// new BitmapImage(new Uri(@"pack://application:,,,/WebCompilerVsix;component/Resources/logo.png"));
                _exe = Path.Combine(folder, "BundlerMinifier.exe");
            }
        }

        public List<ITaskRunnerOption> Options
        {
            get { return null; }
        }

        public async Task<ITaskRunnerConfig> ParseConfig(ITaskRunnerCommandContext context, string configPath)
        {
            return await Task.Run(() =>
            {
                if (BundlerMinifierPackage.Options == null || !BundlerMinifierPackage.Options.EnableTaskRunnerExplorer)
                    return null;

                ITaskRunnerNode hierarchy = LoadHierarchy(configPath);

                return new TaskRunnerConfig(context, hierarchy, _icon);
            });
        }

        private ITaskRunnerNode LoadHierarchy(string configPath)
        {
            var root = new TaskRunnerNode(Vsix.Name);
            var cwd = Path.GetDirectoryName(configPath);

            root.Children.Add(new TaskRunnerNode(Resources.Text.TaskUpdateAllFilesName, true)
            {
                Description = Resources.Text.TaskUpdateAllFilesDescription.AddParams(Constants.CONFIG_FILENAME),
                Command = GetCommand(cwd, $"\"{configPath}\"")
            });

            root.Children.Add(new TaskRunnerNode(Resources.Text.TaskCleanOutputFilesName, true)
            {
                Description = Resources.Text.TaskCleanOutputFilesDescription,
                Command = GetCommand(cwd, $"clean \"{configPath}\"")
            });

            //var list = new List<ITaskRunnerNode> {
            //   
            //   
            //   
            //   
            //};
            var configs = BundleHandler.GetBundles(configPath, false);
            if (configs != null && configs.Any())
            {
                configs = configs.OrderBy(b => b.Category).ThenBy(b => b.OutputFileName);
                Bundle bundle;
                var list = new List<ITaskRunnerNode>();
                //categories
                ITaskRunnerNode task;
                while (configs != null && (bundle = configs.FirstOrDefault(b => b.Category != null)) != null)
                {
                    //js
                    if ((task = GetFileType(ref configs, false, bundle.Category, ".js", false, false, ".js")) != null) list.Add(task);
                    //css
                    if ((task = GetFileType(ref configs, false, bundle.Category, ".css", false, false, ".css")) != null) list.Add(task);
                    //html
                    if ((task = GetFileType(ref configs, false, bundle.Category, ".html", false, false, ".html", ".htm")) != null) list.Add(task);
                    //images
                    if ((task = GetFileType(ref configs, false, bundle.Category, "Images", false, false, ".png", ".jpg", "jpeg", ".gif", ".svg", ".ico", ".bmp")) != null) list.Add(task);
                    //fonts
                    if ((task = GetFileType(ref configs, false, bundle.Category, "Fonts", false, false, ".eot", ".ttf", ".woff", ".woff2")) != null) list.Add(task);
                    //others
                    if ((task = GetFileType(ref configs, false, bundle.Category, "Others", true, true, ".js", ".css", ".html", ".htm", ".png", ".jpg", "jpeg", ".gif", ".svg", ".ico", ".bmp", ".eot", ".ttf", ".woff", ".woff2")) != null) list.Add(task);
                }
                //js
                if ((task = GetFileType(ref configs, false, null, ".js", false, false, ".js")) != null) list.Add(task);
                //css
                if ((task = GetFileType(ref configs, false, null, ".css", false, false, ".css")) != null) list.Add(task);
                //html
                if ((task = GetFileType(ref configs, false, null, ".html", false, false, ".html", ".htm")) != null) list.Add(task);
                //images
                if ((task = GetFileType(ref configs, false, null, "Images", false, false, ".png", ".jpg", "jpeg", ".gif", ".svg", ".ico", ".bmp")) != null) list.Add(task);
                //fonts
                if ((task = GetFileType(ref configs, false, null, "Fonts", false, false, ".eot", ".ttf", ".woff", ".woff2")) != null) list.Add(task);
                //others
                if ((task = GetFileType(ref configs, true, null, "Others", true, true, ".js", ".css", ".html", ".htm", ".png", ".jpg", "jpeg", ".gif", ".svg", ".ico", ".bmp", ".eot", ".ttf", ".woff", ".woff2")) != null) list.Add(task);

                //if (task != null) list.Add(task);

                //{
                //    GetFileType(configPath, ".js", configs),
                //    
                //    
                //    
                //};
                //list.Add(GetFileType(configPath, ".js", configs));
                //{
                //     
                //     
                //     
                //     
                //     GetFileType(configPath, "*",configs,new List<string>{".js",".css",".html",".htm" }),
                //};
                if (list.Any())
                    root.Children.AddRange(list);//.Where(i => i != null));
            }
            return root;
        }

        private ITaskRunnerNode GetFileType(ref IEnumerable<Bundle> configs, bool all, string category, string label, bool not, bool notCheckExt, params string[] extensions)
        {
            if (configs == null || !configs.Any()) { configs = null; return null; }
            IEnumerable<Bundle> types;
            if (all)
            {
                //all
                types = configs;
                configs = null;
            }
            else
            {
                var tasks = new List<Bundle>();
                types = tasks;
                var rsl = new List<Bundle>();

                foreach (var bundle in configs)
                {
                    //category
                    if (category == null && !string.IsNullOrEmpty(bundle.Category))
                    {
                        rsl.Add(bundle);
                        continue;
                    }
                    if (category != null && category != bundle.Category)
                    {
                        rsl.Add(bundle);
                        continue;
                    }
                    if (!notCheckExt && extensions.Contains(Path.GetExtension(bundle.OutputFileName), StringComparer.OrdinalIgnoreCase) == not)
                    {
                        rsl.Add(bundle);
                        continue;
                    }
                    tasks.Add(bundle);


                }

                configs = rsl.Any() ? rsl : null;
            }

            //var types = extension == "*" ? configs : configs.Where(c => Path.GetExtension(c.OutputFileName).Equals(extension, StringComparison.OrdinalIgnoreCase));
            //
            //if (extension == null && (excludes == null || !excludes.Any()))
            //{
            //    types = configs;
            //}
            //else
            //{
            //    types = configs.Where(c =>
            //    {
            //        var ex = Path.GetExtension(c.OutputFileName);
            //        if (string.IsNullOrEmpty(ex))
            //        {
            //            if (extension != null || (excludes != null && excludes.Contains(ex))) return false;
            //            return true;
            //        }
            //        if (extension != null && !extension.Equals(ex, StringComparison.OrdinalIgnoreCase)) return false;
            //        if (excludes != null && excludes.Contains(ex, StringComparer.OrdinalIgnoreCase)) return false;
            //        return true;
            //    }/*Path.GetExtension(c.OutputFileName).Equals(extension, StringComparison.OrdinalIgnoreCase) && (excludes==null)*/);
            //}
            if (types == null || !types.Any())
                return null;
            var configPath = types.FirstOrDefault().FileName;
            string cwd = Path.GetDirectoryName(configPath);
            string friendlyName;

            if (category == null)
                friendlyName = GetFriendlyName(label);
            else if (label == null)
                friendlyName = category;
            else
                friendlyName = category + "(" + GetFriendlyName(label) + ")";

            TaskRunnerNode type = new TaskRunnerNode(friendlyName, true)
            {
                Command = GetCommand(cwd, (not ? "\"!*" : "\"*") + string.Join("*", extensions) + ":" + (category ?? "") + "\" \"" + configPath + "\"")
            };

            //if (extension != null)
            //{
            //    type.Command = GetCommand(cwd, $"*{label} \"{configPath}\"");
            //}
            int len = types.FirstOrDefault().Config.OutputBase.Length + 1;
            foreach (var config in types)
            {
                TaskRunnerNode child = new TaskRunnerNode(config.OutputFileName.Substring(len), true)
                {
                    Command = GetCommand(cwd, $"\"{config.OutputFileName}\" \"{configPath}\"")
                };

                type.Children.Add(child);
            }

            return type;
        }

        private string GetFriendlyName(string extension)
        {
            if (extension == null)
            {
                return "Others";
            }
            switch (extension.ToUpperInvariant())
            {
                case ".CSS":
                    return "Stylesheets";
                case ".JS":
                    return "JavaScript";
                case ".HTML":
                case ".HTM":
                    return "HTML";
            }

            return extension;
        }

        private ITaskRunnerCommand GetCommand(string cwd, string arguments)
        {
            ITaskRunnerCommand command = new TaskRunnerCommand(cwd, _exe, arguments);

            return command;
        }

        private static string GetExecutableFolder()
        {
            string assembly = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(assembly);
        }
    }
}
