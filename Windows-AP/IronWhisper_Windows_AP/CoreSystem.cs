namespace IronWhisper_Windows_AP
{
    internal class CoreSystem
    {
        public static int Verbosity = 1;

        static async Task Main(string[] args)
        {
            Log("IW-Windows-AP v0.0.1a\n-------------------------------------------\n");

            Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);


            var server = new TCPServer();
            await server.Start();
            Log("\n Closing IW-Windows-AP v0.0.1a");

        }

        public static void Log(int verbosity = 0)
        {
            Log("", verbosity);
        }

        public static void Log(string message, int verbosity = 0, bool writeLine = true)
        {
            if (Verbosity >= verbosity)
            {
                Console.Write(message + (writeLine ? "\n" : ""));
            }
        }

        protected static void ExitHandler(object sender, ConsoleCancelEventArgs args)
        {
            Log("Ctrl-C pressed. Exiting");
            args.Cancel = true;

            if (TCPServer.Instance != null)
            {
                TCPServer.Instance.Stop();
            }

            Environment.Exit(0);
        }
    }
}