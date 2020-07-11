using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using C = System.Console;
using CC = SpotifyDownloader.ConsoleExtensions;

namespace SpotifyDownloader.Configuration
{
    public class ConfigurationManager
    {
        private readonly string _path;
        private readonly string _userPath;

        private JObject _base;
        private JObject _user;

        private Config _config;

        public Config Config => _config ?? (_config = Load());

        public ConfigurationManager()
        {
            // ReSharper disable once PossibleNullReferenceException
            var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            _path = Path.Combine(assemblyPath, "appsettings.json");
            _userPath = Path.Combine(assemblyPath, "appsettings.user.json");
        }

        public Config Load()
        {
            _base = JObject.Parse(File.ReadAllText(_path));
            _user = JObject.Parse(File.ReadAllText(_userPath));

            var cloned = (JObject) _base.DeepClone();
            cloned.Merge(_user);


            _config = cloned.ToObject<Config>();
            Update(_config);
            return _config;
        }

        public static void Update(Config config)
        {
            if (config == null) return;

            if (config.Output != null)
            {
                // Fix extension
                if (config.Output.Extension?.StartsWith(".") == true)
                    config.Output.Extension = config.Output.Extension.Substring(1);
            }
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(Config, Formatting.Indented,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, ContractResolver = new CamelCasePropertyNamesContractResolver() });
            File.WriteAllText(_userPath, json, Encoding.UTF8);
        }

        public void Configure()
        {
            var config = Config;
            config.Output = config.Output ?? new SpotDl.Options();
            
            CC.WriteLine(ConsoleColor.DarkCyan, "Configuring");

            ConfigureDirectory(config);
            C.WriteLine();
            ConfigureExtension(config);

            CC.WriteLine(ConsoleColor.Green, "Done configuring!");
        }

        private void ConfigureDirectory(Config config)
        {
            // Directory
            C.WriteLine("Output directory:");
            CC.WriteLine(ConsoleColor.DarkGray, "Enter nothing to use your 'My Music' directory.");
            C.ForegroundColor = ConsoleColor.Yellow;
            bool exists;
            do
            {
                config.Output.Directory = C.ReadLine();
                
                // My Music
                if (string.IsNullOrWhiteSpace(config.Output.Directory))
                {
                    config.Output.Directory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                    C.SetCursorPosition(0, C.CursorTop - 1);
                    C.WriteLine(config.Output.Directory);
                }

                // Check validity
                exists = Directory.Exists(config.Output.Directory);
                if (!exists)
                    CC.WriteLine(ConsoleColor.Red, "The directory does not exist. Please enter a valid directory.");
            } while (!exists);

            C.ResetColor();
        }

        private void ConfigureExtension(Config config)
        {
            var allowed = new HashSet<string> {"m4a", "mp3", "flac"};

            // Extension
            C.WriteLine("File format (m4a, mp3, flac):");
            CC.WriteLine(ConsoleColor.DarkGray, "Enter nothing to use 'm4a'.");
            C.ForegroundColor = ConsoleColor.Yellow;
            string extension;
            do
            {
                extension = (C.ReadLine() ?? string.Empty).ToLowerInvariant();
                
                // Default
                if (string.IsNullOrWhiteSpace(extension))
                {
                    extension = "m4a";
                    PreviousLine();
                    C.WriteLine(extension);
                }
                else if (extension.StartsWith("."))
                    extension = extension.Substring(1);

                if (!allowed.Contains(extension))
                    CC.WriteLine(ConsoleColor.Red, "The format is not valid. Please enter one of the options.");
            } while (!allowed.Contains(extension));

            config.Output.Extension = extension;

            C.ResetColor();
        }

        private static void PreviousLine() => C.SetCursorPosition(0, C.CursorTop - 1);
    }
}
