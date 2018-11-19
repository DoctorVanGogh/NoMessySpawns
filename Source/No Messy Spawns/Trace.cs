using System.Diagnostics;

namespace SimpleFixes
{
    class Trace
    {
        [Conditional("TRACE")]
        public static void Log(string message) => Verse.Log.Message(message);
        [Conditional("TRACE")]
        public static void Warning(string message) => Verse.Log.Warning(message);

    }
}
