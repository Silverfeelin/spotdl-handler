using System;

namespace SpotifyDownloader
{
    public static class ConsoleExtensions
    {
        public static void Write(ConsoleColor color, string text, params object[] args)
            => Color(color, () => Console.Write(text, args));
        
        public static void WriteLine(ConsoleColor color, string text, params object[] args)
            => Color(color, () => Console.WriteLine(text, args));

        public static void WriteLine(ConsoleColor color, object obj)
            => Color(color, () => Console.WriteLine(obj));

        private static void Color(ConsoleColor c, Action a)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = c;
            a();
            Console.ForegroundColor = oldColor;
        }
    }
}
