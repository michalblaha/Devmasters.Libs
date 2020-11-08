using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Diagnostics;


namespace Devmasters.AutoUpdateLauncher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            Logger.Debug("Starting Launcher.exe. " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Console.WriteLine("Starting Launcher.exe " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

            lauchArguments = args; 
            launch_Filename = Helpers.Util.GetConfigValue("Launch.Filename");
            launch_UpdateLocation = Helpers.Util.GetConfigValue("Launch.Update.Location");
            if (!launch_UpdateLocation.EndsWith("\\"))
                launch_UpdateLocation += "\\";

            isPathUpdateType = !(launch_UpdateLocation.StartsWith("http"));
            currentDir = AppDomain.CurrentDomain.BaseDirectory;

            remoteFullAppName = System.IO.Path.Combine(launch_UpdateLocation, launch_Filename);
            localFullAppName = System.IO.Path.Combine(currentDir, launch_Filename);
            excludeWildcards = Helpers.Util.GetConfigValue("Launch.Update.Exclude").Split(new char[] { ';' },  StringSplitOptions.RemoveEmptyEntries);

            if (isPathUpdateType)
            {
                var res = DoPathUpdate();
                switch (res)
                {
                    case UpdateStatus.NoAvailableUpdate:
                        Console.WriteLine(" No new update.");

                        break;
                    case UpdateStatus.Success:
                        Console.WriteLine(" Application update was successfull.");
                        break;
                    case UpdateStatus.Canceled:
                        Logger.Error("Application update was canceled");
                        Console.WriteLine(" Application update was canceled.");
                        break;
                    case UpdateStatus.AnotherInstance:
                        Console.WriteLine(" Another instance running.");
                        break;
                    default:
                        break;
                }

                StartApp();
            }
            else
            {
                throw new NotImplementedException();
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        static string[] lauchArguments;
        static string currentDir;
        static string launch_Filename;
        static string launch_UpdateLocation;
        static bool isPathUpdateType;
        public static Helpers.Logger Logger = new Helpers.Logger("Devmasters.AppAutoUpdater");
        static string remoteFullAppName;
        static string localFullAppName;
        static string[] excludeWildcards;

        public enum UpdateStatus
        {
            NoAvailableUpdate,
            Success,
            Canceled,
            AnotherInstance
        }
        static UpdateStatus DoPathUpdate()
        {
            string remoteFullAppName = System.IO.Path.Combine(launch_UpdateLocation, launch_Filename);
            string localFullAppName = System.IO.Path.Combine(currentDir, launch_Filename);

            if (Helpers.Util.IsProcessRunning(localFullAppName))
            {
                Console.WriteLine($"Another instance of app {System.IO.Path.GetFileName(localFullAppName)} is running.");

                Logger.Error($"Another instance of app {System.IO.Path.GetFileName(localFullAppName)} is running. Update is canceled");
                return UpdateStatus.AnotherInstance;
            }
            Version remoteVersion = null;
            Version localVersion = null;

            if (!System.IO.Directory.Exists(launch_UpdateLocation))
            {
                Console.WriteLine("Remote Update location " + launch_UpdateLocation + " doesn't exists.");
                Logger.Error("Remote Update location " + launch_UpdateLocation + " doesn't exists. Update is canceled");
                return UpdateStatus.NoAvailableUpdate;
            }
            if (!System.IO.File.Exists(remoteFullAppName))
            {
                Console.WriteLine("Remote Application to update " + launch_UpdateLocation + " doesn't exists.");
                Logger.Error("Remote Application to update " + launch_UpdateLocation + " doesn't exists. Update is canceled");
                return UpdateStatus.NoAvailableUpdate;

            }
            else
            {
                FileVersionInfo remoteInfo = FileVersionInfo.GetVersionInfo(remoteFullAppName);
                if (remoteInfo == null)
                {
                    Console.WriteLine("No Version info in remote Application  " + remoteFullAppName + ".");
                    Logger.Error("No Version info in remote Application  " + remoteFullAppName + ". Update is canceled");
                    return UpdateStatus.NoAvailableUpdate;
                }
                else
                    remoteVersion = new Version(remoteInfo.FileVersion);
            }
            if (!System.IO.File.Exists(localFullAppName))
            {
                localVersion = new Version(0, 0, 0, 0);
            }
            else
            {
                FileVersionInfo localInfo = FileVersionInfo.GetVersionInfo(localFullAppName);
                if (localInfo == null)
                {
                    Console.WriteLine("No Version info in local Application  " + localFullAppName + ".");
                    Logger.Error("No Version info in local Application  " + localFullAppName + ". Update is canceled");
                    localVersion = new Version(0, 0, 0, 0);
                }
                else
                    localVersion = new Version(localInfo.FileVersion);

            }



            bool isThereUpdate = remoteVersion > localVersion;
            if (!isThereUpdate)
            {
                Console.WriteLine("No update available in " + remoteFullAppName);
                Logger.Info("No update available in " + remoteFullAppName);
                return UpdateStatus.NoAvailableUpdate; 
            }

            //prepare files to copy
            string[] allFiles = System.IO.Directory.GetFiles(launch_UpdateLocation, "*.*", System.IO.SearchOption.AllDirectories);
            List<string> filesToExclude = new List<string>();
            foreach (var wildcardToExclude in excludeWildcards)
            {
                filesToExclude.AddRange(System.IO.Directory.GetFiles(launch_UpdateLocation, wildcardToExclude));
            }
            var filesToCopy = allFiles.Except(filesToExclude);

            //copy files
            Console.WriteLine("Update is available in " + remoteFullAppName + ". Version " + remoteVersion.ToString());
            Logger.Info("Update is available in " + remoteFullAppName + ". Version " + remoteVersion.ToString());
            foreach (var fn in filesToCopy)
            {
                string relativePath = Helpers.Util.GetRelativePath(fn, launch_UpdateLocation);
                string targetFn = System.IO.Path.Combine(currentDir, relativePath);
                var ok = Helpers.Util.MyCopy(fn, targetFn);
                switch (ok)
                {
                    case Helpers.Util.CopyResult.Copied:
                        Console.WriteLine("Copy of " + targetFn + " was successfull.");
                        Logger.Debug("Copy of " + targetFn + " was successfull.");
                        break;
                    case Helpers.Util.CopyResult.Newer:
                        Console.WriteLine("Copy of new version " + targetFn + " was successfull.");
                        Logger.Debug("Copy of new version " + targetFn + " was successfull.");
                        break;
                    case Helpers.Util.CopyResult.Skipped:
                        //Console.WriteLine("Skipped copy of " + targetFn + ".");
                        Logger.Debug("Skipped copy of " + targetFn + ".");
                        break;
                    case Helpers.Util.CopyResult.Error:
                        Console.WriteLine("Error during copy of " + targetFn);
                        Logger.Error("Error during copy of " + targetFn);
                        return UpdateStatus.Canceled;
                    default:
                        break;
                }
            }


            return UpdateStatus.Success;
        }


        static void StartApp()
        {

            //System.AppDomain appDom = System.AppDomain.CreateDomain("AppDomainForApp");
            //// Load the assembly and call the default entry point:
            //appDom.ExecuteAssembly(launch_Filename, appArguments);
            string arguments = "";
            if (lauchArguments != null && lauchArguments.Length > 0)
                arguments = lauchArguments
                        .Select(t=> "\"" + t + "\"")
                        .Aggregate((f, s) => f + " " + s );
            var pi = new ProcessStartInfo(launch_Filename, arguments);
            pi.WorkingDirectory = currentDir;
            Helpers.ProcessExecutor pe = new Helpers.ProcessExecutor(pi);
            pe.SetStandartProcessInfoParams();

            Logger.Info("Starting " + launch_Filename + ".");

            pe.Start();

            Logger.Info(launch_Filename + " ended.");

        }
    }
}
