using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace SpotifyDownloader
{
    internal class Program
    {
        private static JObject _config;

        private static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("= Spotify Downloader =");
            /* Load config */
            LoadConfig();

            /* Configure */
            if (args.Length == 0)
            {
                Configure();
                return;
            }

            /* Check output directory */
            var directory = _config["output"]["directory"].Value<string>();
            if (string.IsNullOrWhiteSpace(directory))
            {
                WaitAndExit("Output directory not configured. Please run the application without any arguments to configure settings."); return;
            }
            if (!Directory.Exists(directory))
            {
                WaitAndExit("Output directory '{0}' does not exist. Please run the application without any arguments to configure settings."); return;
            }

            // Fix args
            if (args[0].IndexOf("%20", StringComparison.Ordinal) != -1) args = args[0].Split("%20");
            if (args[0].IndexOf("spotdl:", StringComparison.Ordinal) == 0) args[0] = args[0].Substring(7);

            // Validate args
            if (args.Length != 1)
            {
                WaitAndExit("Expected 1 argument. Got {0}.\nUsage:\ndotnet SpotifyDownloader.dll https://open.spotify.com/track/ID", args.Length); return;
            }

            // Get type
            var reg = new Regex("https:\\/\\/open\\.spotify\\.com\\/([a-zA-Z]+)\\/[a-zA-Z0-9]+");
            var match = reg.Match(args[0]);
            if (!match.Success)
            {
                WaitAndExit("Incorrect Spotify URL."); return;
            }

            // Download
            var options = _config["output"].ToObject<SpotDl.Options>();

            var valid = true;
            var spotdl = new SpotDl();
            switch (match.Groups[1].Value)
            {
                case "track":
                    options.SongUrl = args[0];
                    break;
                case "album":
                    options.AlbumUrl = args[0];
                    break;
                case "playlist":
                    options.PlaylistUrl = args[0];
                    break;
                default:
                    Console.WriteLine("Type {0} not supported! Downloading is limited to tracks, albums and playlists.", match.Groups[1].Value);
                    valid = false;
                    break;
            }

            if (valid)
            {
                spotdl.Download(options);
            }

            // Wait for exit...
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Done downloading!");
            Console.ResetColor();

            if (_config["keepOpen"]?.Value<bool>() == true)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
            }
        }

        #region Config

        private static void LoadConfig()
        {
            var assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            var appsettingsPath = Path.Combine(assemblyPath, "appsettings.json");
            var userAppsettingsPath = Path.Combine(assemblyPath, "appsettings.user.json");

            _config = JObject.Parse(File.ReadAllText(appsettingsPath));
            if (File.Exists(userAppsettingsPath))
            {
                var userConfig = JObject.Parse(File.ReadAllText(userAppsettingsPath));
                _config.Merge(userConfig);
            }
        }

        static void Configure()
        {
            Console.WriteLine("Configure SpotifyDownloader. Type 'exit' at any time to stop early.");

            /* Directory */
            Console.WriteLine("Download folder (blank for 'My Music'):");
            var path = Console.ReadLine();
            if (path == "exit") return;
            if (!string.IsNullOrWhiteSpace(path))
            {
                _config["output"]["directory"] = path;
            }
            else
            {
                Console.WriteLine("Defaulting to 'My Music'.");
                _config["output"]["directory"] = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            }

            /* Extension */
            Console.WriteLine("File extension (blank for '.m4a'):");
            Console.WriteLine("Supported: .mp3, .m4a, .flac");
            var ext = Console.ReadLine();
            if (ext == "exit") { SaveConfig(); return; }
            if (string.IsNullOrWhiteSpace(ext)) ext = ".m4a";
            _config["output"]["extension"] = ext;

            /* Save */
            SaveConfig();
        }

        private static async void SaveConfig()
        {
            var path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "appsettings.user.json");
            await File.WriteAllTextAsync(path, _config.ToString(Formatting.Indented));
        }

        #endregion

        /* Shows a message and waits for a key before exiting. */
        private static void WaitAndExit(string message, params object[] args)
        {
            Console.WriteLine(message, args);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
