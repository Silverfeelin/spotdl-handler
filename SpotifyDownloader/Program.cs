using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace SpotifyDownloader
{
    class Program
    {
        static ManualResetEvent stopEvent;
        
        static JObject config;

        static void Main(string[] args)
        {
            /* Load config */
            LoadConfig();

            /* Configure */
            if (args.Length == 0)
            {
                Configure();
                return;
            }

            string extension = config["output"]["extension"].Value<string>();

            /* Check output directory */
            var directory = config["output"]["directory"].Value<string>();
            if (string.IsNullOrWhiteSpace(directory))
            {
                WaitAndExit("Output directory not configured. Please run the application without any arguments to configure settings."); return;
            }
            if (!Directory.Exists(directory))
            {
                WaitAndExit("Output directory '{0}' does not exist. Please run the application without any arguments to configure settings."); return;
            }

            // Fix args
            if (args[0].IndexOf("%20") != -1) args = args[0].Split("%20");
            if (args[0].IndexOf("spotdl:") == 0) args[0] = args[0].Substring(7);

            // Validate args
            if (args.Length != 1)
            {
                WaitAndExit("Expected 1 argument. Got {0}.\nUsage:\ndotnet SpotifyDownloader.dll https://open.spotify.com/track/ID", args.Length); return;
            }

            // Get type
            Regex reg = new Regex("https:\\/\\/open\\.spotify\\.com\\/([a-zA-Z]+)\\/[a-zA-Z0-9]+");
            var match = reg.Match(args[0]);
            if (!match.Success)
            {
                WaitAndExit("Incorrect Spotify URL.");  return;
            }

            // Download
            stopEvent = new ManualResetEvent(false);

            var spotdl = new SpotDL();
            switch (match.Groups[1].Value)
            {
                case "track":
                    Console.WriteLine("Downloading Track {0} to {1} with extension {2}...", args[0], directory, extension);
                    spotdl.DownloadTrack(args[0], directory, extension);
                    break;
                case "album":
                    Console.WriteLine("Downloading Album {0} to {1} with extension {2}...", args[0], directory, extension);
                    spotdl.DownloadAlbum(args[0], directory, extension);
                    break;
                case "playlist":
                    Console.WriteLine("Downloading Playlist {0} to {1} with extension {2}", args[0], directory, extension);
                    spotdl.DownloadPlaylist(args[0], directory, extension);
                    break;
                default:
                    Console.WriteLine("Type {0} not supported!", match.Groups[1].Value);
                    break;
            }

            // Wait for exit...
            Console.WriteLine("Done downloading!");
            Thread.Sleep(500);
            //Console.ReadKey();
            //WaitAndExit("Done downloading!");
            //stopEvent.WaitOne();
        }
        
        #region Config

        static void LoadConfig()
        {
            string appsettingsPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "appsettings.json");
            string userAppsettingsPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "appsettings.user.json");

            config = JObject.Parse(File.ReadAllText(appsettingsPath));
            if (File.Exists(userAppsettingsPath))
            {
                JObject userConfig = JObject.Parse(File.ReadAllText(userAppsettingsPath));
                config.Merge(userConfig);
            }
        }

        static void Configure()
        {
            Console.WriteLine("Configure SpotifyDownloader. Type 'exit' at any time to stop early.");

            /* Folder */
            Console.WriteLine("Download folder (blank for 'My Music'):");
            var path = Console.ReadLine();
            if (path == "exit") return;
            if (!string.IsNullOrWhiteSpace(path))
            {
                config["output"]["directory"] = path;
            }
            else
            {
                Console.WriteLine("Defaulting to 'My Music'.");
                config["output"]["directory"] = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            }

            /* Extension */
            Console.WriteLine("File extension (blank for '.m4a'):");
            Console.WriteLine("Supported: .mp3, .m4a, .flac");
            var ext = Console.ReadLine();
            if (ext == "exit") { SaveConfig(); return; }
            if (string.IsNullOrWhiteSpace(ext)) ext = ".m4a";
            config["output"]["extension"] = ext;
            
            ///* Client ID */
            //Console.WriteLine("Client ID:");
            //var clientId = Console.ReadLine();
            //if (clientId == "exit") { SaveConfig(); return; }
            //config["id"]["clientId"] = clientId;

            ///* Secret ID */
            //Console.WriteLine("Secret ID:");
            //var secretId = Console.ReadLine();
            //if (secretId == "exit") { SaveConfig(); return; }
            //config["id"]["secretId"] = secretId;

            /* Save */
            SaveConfig();
        }

        static async void SaveConfig()
        {
            string path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "appsettings.user.json");
            await File.WriteAllTextAsync(path, config.ToString(Formatting.Indented));
        }

        #endregion

        #region Input

        static int ReadInt(int? defaultValue = null, int? min = null, int? max = null)
        {
            int? value = null;
            while (!value.HasValue)
            {
                var v = Console.ReadLine();
                if (Int32.TryParse(v, out int i))
                {
                    if (min.HasValue && i < min) Console.WriteLine("Smaller than {0}. Try again...", min.Value);
                    else if (max.HasValue && i > max) Console.WriteLine("Bigger than {0}. Try again...", max.Value);
                    else value = i;

                }
                else if (defaultValue.HasValue) value = defaultValue;
                else Console.WriteLine("NaN. Try again...");
            }
            return value.Value;
        }

        static int[] ParseIds(string ids)
        {
            var split = ids.Split(' ');
            var indices = new HashSet<int>();
            foreach (var id in split)
            {
                var i = id.IndexOf('-');
                if (i > 0)
                {
                    if (int.TryParse(id.Substring(0, i), out int a) && int.TryParse(id.Substring(i + 1), out int b))
                    {
                        for (int index = a; index <= b; index++) indices.Add(index);
                    }
                }
                else if (int.TryParse(id, out int num)) indices.Add(num);
            }

            return indices.ToArray();
        }

        #endregion

        static void Download(ICollection<string> ids, string outFolder)
        {
            // Create temp file.
            var tempFile = Path.GetTempFileName() + ".txt";
            var dump = "";
            foreach (var id in ids)
            {
                dump += "https://open.spotify.com/track/" + id + Environment.NewLine;
            }
            File.WriteAllText(tempFile, dump);

            // Download
            Process process = new Process();
            process.StartInfo.FileName = "spotdl";
            process.StartInfo.Arguments = $"--list \"{tempFile}\" --overwrite force -o .m4a -f \"{outFolder}\"";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;// Do not use OS shell
            process.StartInfo.CreateNoWindow = true; // We don't need new window
            process.StartInfo.RedirectStandardOutput = true;// Any output, generated by application will be redirected back
            process.StartInfo.RedirectStandardError = true; // Any error in standard output will be redirected back (for example exceptions)

            process.EnableRaisingEvents = true;
            process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            process.ErrorDataReceived += (sender, e) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(e.Data);
                Console.ResetColor();
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            
            // Remove temp file.
            File.Delete(tempFile);
        }

        /* Shows a message and waits for a key before exiting. */
        static void WaitAndExit(string message, params object[] args)
        {
            Console.WriteLine(message, args);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
