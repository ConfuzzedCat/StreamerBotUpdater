using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace StreamerBotUpdater
{
    class Program
    {
        static async Task Main()
        {
            Console.Title = "Streamer.Bot Updater";
            bool isCurDirDownloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).ToString(), "Downloads") == Directory.GetCurrentDirectory();
            if (isCurDirDownloads)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: Don't have your Streamer.Bot in the Downloads folder... Updater will close!");
                Console.ReadLine();
                Environment.Exit(0);
            }
            Console.ForegroundColor = ConsoleColor.White;
            Process process = null;
            bool sbRunning = Process.GetProcessesByName("Streamer.bot").Length > 0;
            if (sbRunning)
            {
                process = Process.GetProcessesByName("Streamer.bot")[0];
            }
            string path = Directory.GetCurrentDirectory();
            Console.WriteLine("Welcome!");
            GitHubClient client = new GitHubClient(new ProductHeaderValue("StreamerBotUpdater"));
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("Streamerbot", "Streamer.bot");
            string latestVersion = releases[0].TagName.Remove(0, 1);
            string link = $"https://github.com/Streamerbot/Streamer.bot/releases/latest/download/Streamer.bot-{latestVersion}.zip";
            string sbVersion = FileVersionInfo.GetVersionInfo(Path.Combine(path, "Streamer.bot.exe")).FileVersion.Remove(5);
            string[] programDlls = new string[]
            {
                Directory.GetFiles(path, "clrcompression.dll")[0],
                Directory.GetFiles(path, "clrjit.dll")[0],
                Directory.GetFiles(path, "coreclr.dll")[0],
                Directory.GetFiles(path, "mscordaccore.dll")[0],
            };
            string[] sbDlls = new string[]
            {
                Path.Combine(path, "System.ValueTuple.dll"),
                Path.Combine(path, "System.Threading.Tasks.Extensions.dll"),
                Path.Combine(path, "System.Threading.Channels.dll"),
                Path.Combine(path, "System.Text.Encoding.CodePages.dll"),
                Path.Combine(path, "System.Security.Principal.Windows.dll"),
                Path.Combine(path, "System.Security.AccessControl.dll"),
                Path.Combine(path, "System.Runtime.CompilerServices.Unsafe.dll"),
                Path.Combine(path, "System.Reflection.Metadata.dll"),
                Path.Combine(path, "System.Numerics.Vectors.dll"),
                Path.Combine(path, "System.Memory.dll"),
                Path.Combine(path, "System.Collections.Immutable.dll"),
                Path.Combine(path, "System.Buffers.dll"),
                Path.Combine(path, "streamer.bot.png"),
                Path.Combine(path, "Streamer.bot.pdb"),
                Path.Combine(path, "Streamer.bot.exe.config"),
                Path.Combine(path, "Streamer.bot.exe"),
                Path.Combine(path, "Mono.Cecil.Rocks.dll"),
                Path.Combine(path, "Mono.Cecil.Pdb.dll"),
                Path.Combine(path, "Mono.Cecil.Mdb.dll"),
                Path.Combine(path, "Mono.Cecil.dll"),
                Path.Combine(path, "Microsoft.Win32.Registry.dll"),
                Path.Combine(path, "Microsoft.Extensions.DependencyInjection.dll"),
                Path.Combine(path, "Microsoft.Extensions.DependencyInjection.Abstractions.dll"),
                Path.Combine(path, "Microsoft.CodeAnalysis.dll"),
                Path.Combine(path, "Microsoft.CodeAnalysis.CSharp.dll"),
                Path.Combine(path, "Microsoft.Bcl.AsyncInterfaces.dll"),
            };
            if (String.Compare(latestVersion, sbVersion) == 0)
            {
                Console.WriteLine($"Streamer.Bot is already the latest version: {sbVersion}");
                Environment.Exit(0);
            }
            if (sbRunning && process != null)
            {
                Console.WriteLine("Streamer.Bot is running... The updater will wait till it's close with continuing!");
                process.WaitForExit();
            }
            Console.WriteLine($"Older version found: {sbVersion}. The latest version is {latestVersion}!");
            Console.WriteLine("Downloading new version and getting what files to backup.");
            using (WebClient wc = new WebClient())
            {
                try
                {
                    Console.WriteLine("Downloading latest zip file.");
                    string _path = Path.Combine(path, "_tmp");
                    Directory.CreateDirectory(_path);
                    wc.DownloadFile(link, Path.Combine(_path, $"Streamer.bot-{latestVersion}.zip"));
                    if (File.Exists(Path.Combine(_path, $"Streamer.bot-{latestVersion}.zip")))
                    {

                        ZipFile.ExtractToDirectory(Path.Combine(_path, $"Streamer.bot-{latestVersion}.zip"), _path, true);
                        File.Delete(Path.Combine(_path, $"Streamer.bot-{latestVersion}.zip"));
                        string[] sbNewFiles = Directory.GetFiles(_path);
                        Console.WriteLine("Backing up old files...");
                        string backupPath = Path.Combine(path, "OldVer_Backup");
                        if (!Directory.Exists(backupPath))
                        {
                            Directory.CreateDirectory(backupPath);
                        }
                        else
                        {
                            Directory.Delete(backupPath, true);
                            Directory.CreateDirectory(backupPath);
                        }
                        List<string> oldFilesList = new List<string>();
                        foreach(string file in sbNewFiles)
                        {
                            oldFilesList.Add(Path.Combine(path, file.Remove(0, file.LastIndexOf('\\') + 1)));
                        }
                        CopyFiles(backupPath, oldFilesList.ToArray());
                        CopyFiles(path, sbNewFiles);
                        sbVersion = FileVersionInfo.GetVersionInfo(Path.Combine(path, "Streamer.bot.exe")).FileVersion.Remove(5);
                        if (String.Compare(latestVersion, sbVersion) == 0)
                        {
                            Console.WriteLine($"Streamer.Bot is now the latest version: {sbVersion}");
                            Directory.Delete(_path, true);
                            Environment.Exit(0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }
            
            
        }
        static private void CopyFiles(string destPath, string[] files, bool ow = true)
        {
            foreach (string item in files)
            {
                if (File.Exists(item))
                {
                    File.Copy(item, Path.Combine(destPath, item.Remove(0, item.LastIndexOf('\\') + 1)), ow);
                }
                
            }
        }
    }
}


