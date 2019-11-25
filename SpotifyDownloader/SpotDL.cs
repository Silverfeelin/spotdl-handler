using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            if (options.SongUrl != null || options.List != null)
            {
                Call(options);
                return;
            }

            options = options.Clone();
            
            var file = Path.Combine(options.Directory, "spotdl_songs.txt");
            File.Delete(file);
            options.WriteTo = file;
            Call(options);
            
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
            AppendIf("-b", options.AlbumUrl);
            AppendIf("-p", options.PlaylistUrl);
            AppendIf("-l", options.List);

            AppendIf("-o", options.Extension);
            AppendIf("--overwrite", options.Overwrite);
            AppendIf("-ll", options.LogLevel);

            AppendTextIf("--write-to", options.WriteTo);
            AppendTextIf("--folder", options.Directory);
            AppendTextIf("--ff", options.FileFormat);

            AppendBoolIf("-m", options.Manual);
            AppendBoolIf("-nm", options.NoMetadata);

            return sb.ToString();
        }

        #region Process

        /* Creates process for spotdl without args. */
        private Process CreateProcess()
        {
            var process = new Process
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
            process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            process.ErrorDataReceived += (sender, e) => Console.Error.WriteLine(e.Data);

            return process;
        }

        /* Runs the process and waits for it to finish. */
        private void RunProcess(Process process)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("{0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments ?? string.Empty);
            Console.ResetColor();
            process.Start();
            process.WaitForExit();
        }

        /* Runs the process asynchronously. */
        private async Task RunProcessAsync(Process process)
        {
            var sem = new SemaphoreSlim(0, 1);
            process.Start();
            process.Exited += (sender, args) => sem.Release();
            await sem.WaitAsync();
        }

        #endregion
    }
}
