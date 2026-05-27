using System;
using System.Runtime.CompilerServices;
using IPALogger = IPA.Logging.Logger;

namespace SyncSaber
{
    internal static class Logger
    {
        internal static IPALogger Log { get; set; }

        public static void Info(string log, [CallerFilePath] string filepath = "", [CallerMemberName] string member = "", [CallerLineNumber] int? linenum = 0)
        {
#if DEBUG
            Logger.Log.Info($"[{Path.GetFileName(filepath)}] [{member}:({linenum})] : {log}");
#else
            Logger.Log.Info($"[{member}:({linenum})] : {log}");
#endif
        }
        public static void Error(string log, [CallerFilePath] string filepath = "", [CallerMemberName] string member = "", [CallerLineNumber] int? linenum = 0)
        {
#if DEBUG
            Logger.Log.Error($"[{Path.GetFileName(filepath)}] [{member}:({linenum})] : {log}");
#else
            Logger.Log.Error($"[{member}:({linenum})] : {log}");
#endif
        }

        public static void Error(Exception e, [CallerFilePath] string filepath = "", [CallerMemberName] string member = "", [CallerLineNumber] int? linenum = 0)
        {
#if DEBUG
            Logger.Log.Error($"[{Path.GetFileName(filepath)}] [{member}:({linenum})] : {e}\r\n{e.Message}");
#else
            Logger.Log.Error($"[{member}:({linenum})] : {e}\r\n{e.Message}");
#endif
        }

        public static void Notice(string log, [CallerFilePath] string filepath = "", [CallerMemberName] string member = "", [CallerLineNumber] int? linenum = 0)
        {
#if DEBUG
            Logger.Log.Notice($"[{Path.GetFileName(filepath)}] [{member}:({linenum})] : {log}");
#else
            Logger.Log.Notice($"[{member}:({linenum})] : {log}");
#endif
        }

        public static void Debug(string log, [CallerFilePath] string filepath = "", [CallerMemberName] string member = "", [CallerLineNumber] int? linenum = 0)
        {
#if DEBUG
            Logger.Log.Debug($"[{Path.GetFileName(filepath)}] [{member}:({linenum})] : {log}");
#endif
        }
    }
}
