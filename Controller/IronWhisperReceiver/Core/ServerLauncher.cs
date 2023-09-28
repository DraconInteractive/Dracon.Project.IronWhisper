using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Core
{
    public class ServerLauncher
    {
        public static ServerLauncher Instance;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);

        delegate bool ConsoleCtrlDelegate(int dwCtrlType);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

        public Process process;

        public ServerLauncher()
        {
            Instance = this;
        }

        public void Launch()
        {
            process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                UseShellExecute = true,
                CreateNoWindow = false,
                Verb = "runas"
            };
            process.StartInfo = startInfo;
            process.Start();

            // Assuming the C# app has admin permissions, the cmd should also have them
            using (StreamWriter sw = process.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine("wsl");

                    sw.WriteLine("cd /mnt/C/Users/pmc10/Documents/IronWhisper");

                    sw.WriteLine("./command -t 8");
                }
            }
        }

        public bool Close()
        {
            if (process != null)
            {
                if (AttachConsole((uint)process.Id))
                {
                    GenerateConsoleCtrlEvent(0, 0);
                    FreeConsole();
                    process = null;
                    return true;
                }
                else
                {
                    Console.WriteLine("Unable to attach console, cannot close server terminal");
                    return false;
                }
            }
            return true;
        }
    }
}
