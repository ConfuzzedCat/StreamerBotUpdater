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
            if(String.Compare(latestVersion, sbVersion) == 0)
            {
                Console.WriteLine($"Streamer.Bot is already the latest version: {sbVersion}");
                Environment.Exit(0);
            }
            Console.WriteLine($"Older version found: {sbVersion}. The latest version is {latestVersion}!");
            Console.WriteLine("Backing up old files...");
            string backupPath = Path.Combine(path, "OldVer_Backup");
            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }
            else
            {
                Directory.Delete(backupPath, true);
            }
            CopyFilesByExtension(path, backupPath, "*.dll", programDlls);
            File.Copy("streamer.bot.png", Path.Combine(backupPath, "streamer.bot.png"), false);
            File.Copy("streamer.bot.pdb", Path.Combine(backupPath, "streamer.bot.pdb"), false);
            File.Copy("streamer.bot.exe.config", Path.Combine(backupPath, "streamer.bot.exe.config"), false);
            File.Copy("streamer.bot.exe", Path.Combine(backupPath, "streamer.bot.exe"), false);            
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
                        ZipFile.ExtractToDirectory(Path.Combine(_path, $"Streamer.bot-{latestVersion}.zip"), path, true);
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
                }
            }            
            Console.ReadKey();
        }
        //static private void CopyFilesByExtension(string path, string destPath, string fileExtension, bool ow = true)
        //{
        //    string[] files = Directory.GetFiles(path, fileExtension);
        //    foreach (string item in files)
        //    {
        //        File.Copy(item, Path.Combine(destPath, item.Remove(0, item.LastIndexOf('\\') + 1)), ow);
        //    }
        //}
        static private void CopyFilesByExtension(string path, string destPath, string fileExtension, string[] exclude, bool ow = true)
        {
            string[] files = Directory.GetFiles(path, fileExtension);
            foreach (string item in files)
            {
                if(!exclude.Any(item.Contains))
                {
                    File.Copy(item, Path.Combine(destPath, item.Remove(0, item.LastIndexOf('\\') + 1)), ow);
                }
                
            }
        }
    }
}


