using System;

namespace Spider
{
    public static class ContextExtenders
    {
        public static ConsoleColor ToConsoleColor(this Context context)
        {
            switch (context)
            {
                case Context.BadHTML:
                case Context.BadMedia:
                    return ConsoleColor.Red;
                case Context.BadStatus:
                    return ConsoleColor.Cyan;
                case Context.GoodHTML:
                case Context.GoodMedia:
                    return ConsoleColor.Green;
                default:
                    return ConsoleColor.White;
            }
        }
    }
}
