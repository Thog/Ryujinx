using Ryujinx.Common.Logging;
using Ryujinx.Ui;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Ryujinx
{
    public class UpdateParser
    {
        private static string currentPlatformDownloadUrl;
        private static string currentPlatformDownloadSHA;

        public static string RyuDir = Environment.CurrentDirectory;

        public static async void BeginParse()
        {
            string newVersionNumber = "";

            // Get latest version number from Github

            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync("https://github.com/Thog/temp_test/releases/latest/");
                response.EnsureSuccessStatusCode();

                newVersionNumber = response.RequestMessage.RequestUri.ToString().Split('/').Last();
            }
            catch (Exception ex)
            {
                Logger.PrintError(LogClass.Application, ex.Message);
                GtkDialog.CreateErrorDialog($"An error has occured when trying to get release information from GitHub.");

                return;
            }

            // Detect current platform

            if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    string url = $"https://github.com/Thog/temp_test/releases/download/{newVersionNumber}/Ryujinx-Release-{newVersionNumber}-osx-x64";
                    currentPlatformDownloadUrl = url + ".zip";
                    currentPlatformDownloadSHA = url + ".sha256";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string url = $"https://github.com/Thog/temp_test/releases/download/{newVersionNumber}/Ryujinx-Release-{newVersionNumber}-win-x64";
                    currentPlatformDownloadUrl = url + ".zip";
                    currentPlatformDownloadSHA = url + ".sha256";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    string url = $"https://github.com/Thog/temp_test/releases/download/{newVersionNumber}/Ryujinx-Release-{newVersionNumber}-linux-x64";
                    currentPlatformDownloadUrl = url + ".zip";
                    currentPlatformDownloadSHA = url + ".sha256";
                }
            }
            else
            {
                Logger.PrintError(LogClass.Application, $"You are using an operating system architecture ({RuntimeInformation.ProcessArchitecture.ToString()}) not compatible with Ryujinx.");
                GtkDialog.CreateErrorDialog($"You are using an operating system architecture ({RuntimeInformation.ProcessArchitecture.ToString()}) not compatible with Ryujinx.");

                return;
            }

            // Get Version from app.config to compare versions

            Version newVersion = Version.Parse("0.0");
            Version currentVersion = Version.Parse("0.0");

            try
            {
                newVersion = Version.Parse(newVersionNumber);
                currentVersion = Version.Parse(Program.Version);
            } catch
            {
                Logger.PrintWarning(LogClass.Application, "Failed to convert current Ryujinx version.");
            }

            if (newVersion < currentVersion)
            {
                GtkDialog.CreateDialog("Ryujinx - Updater", "You are already using the most updated version of Ryujinx!", "");

                return;
            }

            // Show a message asking the user if they want to update

            if (GtkDialog.CreateChoiceDialog("Ryujinx - Updater", "Do you want to update Ryujinx to the latest version?", Program.Version + " -> " + newVersion.ToString()))
            {
                Program.mainWindow.Hide();
                Console.Clear();
                Updater.UpdateRyujinx(currentPlatformDownloadUrl, currentPlatformDownloadSHA);
            }
        }
    }

}