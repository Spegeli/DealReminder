using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using DealReminder_Linux.Configs;
using DealReminder_Linux.GUI;

namespace DealReminder_Linux.Logging
{
    public static class Logger
    {
        public static string CurrentFile;

        /// <summary>
        /// Set the logger. All future requests to <see cref="Write(string,LogLevel,ConsoleColor)"/> will use that logger, any old will be unset.
        /// </summary>
        public static void SetLogger()
        {
            if (!Directory.Exists(FoldersFilesAndPaths.Logs))
                Directory.CreateDirectory(FoldersFilesAndPaths.Logs);

            CurrentFile = Process.GetCurrentProcess().Id + " " + DateTime.Now.ToString("yyyy-MM-dd - HH.mm.ss");
            Write($"Initializing DealReminder (v{Updater.LocalVersion()}) logger @ {DateTime.Now}...");
        }

        /// <summary>
        ///     Log a specific message to the logger setup by <see cref="SetLogger()" /> .
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="level">Optional level to log. Default <see cref="LogLevel.Info" />.</param>
        /// <param name="color">Optional. Default is automatic color.</param>
        public static void Write(string message, LogLevel level = LogLevel.None, ConsoleColor color = ConsoleColor.White)
        {
            Console.OutputEncoding = Encoding.Unicode;

            var dateFormat = DateTime.Now.ToString("HH:mm:ss");
            //if (Settings.Get<bool>("Debug"))
            //    dateFormat = DateTime.Now.ToString("HH:mm:ss:fff");

            switch (level)
            {
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"[{dateFormat}] (INFO) {message}");
                    Log(string.Concat($"[{dateFormat}] ", message));
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"[{dateFormat}] (ATTENTION) {message}");
                    Log(string.Concat($"[{dateFormat}] ", message));
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{dateFormat}] (ERROR) {message}");
                    Log(string.Concat($"[{dateFormat}] ", message));
                    break;
                case LogLevel.Debug:
                    if (Settings.Get<bool>("Debug"))
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine($"[{dateFormat}] (DEBUG) {message}");
                        Log(string.Concat($"[{dateFormat}] ", message));
                    }
                    break;
                case LogLevel.None:
                    Console.ForegroundColor = color;
                    Console.WriteLine($"[{dateFormat}] {message}");
                    Log(string.Concat($"[{dateFormat}] ", message));
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"[{dateFormat}] {message}");
                    Log(string.Concat($"[{dateFormat}] ", message));
                    break;
            }
        }

        private static void Log(string message)
        {
            if (File.Exists(Path.Combine(FoldersFilesAndPaths.Logs, CurrentFile + ".txt")))
            {
                long size = new FileInfo(Path.Combine(FoldersFilesAndPaths.Logs, CurrentFile + ".txt")).Length;
                if (size >= 52428800)
                    SetLogger();
            }

            using (var log = File.AppendText(Path.Combine(FoldersFilesAndPaths.Logs, CurrentFile + ".txt")))
            {
                log.WriteLine(message);
                log.Flush();
            }
        }
    }

    public enum LogLevel
    {
        None = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Debug = 4
    }
}
