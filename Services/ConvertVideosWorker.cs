using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace VideoConverter_Streaming.Services
{
    public class ConvertVideosWorker : BackgroundService
    {
        private readonly ILogger<ConvertVideosWorker> _logger;
        private string InputVideoFolder { get; set; }
        private string ProcessedVideoFolder { get; set; }
        private string ConvertedVideoFolder { get; set; }

        public ConvertVideosWorker(ILogger<ConvertVideosWorker> logger, IConfiguration configuration)
        {
            _logger = logger;
            InputVideoFolder = configuration["InputVideoFolder"];
            ProcessedVideoFolder = configuration["ProcessedVideoFolder"];
            ConvertedVideoFolder = configuration["ConvertedVideoFolder"];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var _watcher = new FileSystemWatcher(InputVideoFolder);

            _watcher.Filter = "*.mp4";
            _watcher.EnableRaisingEvents = true;
            _watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;
            
            _watcher.Created += ConvertFileAndStream;

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Worker running at: {DateTimeOffset.Now}");
                await Task.Delay(-1, stoppingToken);
            }
        }

        private void ConvertFileAndStream(object sender, FileSystemEventArgs e)
        {
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var inputFileInfo = new FileInfo(e.FullPath); // Path to input MP4 file
            var inputFileName = Path.GetFileNameWithoutExtension(e.FullPath);
            
            //var inputFile = e.FullPath; 
            var outputFile = Path.Combine(ConvertedVideoFolder, "output.webm"); // Path to output WebM file

            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            // Run FFmpeg command to convert the video to WebM format
            var process = new Process();

            process.StartInfo.FileName = "ffmpeg";
            process.StartInfo.Arguments = $"-i {inputFileInfo.FullName} -c:v libvpx -c:a libvorbis -quality good -b:v 2M -b:a 128K {outputFile}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            // Wait for FFmpeg to finish converting the video
            process.WaitForExit();
            _logger.LogInformation($"File has been converted from {inputFileInfo.FullName} to {outputFile}");

            if (inputFileInfo.Directory != null)
            {
                var processedFile = Path.Combine(ProcessedVideoFolder, inputFileInfo.Name);
                if (File.Exists(processedFile))
                {
                    File.Delete(processedFile);
                }
                
                File.Move(inputFileInfo.FullName, Path.Combine(ProcessedVideoFolder, inputFileInfo.Name)); 
            }
        }
    }
}
