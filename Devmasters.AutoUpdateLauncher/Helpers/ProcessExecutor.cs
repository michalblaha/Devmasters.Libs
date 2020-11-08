using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;


namespace Devmasters.AutoUpdateLauncher.Helpers
{
    public class ProcessExecutor
    {
        ProcessStartInfo processInfo = null;
        string outputLogFile = string.Empty;
        string errorLogFile = string.Empty;
        int timeOut = -1;
        //        System.Diagnostics.Process process;
        bool log = false;
        //        bool finishedProcess = false;
        int exitCode = int.MinValue;

        StringBuilder sbOut = new StringBuilder();
        StringBuilder sbErr = new StringBuilder();


        public ProcessExecutor(ProcessStartInfo processInfo)
            : this(processInfo, -1, string.Empty, string.Empty, false)
        {
        }
        public ProcessExecutor(ProcessStartInfo processInfo, int timeOutInSec)
                : this(processInfo, timeOutInSec, string.Empty, string.Empty, false)
        { }


        public ProcessExecutor(ProcessStartInfo processInfo, int timeOutInSec, string outputLogFile, string errorLogFile, bool logAll)
        {
            this.timeOut = timeOutInSec * 1000;
            this.processInfo = processInfo;
            this.outputLogFile = outputLogFile;
            this.errorLogFile = errorLogFile;

            log = logAll;

        }

        void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            sbErr.AppendLine(e.Data);
        }

        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            sbOut.AppendLine(e.Data);
        }

        void process_Exited(object sender, EventArgs e)
        {
            //            finishedProcess = true;
        }


        public string StandartOutput
        {
            get
            {
                //return process.StandardOutput.ReadToEnd();
                return sbOut.ToString();
            }
        }


        public string ErrorOutput
        {
            get
            {
                //return process.StandardOutput.ReadToEnd();
                return sbErr.ToString();
            }
        }

        public string PathWithArguments
        {
            get
            {
                return processInfo.FileName + " " + processInfo.Arguments;
            }
        }

        public int ExitCode
        {
            get
            {
                return exitCode;
            }
        }

        public void SetStandartProcessInfoParams()
        {
            processInfo.CreateNoWindow = false;
            processInfo.RedirectStandardOutput = false;
            processInfo.RedirectStandardError = true;
            processInfo.UseShellExecute = false;

        }
        public void Start()
        {

            using (Process process = new Process())
            {
                process.StartInfo = processInfo;
                process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
                process.ErrorDataReceived += new DataReceivedEventHandler(process_ErrorDataReceived);
                process.Exited += new EventHandler(process_Exited);


                //                finishedProcess = false;
                process.Start();

                if (processInfo.RedirectStandardError)
                    process.BeginErrorReadLine();
                
                if (processInfo.RedirectStandardOutput)
                    process.BeginOutputReadLine();

                if (timeOut < 0)
                    timeOut = -1;
                bool finishedOK = process.WaitForExit(timeOut);
                if (!finishedOK)
                {
                    process.Kill();
                    process.WaitForExit(1000); //wait 1 sec for end
                }
                if (process.ExitCode != 0 && finishedOK == false)
                {
                    //string err = process.StandardError.ReadToEnd();
                }
                exitCode = process.ExitCode;

            }
        }



    }
}
