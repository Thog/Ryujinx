using Ryujinx.Common.Logging;
using Ryujinx.Ui;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Threading;

namespace Ryujinx
{
    class Updater
    {
        public static string ryuDir = Environment.CurrentDirectory;

        public static void DeleteBackupFiles()
        {
            string updateTemp = Path.Combine(ryuDir, "Update_Temp");

            if (Directory.Exists(updateTemp))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("* Deleting Remaining Update Files...");
                Console.ForegroundColor = ConsoleColor.White;

                Directory.Delete(updateTemp, true);
            }

            foreach (string file in Directory.GetFiles(ryuDir, "*", SearchOption.AllDirectories))
            {
                if (Path.GetExtension(file) == ".ryubak")
                {
                    File.Delete(file);
                }
            }

            foreach (string directory in Directory.GetDirectories(ryuDir, "*", SearchOption.AllDirectories))
            {
                if (directory.EndsWith(".ryubak"))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        public static void BackupAllFiles()
        {
            string currentMoved = "";

            //Backup Files

            foreach (string file in Directory.GetFiles(ryuDir, "*", SearchOption.AllDirectories))
            {
                try
                {
                    if (Path.GetExtension(file) != ".ryubak")
                    {
                        currentMoved = file;
                        File.Move(file, file + ".ryubak");
                    }
                }
                catch
                {
                    
                }
            }

            //Backup Directories

            foreach (string directory in Directory.GetDirectories(ryuDir, "*", SearchOption.AllDirectories))
            {
                try
                {
                    if (!directory.EndsWith(".ryubak"))
                    {
                        currentMoved = directory;
                        Directory.Move(directory, directory + ".ryubak");
                    }
                }
                catch
                {
                    
                }
            }
        }

        private static void MoveAllFilesOver(string root, string dest)
        {
            foreach (var directory in Directory.GetDirectories(root))
            {
                string dirName = Path.GetFileName(directory);

                if (!Directory.Exists(Path.Combine(dest, dirName)))
                {
                    Directory.CreateDirectory(Path.Combine(dest, dirName));
                }

                MoveAllFilesOver(directory, Path.Combine(dest, dirName));
            }

            foreach (var file in Directory.GetFiles(root))
            {
                File.Move(file, Path.Combine(dest, Path.GetFileName(file)), true);
            }
        }

        public static void DownloadAndExtractUpdate(string downloadUrl, string shaDownloadURL)
        {
            string updateTemp = Path.Combine(ryuDir, "Update_Temp");

            if (!Directory.Exists(updateTemp))
            {
                Directory.CreateDirectory(updateTemp);
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("* Downloading Update...");

            //Download the update zip and the SHA256

            string updateFile = Path.Combine(updateTemp, "update.bin");
            string shaFile = Path.Combine(updateTemp, "update.sha");

            using (WebClient client = new WebClient())
            {
                client.DownloadFile(downloadUrl, updateFile);
                client.DownloadFile(shaDownloadURL, shaFile);
            }

            //Detect if SHA256 matches

            SHA256 mySHA256 = SHA256Managed.Create();

            using (FileStream filestream = new FileStream(updateFile, FileMode.Open))
            {
                filestream.Position = 0;

                if (File.ReadAllLines(shaFile)[0].Split(' ')[1] != BitConverter.ToString(mySHA256.ComputeHash(filestream)).Replace("-", String.Empty))
                {
                    Directory.Delete(updateTemp, true);
                    GtkDialog.CreateErrorDialog($"The SHA256 did not match!");
                    DeleteBackupFiles();

                    return;
                }
            }

            //Extract Update

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("* Extracting Update...");

            string extractPath = Path.Combine(updateTemp, "extract");

            if (!Directory.Exists(extractPath))
            {
                Directory.CreateDirectory(extractPath);
            }

            ZipFile.ExtractToDirectory(updateFile, extractPath);

            //Move update to main folder

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("* Replacing Old Files...");

            MoveAllFilesOver(extractPath, ryuDir);

            //Restart Ryujinx

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Done!" + Environment.NewLine + Environment.NewLine + "Restarting Ryujinx in 5 seconds!");

            Thread.Sleep(5000);

            Process.Start(Process.GetCurrentProcess().MainModule.FileName);
            Environment.Exit(0);
        }

        public static void UpdateRyujinx(string downloadUrl, string shaDownloadURL)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("* Creating File Backups...");

            BackupAllFiles();

            DownloadAndExtractUpdate(downloadUrl, shaDownloadURL);
        }
    }

}