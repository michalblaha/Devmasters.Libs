using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devmasters.AutoUpdateLauncher.Helpers
{
    public static class Util
    {
        public static string GetConfigValue(string value)
        {
            string @out = System.Configuration.ConfigurationManager.AppSettings[value];
            if (@out == null)
            {
                @out = string.Empty;
            }
            return @out;
        }

        public static string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }


        public enum CopyResult
        {
            Copied,
            Newer,
            Skipped,
            Error,
        }
        public static CopyResult MyCopy(string fromFn, string toFn, bool onlyNewer = true)
        {
            try
            {

                if (!System.IO.File.Exists(fromFn))
                    return CopyResult.Error;

                string targetDir = Path.GetDirectoryName(toFn);
                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                bool copy = true;
                if (onlyNewer)
                {
                    DateTime sourceLastChange = new System.IO.FileInfo(fromFn).LastWriteTimeUtc;
                    if (File.Exists(toFn))
                    {
                        DateTime targetLastChange = new System.IO.FileInfo(toFn).LastWriteTimeUtc;
                        if (sourceLastChange <= targetLastChange)
                            copy = false;
                    }
                }

                if (copy)
                {
                    try
                    {
                        System.IO.File.Copy(fromFn, toFn, true);
                    }
                    catch (Exception)
                    {
                        System.Threading.Thread.Sleep(500);
                        try
                        {
                            System.IO.File.Copy(fromFn, toFn, true);
                        }
                        catch (Exception)
                        {
                            System.Threading.Thread.Sleep(1000);
                            try
                            {
                                System.IO.File.Copy(fromFn, toFn, true);
                            }
                            catch (Exception)
                            {
                                return CopyResult.Error;
                            }
                        }
                    }
                    if (onlyNewer)
                        return CopyResult.Newer;
                    else
                        return CopyResult.Copied;
                }
                else
                    return CopyResult.Skipped;
            }
            catch (Exception e)
            {
                Devmasters.AutoUpdateLauncher.Program.Logger.Error("MyCopy error", e);
                return CopyResult.Error;
            }
        }


        public static bool IsProcessRunning(string filename)
        {
            try
            {
                string onlyname = System.IO.Path.GetFileNameWithoutExtension(filename);
                Process currProc = Process.GetCurrentProcess();
                if (currProc == null)
                {
                    Program.Logger.Debug("Process.GetCurrentProcess() is null");
                    return true;
                }
                string currFn = currProc?.MainModule?.FileName ?? onlyname;
                Process[] ps = Process.GetProcessesByName(onlyname).ToArray();

                foreach (var p in ps)
                {
                    try
                    {
                        if (p.Id != currProc.Id)
                        {
                            if ((p?.MainModule?.FileName ?? onlyname) == currFn)
                                return true;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Root.Warning("Launcher IsProcessRunning error", e);

                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Logger.Root.Error("Launcher IsProcessRunning error", e);
                return false;
            }
        }

    }
}
