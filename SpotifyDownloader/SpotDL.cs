using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SpotifyDownloader
{
    public class SpotDL
    {
        public SpotDL() {}

        /* Downloads a Track sycnhronously. */
        public void DownloadTrack(string url, string directory, string extension = ".m4a")
        {
            var process = CreateProcess();
            process.StartInfo.Arguments = $"-ll WARNING --song {url} -f \"{directory}\" -o {extension} --overwrite skip";
            RunProcess(process);
        }

        /* Downloads a Track asynchronously. */
        public async Task DownloadTrackAsync(string url, string directory, string extension = ".m4a")
        {
            var process = CreateProcess();
            process.StartInfo.Arguments = $"-ll WARNING --song {url} -f \"{directory}\" -o {extension} --overwrite skip";
            await RunProcessAsync(process);
        }

        public void DownloadAlbum(string url, string directory, string extension = ".m4a")
        {
            string file = null;
            using (var process = CreateProcess())
            {
                process.StartInfo.Arguments = $"-ll WARNING -b {url} -f \"{directory}\" --overwrite skip";
                RunProcess(process);
                file = FindRecentFile(directory);
            }

            DownloadFromFile(file, directory, extension);
            File.Delete(file);
        }

        public void DownloadPlaylist(string url, string directory, string extension = ".m4a")
        {
            string file = null;
            using (var process = CreateProcess())
            {
                process.StartInfo.Arguments = $"-ll WARNING -p {url} -f \"{directory}\" --overwrite skip";
                RunProcess(process);
                file = FindRecentFile(directory);
            }

            DownloadFromFile(file, directory, extension);
            File.Delete(file);
        }

        public void DownloadFromFile(string file, string directory, string extension = ".m4a")
        {
            if (!string.IsNullOrWhiteSpace(file) && File.Exists(file))
            {
                using (var process = CreateProcess())
                {
                    process.StartInfo.Arguments = $"-ll WARNING --list \"{file}\" -f \"{directory}\" -o {extension} --overwrite skip";
                    RunProcess(process);
                }
            }
        }

        #region Process

        /* Creates process for spotdl without args. */
        private Process CreateProcess()
        {
            Process process = new Process();
            process.StartInfo.FileName = "spotdl";
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.RedirectStandardError = false;
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            process.ErrorDataReceived += (sender, e) => Console.Error.WriteLine(e.Data);

            return process;
        }

        /* Runs the process and waits for it to finish. */
        private void RunProcess(Process process)
        {
            Console.WriteLine("{0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments ?? "");
            process.Start();
            process.WaitForExit();
        }

        /* Runs the process asynchronously. */
        private async Task RunProcessAsync(Process process)
        {
            SemaphoreSlim sem = new SemaphoreSlim(0, 1);
            process.Start();
            process.Exited += (sender, args) => sem.Release();
            await sem.WaitAsync();
        }

        #endregion

        #region Temp files

        private string GetAppFolder()
        {
            var appFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Console.WriteLine(appFolder);
            return appFolder;
        }
        
        private string FindRecentFile(string folder = null)
        {
            if (string.IsNullOrWhiteSpace(folder)) folder = GetAppFolder();
            var di = new DirectoryInfo(folder);
            // Get latest file. Should be created in last 10 seconds to prevent using wrong files if the script failed to create a file.
            var file = di.GetFiles().Where(f => f.Extension == ".txt" && f.CreationTime > DateTime.Now.AddSeconds(-10)).OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
            return file?.FullName;
        }

        #endregion
    }
}
