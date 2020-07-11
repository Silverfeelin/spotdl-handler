using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SpotifyDownloader
{
    public class SpotDl
    {
        public class Options
        {
            public string SongUrl { get; set; }
            public string List { get; set; }
            public string AlbumUrl { get; set; }
            public string PlaylistUrl { get; set; }
            public string WriteTo { get; set; }
            public bool NoMetadata { get; set; }
            public string Directory { get; set; }
            public string FileFormat { get; set; }
            public string Extension { get; set; } = ".m4a";
            public bool Manual { get; set; }
            public string LogLevel { get; set; } = "INFO";
            public string Overwrite { get; set; } = "prompt";

            public Options Clone()
            {
                return (Options)MemberwiseClone();
            }
        }

        public void Download(Options options)
        {
            // Song or list can be downloaded directly
            if (options.SongUrl != null || options.List != null)
            {
                Call(options);
                return;
            }
            // Playlists and albums require 2 steps.
            options = options.Clone();
            
            // Download track info.
            var file = Path.Combine(options.Directory, "spotdl_songs.txt");
            File.Delete(file);
            options.WriteTo = file;
            Call(options);
            
            // Download track list.
            options.PlaylistUrl = null;
            options.AlbumUrl = null;
            options.WriteTo = null;
            options.List = file;
            Call(options);
            File.Delete(file);
        }

        private void Call(Options options)
        {
            using (var process = CreateProcess())
            {
                process.StartInfo.Arguments = BuildArguments(options);
                RunProcess(process);
            }
        }

        public static string BuildArguments(Options options)
        {
            var sb = new StringBuilder();

            void AppendIf(string k, string v) { if (v != null) sb.Append($"{k} {v} "); }
            void AppendTextIf(string k, string v) { if (v != null) sb.Append($"{k} \"{v}\" "); }
            void AppendBoolIf(string k, bool b) { if (b) sb.Append($"{k} "); }

            AppendIf("-s", options.SongUrl);
            AppendIf("-a", options.AlbumUrl);
            AppendIf("-p", options.PlaylistUrl);
            AppendIf("-l", options.List);

            AppendIf("-o", options.Extension);
            AppendIf("--overwrite", options.Overwrite);
            AppendIf("-ll", options.LogLevel);

            AppendTextIf("--write-to", options.WriteTo);
            AppendTextIf("-f", options.Directory);
            AppendTextIf("--ff", options.FileFormat);

            AppendBoolIf("-m", options.Manual);
            AppendBoolIf("-nm", options.NoMetadata);

            return sb.ToString();
        }

        #region Process

        /* Creates process for spotdl without args. */
        private Process CreateProcess()
        {
            return new Process
            {
                StartInfo =
                {
                    FileName = "spotdl",
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    //RedirectStandardInput = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                },
                EnableRaisingEvents = true
            };
        }

        /* Runs the process and waits for it to finish. */
        private void RunProcess(Process process)
        {
            ConsoleExtensions.WriteLine(ConsoleColor.DarkCyan,
                "{0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments ?? string.Empty);
            process.Start();
            process.WaitForExit();
        }
        
        #endregion
    }
}
