using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using SpotifyDownloader.Configuration;
using C = System.Console;
using CC = SpotifyDownloader.ConsoleExtensions;

namespace SpotifyDownloader
{
    internal class Program
    {
        private static ConfigurationManager _manager;

        private static void Main(string[] args)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            // Title
            C.Write("| ");
            CC.Write(ConsoleColor.Cyan, "Spotify-Downloader Handler");
            C.WriteLine($" v{version.Major}.{version.Minor}.{version.Build}");

            C.Write("| ");
            C.Write("Repository: ");
            CC.WriteLine(ConsoleColor.White, "https://github.com/Silverfeelin/spotdl-handler");
            C.Write("| ");
            C.Write("Powered by Spotify-Downloader: ");
            CC.WriteLine(ConsoleColor.White, "https://github.com/ritiek/spotify-downloader");
            
            //C.WriteLine("");

            try
            {
                _manager = new ConfigurationManager();

                // Configure
                if (args.Length == 0 || args.Any(a => a == "--configure"))
                {
                    _manager.Configure();
                    _manager.Save();
                    Exit(); return;
                }

                // Run
                Run(args);
            }
            catch (Exception exc)
            {
                CC.WriteLine(ConsoleColor.Red, $"Unhandled exception:\n{exc}");
                C.WriteLine("Press Enter to exit...");
                C.ReadLine();
                Environment.Exit(1);
            }

            Exit();
        }

        private static void Run(string[] args)
        {
            var config = _manager.Config;
            
            /* Check output directory */
            if (!ValidateDirectory(config))
            {
                Exit(1); return;
            }

            // Fix args
            if (args[0].IndexOf("%20", StringComparison.Ordinal) != -1) args = args[0].Split("%20");
            if (args[0].IndexOf("spotdl:", StringComparison.Ordinal) == 0) args[0] = args[0].Substring(7);

            // Get type
            var reg = new Regex("https:\\/\\/open\\.spotify\\.com\\/([a-zA-Z]+)\\/[a-zA-Z0-9]+");
            var match = reg.Match(args[0]);
            if (!match.Success)
            {
                CC.WriteLine(ConsoleColor.Red, "Incorrect Spotify URL.");
                Exit(1); return;
            }

            // Download
            if (args.Skip(1).Any(a => a == "-m"))
            {
                config.Output.Manual = true;
                config.Output.Overwrite = "force";
            }

            // Detmerine download type.
            var valid = true;
            var spotdl = new SpotDl();
            switch (match.Groups[1].Value)
            {
                case "track":
                    config.Output.SongUrl = args[0];
                    break;
                case "album":
                    config.Output.AlbumUrl = args[0];
                    break;
                case "playlist":
                    config.Output.PlaylistUrl = args[0];
                    break;
                default:
                    C.WriteLine("Type {0} not supported! Downloading is limited to tracks, albums and playlists.", match.Groups[1].Value);
                    valid = false;
                    break;
            }

            if (valid)
            {
                spotdl.Download(config.Output);
            }

            CC.WriteLine(ConsoleColor.Green, "Done downloading!");
        }

        private static bool ValidateDirectory(Config config)
        {
            var directory = config.Output.Directory;
            if (string.IsNullOrWhiteSpace(directory))
            {
                CC.WriteLine(ConsoleColor.Red, "Output directory not configured. Please run the application without any arguments to configure settings.");
                return false;
            }
            if (!Directory.Exists(directory))
            {
                CC.Write(ConsoleColor.Red, "Output directory ");
                CC.Write(ConsoleColor.Yellow, directory);
                CC.Write(ConsoleColor.Red, " does not exist. Please run the application without any arguments to configure settings.");
                return false;
            }

            return true;
        }

        private static void Exit(int code = 0)
        {
            if (_manager.Config.KeepOpen)
            {
                C.ResetColor();
                C.WriteLine("Press Enter to exit...");
                C.ReadLine();
            }
            Environment.Exit(code);
        }
    }
}
